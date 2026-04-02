using ECommerce.Application.DTOs.Baskets;
using ECommerce.Application.DTOs.Checkout;
using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities.Enums;
using ECommerce.Domain.Entities.Orders;
using SharedKernel.DTOs;
using SharedKernel.Entities;
using SharedKernel.Interfaces;
using System.Transactions;

namespace ECommerce.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IBasketService _basketService;
        private readonly IInventoryAdapterService _inventoryAdapterService;

        public OrderService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IBasketService basketService, IInventoryAdapterService inventoryAdapterService)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _basketService = basketService;
            _inventoryAdapterService = inventoryAdapterService;
        }

        #region GETs

        public async Task<Result<OrderDto>> GetOrderAsync(int orderId, CancellationToken cancellationToken)
        {
            var order = await _unitOfWork.OrderRepository.GetByIdAsync(orderId, cancellationToken);
            if (order == null)
                return Result<OrderDto>.Failure("Order not found !");
            return Result<OrderDto>.Success(MapToDto(order));
        }

        public async Task<Result<List<OrderDto>>> GetOrdersByUserAsync(CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId;
            var orders = await _unitOfWork.OrderRepository.GetOrdersWithLinesByUserId(userId, cancellationToken);
            return Result<List<OrderDto>>.Success(orders.Select(MapToDto).ToList());
        }

        #endregion

        #region Order / Checkout

        public async Task<Result<Order>> PlaceOrderAsync(OrderCreateDto orderCreateDto, CancellationToken cancellationToken)
        {
            ValidateOrderCreate(orderCreateDto.Address, orderCreateDto.BasketId);
            var basket = await ValidateBasket(cancellationToken);

            // 1. Create Order Header (in order to get OrderId)
            var order = await CreateOrderAsync(basket, orderCreateDto.Address, cancellationToken);

            // 2. Inventory Check & Reserve
            var reservesDto = await _inventoryAdapterService.ReserveFIFOAsync(basket.Items.Select(i => new FIFOItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                NeccessaryQty = i.Quantity,
                SourceId = order.Id,
                SourceType = "Order"
            }).ToList(), cancellationToken);

            // Map to ProductSellingPrice
            var productIds = reservesDto.Select(l => l.ProductId).ToList();
            var productsSellingPriceDic = await _inventoryAdapterService.GetProductsSellingPrice(productIds, cancellationToken);

            // 3. Create Order Items & Calculate Total
            order.Items = reservesDto.Select(r => new OrderItem
            {
                OrderId = order.Id,
                ProductId = r.ProductId,
                ProductName = basket.Items.First(i => i.ProductId == r.ProductId).ProductName,
                Quantity = r.ReservedQty,
                UnitCost = r.UnitCost,
                UnitPrice = productsSellingPriceDic.TryGetValue(r.ProductId, out var productSellingPrice) ? productSellingPrice.SellingPrice : 0,
            }).ToList();
            order.TotalAmount = order.Items.Sum(i => i.LineTotal);

            // 4. Update Order with Items & Total
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 5. Clear Basket
            await _basketService.ClearBasketAsync(cancellationToken);

            // 7. Map to DTO & Return
            return Result<Order>.Success(order);
        }

        public async Task ProcessCheckoutSuccessAsync(int orderId, string paymentIntentId, CancellationToken cancellationToken)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var order = await _unitOfWork.OrderRepository.GetWithLinesAsync(orderId, cancellationToken);

                    // Idempotency
                    if (order == null || order.OrderStatus == OrderStatus.Paid)
                        return;

                    order.OrderStatus = OrderStatus.Paid;
                    order.PaymentIntentId = paymentIntentId;
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    // Export Stock
                    await _inventoryAdapterService.DecreaseStockInLayers(orderId, cancellationToken);

                    scope.Complete();
                }
                catch (Exception ex)
                {
                    // If any exception occurs -> Transaction will Rollback
                    throw new Exception(ex.Message);
                }
            }   
        }

        #endregion

        #region CRUDs

        private async Task<Order> CreateOrderAsync(BasketDto basket, string orderAddress, CancellationToken cancellationToken)
        {
            var order = new Order();
            order.UserId = _currentUser.UserId;
            order.BasketId = basket.Id;
            order.Address = orderAddress;

            await _unitOfWork.OrderRepository.AddAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return order;
        }

        #endregion

        #region Helpers

        private void ValidateOrderCreate(string orderAddress, int basketId)
        {
            if (string.IsNullOrWhiteSpace(orderAddress))
                throw new Exception("Order address is required !");

            if (string.IsNullOrWhiteSpace(basketId.ToString()))
                throw new Exception("User doesn't have basket or basket is empty");
        }

        private async Task<BasketDto> ValidateBasket(CancellationToken cancellationToken = default)
        {
            var basketResult = await _basketService.GetByUserIdAsync(cancellationToken);
            if (!basketResult.IsSuccess)
                throw new Exception("Basket is NULL or EMPTY !");

            var basket = basketResult.Data;
            if (basket.Items.Count == 0)
                throw new Exception("Basket is NULL or EMPTY !");

            return basket;
        }

        private OrderDto MapToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                BasketId = order.BasketId,
                Address = order.Address,
                OrderStatus = order.OrderStatus,
                TotalAmount = order.TotalAmount,
                Items = order.Items.Select(item => new OrderItemDto
                {
                   Id = item.Id,
                   OrderId = item.OrderId,
                   ProductId = item.ProductId,
                   ProductName = item.ProductName,
                   Quantity = item.Quantity,
                   UnitCost = item.UnitCost,
                    UnitPrice = item.UnitPrice,
                   LineTotal = item.LineTotal
                }).ToList(),
            };
        }

        #endregion
    }
}
