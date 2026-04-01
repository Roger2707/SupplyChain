using Inventory.Domain.Entities.Suppliers;
using SharedKernel.Entities;

namespace Inventory.Application.Interfaces.Services;

public interface ISupplierProductPriceService
{
    Task<Result<IEnumerable<SupplierProductPrice>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<SupplierProductPrice>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}



