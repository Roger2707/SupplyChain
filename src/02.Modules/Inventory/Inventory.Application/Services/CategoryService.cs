using Inventory.Application.DTOs.Categories;
using Inventory.Application.Interfaces.Queries;
using Inventory.Application.Interfaces.Repositories;
using Inventory.Application.Interfaces.Services;
using Inventory.Domain.Entities.Products;
using SharedKernel.Entities;

namespace Inventory.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICategoryQueries _categoryQueries;

        public CategoryService(IUnitOfWork unitOfWork, ICategoryQueries categoryQueries)
        {
            _unitOfWork = unitOfWork;
            _categoryQueries = categoryQueries;
        }

        public async Task<Result<IEnumerable<CategoryDto>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var categories = await _categoryQueries.GetCategoriesWithSubTree(cancellationToken);
            return Result<IEnumerable<CategoryDto>>.Success(categories);
        }

        public async Task<Result<CategoryDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var category = await _categoryQueries.GetCategoryWithSubTree(id, cancellationToken);
            if (category == null)
            {
                return Result<CategoryDto>.Failure($"Category with ID {id} not found.");
            }
            return Result<CategoryDto>.Success(category);
        }

        public async Task<Result<bool>> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            var exist = await _unitOfWork.CategoryRepository.ExistsAsync(c => c.Id == id, cancellationToken);
            return Result<bool>.Success(exist);
        }

        public async Task<Result<CategoryDto>> CreateAsync(CreateCategoryDto createDto, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(createDto.Name))
                return Result<CategoryDto>.Failure("Category Name cannot be empty.");

            var category = new Category
            {
                Name = createDto.Name,
                ParentId = createDto.ParentId
            };

            await _unitOfWork.CategoryRepository.AddAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var categoryDto = MapToDto(category);
            return Result<CategoryDto>.Success(categoryDto);
        }

        public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var existedCategory = await _unitOfWork.CategoryRepository.GetByIdAsync(id, cancellationToken);
            if (existedCategory == null)
            {
                return Result.Failure($"Category with ID {id} not found.");
            }

            existedCategory.IsDeleted = true;

            await _unitOfWork.CategoryRepository.UpdateAsync(existedCategory, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

        public async Task<Result<CategoryDto>> UpdateAsync(int id, UpdateCategoryDto updateDto, CancellationToken cancellationToken = default)
        {
            var existedCategory = await _unitOfWork.CategoryRepository.GetByIdAsync(id, cancellationToken);
            if(existedCategory == null)
            {
                return Result<CategoryDto>.Failure($"Category with ID {id} not found.");
            }

            existedCategory.Name = updateDto.Name;

            await _unitOfWork.CategoryRepository.UpdateAsync(existedCategory, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var categoryDto = MapToDto(existedCategory);
            return Result<CategoryDto>.Success(categoryDto);
        }

        private CategoryDto MapToDto(Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }
    }
}


