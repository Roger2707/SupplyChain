using Inventory.Application.Interfaces.Repositories;
using Inventory.Domain.Entities.Delivery;
using Inventory.Domain.Enums;
using Inventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Repositories
{
    public class DeliveryRepository : Repository<Delivery>, IDeliveryRepository
    {
        public DeliveryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Delivery>> GetAllWithLinesAsync(CancellationToken cancellationToken = default)
        {
            var deliveries = await _context.Deliveries.Include(d => d.Lines).ToListAsync();
            return deliveries;
        }

        public async Task<Delivery> GetWithLinesAsync(int id, CancellationToken cancellationToken = default)
        {
            var delivery = await _context.Deliveries
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
            return delivery;
        }

        public async Task<List<Delivery>> GetPostedDeliveriesWithLinesAsync(CancellationToken cancellationToken = default)
        {
            var deliveries = await _context.Deliveries.Include(d => d.Lines).Where(d => d.Status == DeliveryStatus.Posted).ToListAsync();
            return deliveries;
        }
    }
}
