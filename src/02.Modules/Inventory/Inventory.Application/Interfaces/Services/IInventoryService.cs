using SharedKernel.DTOs;

namespace Inventory.Application.Interfaces.Services
{
    public interface IInventoryService
    {
        Task<List<ReserveDto>> ReserveFIFOAsync(List<FIFOItemDto> items, CancellationToken cancellationToken = default);

        Task DecreaseStockInLayers(int orderId, CancellationToken cancellationToken = default);
        Task CancelReserveStockInLayers(int orderId, CancellationToken cancellationToken = default);
    }
}