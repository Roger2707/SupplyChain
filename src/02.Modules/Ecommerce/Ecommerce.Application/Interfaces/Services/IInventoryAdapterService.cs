using SharedKernel.DTOs;

namespace ECommerce.Application.Interfaces.Services
{
    public interface IInventoryAdapterService
    {
        Task<List<ReserveDto>> ReserveFIFOAsync(List<FIFOItemDto> items, CancellationToken cancellationToken = default);
        Task<Dictionary<int, ProductSellingPrice>> GetProductsSellingPrice(List<int> productIds, CancellationToken cancellationToken = default);
        Task<ProductSellingPrice> GetProductSellingPrice(int productId, CancellationToken cancellationToken = default);
    }
}