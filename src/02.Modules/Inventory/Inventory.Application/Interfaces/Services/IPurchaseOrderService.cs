using Inventory.Application.DTOs.PurchaseOrders;
using Inventory.Domain.Enums;
using SharedKernel.Entities;

namespace Inventory.Application.Interfaces.Services;

public interface IPurchaseOrderService
{
    Task<Result<IEnumerable<PurchaseOrderDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<PurchaseOrderDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<PurchaseOrderDto>> CreateAsync(CreatePurchaseOrderDto createPurchaseOrder, CancellationToken cancellationToken = default);
    Task<Result<PurchaseOrderDto>> UpdateAsync(int id, UpdatePurchaseOrderDto updatePurchaseOrder, CancellationToken cancellationToken = default);
    Task<Result<PurchaseOrderDto>> ApprovePurchaseOrderAsync(int id, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<bool>> ExistAsync(int id, CancellationToken cancellationToken = default);
}



