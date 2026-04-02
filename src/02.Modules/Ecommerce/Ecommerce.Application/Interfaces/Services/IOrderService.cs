using ECommerce.Application.DTOs.Orders;
using ECommerce.Domain.Entities.Orders;
using SharedKernel.Entities;

namespace ECommerce.Application.Interfaces.Services
{
    public interface IOrderService
    {
        Task<Result<OrderDto>> GetOrderAsync(int orderId, CancellationToken cancellationToken);
        Task<Result<List<OrderDto>>> GetOrdersByUserAsync(CancellationToken cancellationToken);
        Task<Result<Order>> PlaceOrderAsync(OrderCreateDto orderCreateDto, CancellationToken cancellationToken);
        Task ProcessCheckoutSuccessAsync(int orderId, string paymentIntentId, CancellationToken cancellationToken);
    }
}
