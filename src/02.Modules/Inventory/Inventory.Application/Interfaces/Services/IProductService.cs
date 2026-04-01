using Inventory.Application.Common.Pagination;
using Inventory.Application.DTOs.Products;
using Inventory.Domain.Entities.Products;
using SharedKernel.DTOs;
using SharedKernel.Entities;

namespace Inventory.Application.Interfaces.Services
{
    public interface IProductService
    {
        Task<Result<List<ProductDto>>> GetAllAsync(CancellationToken cancellationToken);
        Task<Result<ProductDto>> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<Result<PagedResult<ProductDto>>> GetProductsPagedAsync(ProductParams param, CancellationToken cancellationToken);
        Task<Dictionary<int, ProductSellingPrice>> GetProductsSellingPrice(List<int> productIds, CancellationToken cancellationToken = default);
        Task<ProductSellingPrice> GetProductSellingPrice(int productId, CancellationToken cancellationToken = default);

        Task<Result<ProductDto>> CreateAsync(CreateProductDto createProductDto, CancellationToken cancellationToken);
        Task<Result<ProductDto>> UpdateAsync(int id, UpdateProductDto createProductDto, CancellationToken cancellationToken);
        Task<Result> DeleteAsync(int id, CancellationToken cancellationToken);
        Task<Result<bool>> ExistAsync(int id, CancellationToken cancellationToken);

        // 2026/04/01
        Task SetProductSellingPrice(int productId, CancellationToken cancellationToken);
        Task<List<Product>> GetProductsByIdsAsync(List<int> productIds, CancellationToken cancellationToken);
    }
}


