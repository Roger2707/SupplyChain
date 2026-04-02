using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities.Orders;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(ECommerceDbContext context) : base(context)
        {
        }

        public async Task<List<Order>> GetAllWithLinesAsync(CancellationToken cancellationToken = default)
        {
            var orders = await _context.Orders.Include(o => o.Items).ToListAsync(cancellationToken);
            return orders;
        }

        public async Task<Order> GetWithLinesAsync(int orderId, CancellationToken cancellationToken = default)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            return order;
        }

        public async Task<List<Order>> GetOrdersWithLinesByUserId(int userId, CancellationToken cancellationToken = default)
        {
            var orders = await _context.Orders.Where(o => o.UserId == userId).Include(o => o.Items).ToListAsync(cancellationToken);
            return orders;
        }
    }
}
