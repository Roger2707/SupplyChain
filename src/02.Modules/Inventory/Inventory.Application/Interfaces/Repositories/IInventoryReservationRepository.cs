using Inventory.Domain.Entities.Inventory;

namespace Inventory.Application.Interfaces.Repositories
{
    public interface IInventoryReservationRepository : IRepository<InventoryReservation>
    {
        Task<List<InventoryReservation>> GetReservationBySource(int sourceId, string sourceType, CancellationToken cancellationToken);
    }
}