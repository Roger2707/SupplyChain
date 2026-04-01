using Inventory.Domain.Entities.Suppliers;

namespace Inventory.Application.Interfaces.Repositories;

public interface ISupplierProductPriceRepository : IRepository<SupplierProductPrice>
{
    Task<decimal> GetTopUnitPrice(int supplierId, int productId, CancellationToken cancellationToken = default);
}

