using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities.Baskets;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories
{
    public class BasketRepository : Repository<Basket>, IBasketRepository
    {
        public BasketRepository(ECommerceDbContext context) : base(context)
        {
        }

        public async Task<Basket> GetByBasketIdAsync(int basketId, CancellationToken cancellationToken = default)
        {
            return await _context.Baskets
                 .Include(x => x.Items)
                 .FirstOrDefaultAsync(x => x.UserId == basketId, cancellationToken);
        }

        public async Task<Basket> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _context.Baskets
                            .Include(x => x.Items)
                            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        }
    }
}
