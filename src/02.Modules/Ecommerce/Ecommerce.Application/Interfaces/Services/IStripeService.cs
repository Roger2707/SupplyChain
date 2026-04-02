using ECommerce.Application.DTOs.Stripes;

namespace ECommerce.Application.Interfaces.Services
{
    public interface IStripeService
    {
        Task<PaymentIntentResponse> CreatePaymentIntentAsync(long amount, string currency = "vnd", string orderId = "");
    }
}
