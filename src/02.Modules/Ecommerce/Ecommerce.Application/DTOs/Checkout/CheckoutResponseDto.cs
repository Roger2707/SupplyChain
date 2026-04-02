using ECommerce.Application.DTOs.Orders;

namespace ECommerce.Application.DTOs.Checkout
{
    public class CheckoutResponseDto
    {
        public OrderDto OrderDto { get; set; }
        public string ClientSecret { get; set; }
        public string PaymentIntentId { get; set; }
    }
}
