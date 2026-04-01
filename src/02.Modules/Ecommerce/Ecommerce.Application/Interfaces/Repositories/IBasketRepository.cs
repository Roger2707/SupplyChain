using ECommerce.Domain.Entities.Baskets;

namespace ECommerce.Application.Interfaces.Repositories
{
    public interface IBasketRepository : IRepository<Basket>
    {
        Task<Basket?> GetByBasketIdAsync(int basketId, CancellationToken cancellationToken = default);
        Task<Basket?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    }
}