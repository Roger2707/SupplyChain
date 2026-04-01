using Inventory.Domain.Entities.PurchaseOrder;

namespace Inventory.Application.Interfaces.Repositories;

public interface IPurchaseOrderRepository : IRepository<PurchaseOrder>
{
    Task<IEnumerable<PurchaseOrder>> GetAllWithLinesAsync(CancellationToken cancellationToken = default);
    Task<PurchaseOrder> GetWithLinesAsync(int id, CancellationToken cancellationToken = default);
}

