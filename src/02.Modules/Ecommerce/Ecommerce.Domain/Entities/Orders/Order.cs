using ECommerce.Domain.Entities.Enums;
using SharedKernel.Entities;

namespace ECommerce.Domain.Entities.Orders
{
    public class Order : BaseEntity
    {
        public int UserId { get; set; }
        public int BasketId { get; set; }
        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;
        public string Address { get; set; }
        public decimal TotalAmount { get; set; } = 0;
        public string PaymentIntentId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public List<OrderItem> Items { get; set; } = new();
    }
}
