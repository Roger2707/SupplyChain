using Inventory.Application.DTOs.Inventory;
using SharedKernel.Entities;

namespace Inventory.Application.Interfaces.Services;

public interface IInventoryCostLayerService
{
    Task<Result<IEnumerable<InventoryCostLayerDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<InventoryCostLayerDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}




