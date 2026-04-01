using SharedKernel.Entities;

namespace ECommerce.Domain.Entities.Baskets
{
    public class Basket : BaseEntity
    {
        public int UserId { get; set; }
        public List<BasketItem> Items { get; set; } = new();
    }
}
