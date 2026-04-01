using Inventory.Domain.Entities.StockTransfer;
using SharedKernel.Entities;

namespace Inventory.Application.Interfaces.Services;

public interface IStockTransferService
{
    Task<Result<IEnumerable<StockTransfer>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<StockTransfer>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}



