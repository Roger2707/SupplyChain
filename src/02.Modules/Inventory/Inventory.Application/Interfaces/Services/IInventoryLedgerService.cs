using Inventory.Application.DTOs.Inventory;
using SharedKernel.Entities;

namespace Inventory.Application.Interfaces.Services;

public interface IInventoryLedgerService
{
    Task<Result<IEnumerable<InventoryLedgerDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<InventoryLedgerDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}