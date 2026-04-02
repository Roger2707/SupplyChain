using ECommerce.Application.DTOs.Checkout;
using ECommerce.Application.DTOs.Orders;
using SharedKernel.Entities;

namespace ECommerce.Application.Interfaces.Services
{
    public interface ICheckoutService
    {
        Task<Result<CheckoutResponseDto>> CheckoutAsync(OrderCreateDto dto, CancellationToken cancellationToken);
    }
}
