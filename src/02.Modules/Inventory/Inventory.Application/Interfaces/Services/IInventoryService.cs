using SharedKernel.DTOs;

namespace Inventory.Application.Interfaces.Services
{
    public interface IInventoryService
    {
        Task<List<ReserveDto>> ReserveFIFOAsync(List<FIFOItemDto> items, CancellationToken cancellationToken = default);
    }
}