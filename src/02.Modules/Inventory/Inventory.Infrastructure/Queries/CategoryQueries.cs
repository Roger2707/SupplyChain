using Dapper;
using Inventory.Application.DTOs.Categories;
using Inventory.Application.Interfaces.Queries;
using SharedKernel.Repositories;

namespace Inventory.Infrastructure.Queries
{
    public class CategoryQueries : ICategoryQueries
    {
        private readonly IDapperExecutor _dapper;

        public CategoryQueries(IDapperExecutor dapper)
        {
            _dapper = dapper;
        }

        public async Task<IReadOnlyList<CategoryDto>> GetCategoriesWithSubTree(CancellationToken cancellationToken)
        {
            const string sql = """
                           WITH CategoryTree AS
                           (
                           	SELECT 
                           		c.Id
                           		, c.Name
                           		, c.ParentId
                           		, c.CreatedAt
                           		, c.UpdatedAt
                           	FROM Categories c
                           	WHERE c.ParentId IS null

                           	UNION ALL

                           	SELECT 
                           		c.Id
                           		, c.Name
                           		, c.ParentId
                           		, c.CreatedAt
                           		, c.UpdatedAt
                           	FROM Categories c
                           	INNER JOIN CategoryTree ct ON ct.Id = c.ParentId
                           )

                           SELECT * FROM CategoryTree
                           """;

            var rows = await _dapper.QueryAsync<CategoryRow>(sql, cancellationToken: cancellationToken);
            var categories = BuildTree(rows.AsList());

            return categories;
        }

        public async Task<CategoryDto> GetCategoryWithSubTree(int categoryId, CancellationToken cancellationToken = default)
        {
            const string sql = """
                           WITH CategoryTree AS
                           (
                           	SELECT 
                           		c.Id
                           		, c.Name
                           		, c.ParentId
                           		, c.CreatedAt
                           		, c.UpdatedAt
                           	FROM Categories c
                           	WHERE c.Id = @CategoryId

                           	UNION ALL

                           	SELECT 
                           		c.Id
                           		, c.Name
                           		, c.ParentId
                           		, c.CreatedAt
                           		, c.UpdatedAt
                           	FROM Categories c
                           	INNER JOIN CategoryTree ct ON ct.Id = c.ParentId
                           )

                           SELECT * FROM CategoryTree
                           """;

            var rows = await _dapper.QueryAsync<CategoryRow>(sql, new { CategoryId = categoryId }, cancellationToken: cancellationToken);
            var categories = BuildTree(rows.AsList(), categoryId);
            return categories[0];
        }

        private List<CategoryDto> BuildTree(List<CategoryRow> categories, int? parentId = null)
        {
            var lookup = categories.ToLookup(c => c.ParentId);

            List<CategoryDto> Build(int? parentId)
            {
                return lookup[parentId]
                    .Select(c => new CategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        CategoryNodes = Build(c.Id)
                    })
                    .ToList();
            }

            return Build(parentId);
        }

        private sealed class CategoryRow
        {
            public int Id { get; init; }
            public string Name { get; init; }
            public int? ParentId { get; set; }
            public DateTime CreatedAt { get; init; }
            public DateTime? UpdatedAt { get; init; }
        }
    }
}
