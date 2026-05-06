using ECommerce.Application.Interfaces.Services;
using Inventory.Application.Interfaces.Services;
using SharedKernel.DTOs;

namespace ECommerce.Application.Services
{
    public class InventoryAdapterService : IInventoryAdapterService
    {
        private readonly IInventoryService _inventoryService;
        private readonly IProductService _productService;

        public InventoryAdapterService(IInventoryService inventoryService, IProductService productService)
        {
            _inventoryService = inventoryService;
            _productService = productService;
        }

        public async Task<Dictionary<int, ProductSellingPrice>> GetProductsSellingPrice(List<int> productIds, CancellationToken cancellationToken = default)
        {
            return await _productService.GetProductsSellingPrice(productIds, cancellationToken);
        }

        public async Task<ProductSellingPrice> GetProductSellingPrice(int productId, CancellationToken cancellationToken = default)
        {
            return await _productService.GetProductSellingPrice(productId, cancellationToken);
        }

        public async Task<List<ReserveDto>> ReserveFIFOAsync(List<FIFOItemDto> items, CancellationToken cancellationToken = default)
        {
            return await _inventoryService.ReserveFIFOAsync(items, cancellationToken);
        }

        public async Task ExportStockInLayers(int orderId, CancellationToken cancellationToken = default)
        {
            await _inventoryService.ExportStockInLayers(orderId, cancellationToken);
        }

        public async Task ReleaseReserveQtyInLayers(int orderId, CancellationToken cancellationToken = default)
        {
            await _inventoryService.ReleaseReserveQtyInLayers(orderId, cancellationToken);
        }
    }
}
