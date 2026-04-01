using Inventory.Application.DTOs.GoodsReceipts;
using SharedKernel.Entities;

namespace Inventory.Application.Interfaces.Services;

public interface IGoodsReceiptService
{
    Task<Result<IEnumerable<GoodsReceiptDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<GoodsReceiptDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<GoodsReceiptDto>> CreateAsync(CreateGoodsReceiptDto createGoodsReceipt, CancellationToken cancellationToken = default);
    Task<Result<GoodsReceiptDto>> UpdateAsync(int id, UpdateGoodsReceiptDto updateGoodsReceipt, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<Result> PostAsync(int id, CancellationToken cancellationToken = default);
    Task<Result> CancelAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<bool>> ExistAsync(int id, CancellationToken cancellationToken = default);
}




