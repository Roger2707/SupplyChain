using ECommerce.Application.DTOs.Baskets;
using ECommerce.Application.DTOs.Checkout;
using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities.Enums;
using ECommerce.Domain.Entities.Orders;
using MassTransit;
using SharedKernel.Contracts;
using SharedKernel.Entities;
using System.Transactions;

namespace ECommerce.Application.Services
{
    public class CheckoutService : ICheckoutService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderService _orderService;
        private readonly IInventoryAdapterService _inventoryAdapterService;
        private readonly IBasketService _basketService;
        private readonly IStripeService _stripeService;
        private readonly IMessageScheduler _scheduler;
        private readonly IPublishEndpoint _publishEndpoint;

        public CheckoutService(IUnitOfWork unitOfWork, IOrderService orderService, IInventoryAdapterService inventoryAdapterService, IBasketService basketService, IStripeService stripeService, IMessageScheduler scheduler, IPublishEndpoint publishEndpoint)
        {
            _unitOfWork = unitOfWork;
            _orderService = orderService;
            _inventoryAdapterService = inventoryAdapterService;
            _basketService = basketService;
            _stripeService = stripeService;
            _scheduler = scheduler;
            _publishEndpoint = publishEndpoint;
        }


        #region CheckOut Methods

        public async Task<Result<CheckoutResponseDto>> CheckoutAsync(OrderCreateDto dto, CancellationToken ct)
        {
            try
            {
                // 1. Validate Basket - because Order need BasketId
                var basket = await ValidateBasket(ct);
                if (basket == null)
                    throw new Exception($"Can not find basket !");

                // 2. Create Order
                var orderResult = await _orderService.PlaceOrderAsync(dto, ct);
                if (!orderResult.IsSuccess) 
                    return Result<CheckoutResponseDto>.Failure(orderResult.ErrorMessage);
                var order = orderResult.Data;

                // 3. Publish Event "OrderCreated" to Outbox
                await _publishEndpoint.Publish(new OrderCreated(
                    order.Id,
                    basket.Items.Select(i => new ReserveItemDto(i.ProductId, i.ProductName, i.Quantity)).ToList()
                ), ct);

                // 4. SaveChanges and Commit
                await _unitOfWork.SaveChangesAsync(ct);

                // 5. Create PaymentIntent with Stripe
                var intent = await _stripeService.CreatePaymentIntentAsync(
                    (long)basket.TotalAmount,
                    "vnd",
                    order.Id.ToString()
                );

                // 6. Update Order (Tracked)
                order.PaymentIntentId = intent.PaymentIntentId;
                order.ClientSecret = intent.ClientSecret;
                await _unitOfWork.SaveChangesAsync(ct);

                // 6. Schedule a message to check payment status after 30 second (Eventual Consistency)
                await _scheduler.SchedulePublish(
                    DateTime.UtcNow.AddSeconds(30),
                    new OrderPaymentTimeoutCheck(order.Id)
                );

                return Result<CheckoutResponseDto>.Success(
                    new CheckoutResponseDto
                    {
                        OrderDto = MapToDto(order),
                        ClientSecret = intent.ClientSecret,
                        PaymentIntentId = intent.PaymentIntentId,
                    }
                );
            }
            catch (Exception ex)
            {
                return Result<CheckoutResponseDto>.Failure(ex.Message);
            }
        }

        public async Task CheckoutSuccessAsync(int orderId, string paymentIntentId, CancellationToken cancellationToken)
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
                    await _inventoryAdapterService.ExportStockInLayers(orderId, cancellationToken);

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

        #region Helpers

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
