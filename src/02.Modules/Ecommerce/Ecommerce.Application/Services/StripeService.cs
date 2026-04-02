using ECommerce.Application.DTOs.Stripes;
using ECommerce.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace ECommerce.Application.Services
{
    public class StripeService : IStripeService
    {
        private readonly IConfiguration _configuration;

        public StripeService(IConfiguration configuration)
        {
            _configuration = configuration;
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }
        public async Task<PaymentIntentResponse> CreatePaymentIntentAsync(long amount, string currency = "vnd", string orderId = "")
        {
            var options = new PaymentIntentCreateOptions
            {
                // VND: 500000 = 500,000 VND
                // USD: 500000 = 5,000.00 USD (times 100)
                Amount = amount,
                Currency = currency.ToLower(),
                PaymentMethodTypes = new List<string> { "card" },
                Metadata = new Dictionary<string, string>
                {
                    { "OrderId", orderId } // For Webhook identify Order 
                }
            };

            var service = new PaymentIntentService();
            var intent = await service.CreateAsync(options);

            return new PaymentIntentResponse
            {
                ClientSecret = intent.ClientSecret,
                PaymentIntentId = intent.Id
            };
        }
    }
}
