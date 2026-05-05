using Inventory.Application.Interfaces.Repositories;
using Inventory.Domain.Entities.Inventory;
using Inventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Repositories
{
    public class InventoryReservationRepository : Repository<InventoryReservation>, IInventoryReservationRepository
    {
        public InventoryReservationRepository(InventoryDbContext context) : base(context)
        {
        }

        public async Task<List<InventoryReservation>> GetReservationBySource(int sourceId, string sourceType, CancellationToken cancellationToken)
        {
            var reservations = await _context.InventoryReservations
                .Where(i => i.SourceId == sourceId && i.SourceType == sourceType)
                .ToListAsync(cancellationToken);
            return reservations;
        }
    }
}