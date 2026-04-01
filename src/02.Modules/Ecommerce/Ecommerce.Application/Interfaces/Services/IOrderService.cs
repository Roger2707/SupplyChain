using ECommerce.Application.DTOs.Orders;
using SharedKernel.Entities;

namespace ECommerce.Application.Interfaces.Services
{
    public interface IOrderService
    {
        Task<Result<OrderDto>> GetOrderAsync(int orderId, CancellationToken cancellationToken);
        Task<Result<OrderDto>> PlaceOrderAsync(OrderCreateDto orderCreateDto, CancellationToken cancellationToken);
    }
}
