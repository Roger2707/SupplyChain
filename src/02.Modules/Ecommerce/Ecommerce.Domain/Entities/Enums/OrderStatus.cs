using StackExchange.Redis;

namespace ECommerce.Domain.Entities.Enums
{
    public enum OrderStatus
    {
        Pending,
        Paid,
        Shipping,
        Shipped,
    }
}
