using Inventory.Domain.Entities;

namespace Inventory.Application.Interfaces.Repositories;

public interface IWarehouseRepository : IRepository<Warehouse>
{
    Task<Warehouse> GetByCodeAsync(string warehouseCode, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string warehouseCode, CancellationToken cancellationToken = default);
    Task<IEnumerable<Warehouse>> GetActiveWarehousesAsync(CancellationToken cancellationToken = default);
    Task<List<string>> GetAllWarehouseCodeAsync(CancellationToken cancellationToken);
}

