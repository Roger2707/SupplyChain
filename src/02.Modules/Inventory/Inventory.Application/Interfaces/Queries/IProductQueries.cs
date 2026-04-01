using Inventory.Application.Common.Pagination;
using Inventory.Application.DTOs.Products;

namespace Inventory.Application.Interfaces.Queries
{
    public interface IProductQueries
    {
        Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken);
        Task<ProductDto> GetByIdAsync(int id, CancellationToken cancellationToken);
    }
}

