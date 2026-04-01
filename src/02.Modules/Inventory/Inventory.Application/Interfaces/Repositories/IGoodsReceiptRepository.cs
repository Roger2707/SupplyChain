using Inventory.Domain.Entities.GoodsReceipt;

namespace Inventory.Application.Interfaces.Repositories;

public interface IGoodsReceiptRepository : IRepository<GoodsReceipt>
{
    Task<IEnumerable<GoodsReceipt>> GetAllWithLinesAsync(CancellationToken cancellationToken = default);
    Task<GoodsReceipt> GetWithLinesAsync(int id, CancellationToken cancellationToken = default);
}

