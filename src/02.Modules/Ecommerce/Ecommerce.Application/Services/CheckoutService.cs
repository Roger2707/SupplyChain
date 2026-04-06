using ECommerce.Application.DTOs.Checkout;
using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
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
        private readonly IStripeService _stripeService;
        private readonly IMessageScheduler _scheduler;

        public CheckoutService(IUnitOfWork unitOfWork, IOrderService orderService, IStripeService stripeService, IMessageScheduler scheduler)
        {
            _unitOfWork = unitOfWork;
            _orderService = orderService;
            _stripeService = stripeService;
            _scheduler = scheduler;
        }

        public async Task<Result<CheckoutResponseDto>> CheckoutAsync(OrderCreateDto dto, CancellationToken ct)
        {
            // Create Transaction Scope to ensure all steps succeed or fail together (Atomicity)
            // If any step is error -> Transaction'll' Rollback
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    // 1. Create Order + Reserve Inventory and track Order Entity
                    var orderResult = await _orderService.PlaceOrderAsync(dto, ct);
                    if (!orderResult.IsSuccess) return Result<CheckoutResponseDto>.Failure(orderResult.ErrorMessage);
                    var order = orderResult.Data;

                    // 2. Create PaymentIntent with Stripe
                    var intent = await _stripeService.CreatePaymentIntentAsync(
                        (long)order.TotalAmount,
                        "vnd",
                        order.Id.ToString()
                    );

                    // 3. Update Order (Tracked)
                    order.PaymentIntentId = intent.PaymentIntentId;
                    order.ClientSecret = intent.ClientSecret;

                    await _unitOfWork.SaveChangesAsync(ct);

                    // 4. If all steps succeed -> Transaction will Commit
                    scope.Complete();

                    // 5. Schedule a message to check payment status after 15 minutes (Eventual Consistency)
                    await _scheduler.SchedulePublish(
                        DateTime.UtcNow.AddSeconds(30),
                        new OrderPaymentTimeoutCheck(order.Id)
                    );

                    return Result<CheckoutResponseDto>.Success(new CheckoutResponseDto
                    {
                        OrderDto = MapToDto(order),
                        ClientSecret = intent.ClientSecret,
                        PaymentIntentId = intent.PaymentIntentId,
                    });
                }
                catch (Exception ex)
                {
                    // If any exception occurs -> Transaction will Rollback
                    return Result<CheckoutResponseDto>.Failure(ex.Message);
                }
            }
        }

        #region Helpers

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
