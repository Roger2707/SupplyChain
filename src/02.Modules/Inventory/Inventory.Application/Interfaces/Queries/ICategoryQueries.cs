using Inventory.Application.DTOs.Categories;
using Inventory.Domain.Entities;

namespace Inventory.Application.Interfaces.Queries
{
    public interface ICategoryQueries
    {
        Task<IReadOnlyList<CategoryDto>> GetCategoriesWithSubTree(CancellationToken cancellationToken);
        Task<CategoryDto?> GetCategoryWithSubTree(int categoryId, CancellationToken cancellationToken = default);
    }
}
