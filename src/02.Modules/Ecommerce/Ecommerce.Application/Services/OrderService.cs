using ECommerce.Application.DTOs.Baskets;
using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities.Orders;
using SharedKernel.Entities;
using SharedKernel.Interfaces;

namespace ECommerce.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IBasketService _basketService;

        public OrderService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IBasketService basketService)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _basketService = basketService;
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

        #region Place Order

        public async Task<Result<Order>> PlaceOrderAsync(OrderCreateDto orderCreateDto, CancellationToken cancellationToken)
        {
            // 1. Validate
            ValidateOrderCreate(orderCreateDto.Address, orderCreateDto.BasketId);

            // 2. Create Order(Header)
            var order = await CreateOrderAsync(orderCreateDto.BasketId, orderCreateDto.Address, cancellationToken);

            // 3. Map to DTO & Return
            return Result<Order>.Success(order);
        }       

        #endregion

        #region CRUDs

        private async Task<Order> CreateOrderAsync(int basketId, string orderAddress, CancellationToken cancellationToken)
        {
            var order = new Order();
            order.UserId = _currentUser.UserId;
            order.BasketId = basketId;
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
