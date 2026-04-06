using StackExchange.Redis;

namespace ECommerce.Domain.Entities.Enums
{
    public enum OrderStatus
    {
        Pending,
        Paid,
        Cancelled,
        Shipping,
        Shipped,
    }
}
