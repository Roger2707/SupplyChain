using Inventory.Application.DTOs.Categories;
using SharedKernel.Entities;

namespace Inventory.Application.Interfaces.Services
{
    public interface ICategoryService
    {
        Task<Result<IEnumerable<CategoryDto>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<CategoryDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<CategoryDto>> CreateAsync(CreateCategoryDto createDto, CancellationToken cancellationToken = default);
        Task<Result<CategoryDto>> UpdateAsync(int id, UpdateCategoryDto updateDto, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<bool>> ExistsAsync(int id, CancellationToken cancellationToken = default);
    }
}



