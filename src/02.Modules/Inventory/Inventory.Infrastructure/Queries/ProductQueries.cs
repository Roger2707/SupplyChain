using Inventory.Application.DTOs.Products;
using Inventory.Application.Interfaces.Queries;
using SharedKernel.Repositories;


namespace Inventory.Infrastructure.Queries
{
    public class ProductQueries : IProductQueries
    {
        private readonly IDapperExecutor _dapper;

        public ProductQueries(IDapperExecutor dapper)
        {
            _dapper = dapper;
        }

        public async Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken)
        {
            const string query = @"
                                    SELECT
	                                    p.Id
	                                    , p.Name
	                                    , p.SKU
	                                    , p.Barcode
	                                    , p.CategoryId
	                                    , ct.Name as CategoryName
	                                    , p.BaseUoMId
	                                    , u_base.Name as BaseUoMName
                                        , p.SellingPrice
	                                    , p.MinStockLevel
	                                    , p.IsDeleted
	                                    , p.IsPerishable
                                        , p.IsActive
	                                    , p.CreatedAt
	                                    , p.UpdatedAt

	                                    -- conversion

	                                    , c.FromUoMId as UoMIdFrom
	                                    , u_from.Name as UoMNameFrom
	                                    , c.ToUoMId as UoMIdTo
	                                    , u_to.Name as UoMNameTo
	                                    , c.Factor

                                    FROM Products p

                                    LEFT JOIN Categories ct ON ct.Id = p.CategoryId
                                    LEFT JOIN ProductUoMConversions c ON c.ProductId = p.Id
                                    LEFT JOIN UoMs u_base ON u_base.Id = p.BaseUoMId
                                    LEFT JOIN UoMs u_from ON u_from.Id = c.FromUoMId
                                    LEFT JOIN UoMs u_to ON u_to.Id = c.ToUoMId

                                    ORDER BY p.Id ASC
                                ";
            
            var product_rows = await _dapper.QueryAsync<ProductRow>(query);

            var productDtos = product_rows
                .GroupBy(x => x.Id)
                .Select(g =>
                {
                    var first = g.First();

                    return new ProductDto
                    {
                        Id = first.Id,
                        Name = first.Name,
                        SKU = first.SKU,
                        Barcode = first.Barcode,
                        CategoryId = first.CategoryId,
                        CategoryName = first.CategoryName,
                        BaseUoMId = first.BaseUoMId,
                        BaseUoMName = first.BaseUoMName,
                        MinStockLevel = first.MinStockLevel,
                        SellingPrice = first.SellingPrice,
                        IsPerishable = first.IsPerishable,
                        IsActive = first.IsActive,
                        CreatedAt = first.CreatedAt,
                        UpdatedAt = first.UpdatedAt,

                        ConversionDtos = g
                            .Where(x => x.UoMIdFrom != 0)
                            .Select(x => new ProductConversionDto
                            {
                                ProductId = first.Id,
                                ProductName = first.Name,
                                UoMIdFrom = x.UoMIdFrom,
                                UoMNameFrom = x.UoMNameFrom,
                                UoMIdTo = x.UoMIdTo,
                                UoMNameTo = x.UoMNameTo,
                                Factor = x.Factor
                            })
                            .ToList()
                    };
                })
                .OrderBy(x => x.Id)
                .ToList();

            return productDtos;
        }

        public async Task<ProductDto> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            const string query = @"
                                    SELECT
	                                    p.Id
	                                    , p.Name
	                                    , p.SKU
	                                    , p.Barcode
	                                    , p.CategoryId
	                                    , ct.Name as CategoryName
	                                    , p.BaseUoMId
	                                    , u_base.Name as BaseUoMName
                                        , p.BasePrice
	                                    , p.MinStockLevel
	                                    , p.IsDeleted
                                        , p.IsActive
	                                    , p.IsPerishable
	                                    , p.CreatedAt
	                                    , p.UpdatedAt

	                                    -- conversion

	                                    , c.FromUoMId as UoMIdFrom
	                                    , u_from.Name as UoMNameFrom
	                                    , c.ToUoMId as UoMIdTo
	                                    , u_to.Name as UoMNameTo
	                                    , c.Factor

                                    FROM Products p

                                    LEFT JOIN Categories ct ON ct.Id = p.CategoryId
                                    LEFT JOIN ProductUoMConversions c ON c.ProductId = p.Id
                                    LEFT JOIN UoMs u_base ON u_base.Id = p.BaseUoMId
                                    LEFT JOIN UoMs u_from ON u_from.Id = c.FromUoMId
                                    LEFT JOIN UoMs u_to ON u_to.Id = c.ToUoMId

                                    WHERE p.Id = @Id

                                    ORDER BY p.Id ASC
                                ";

            var product_rows = await _dapper.QueryAsync<ProductRow>(query, new {Id = id});

            var productDtos = product_rows
                .GroupBy(x => x.Id)
                .Select(g =>
                {
                    var first = g.First();

                    return new ProductDto
                    {
                        Id = first.Id,
                        Name = first.Name,
                        SKU = first.SKU,
                        Barcode = first.Barcode,
                        CategoryId = first.CategoryId,
                        CategoryName = first.CategoryName,
                        BaseUoMId = first.BaseUoMId,
                        BaseUoMName = first.BaseUoMName,
                        SellingPrice = first.SellingPrice,
                        MinStockLevel = first.MinStockLevel,
                        IsPerishable = first.IsPerishable,
                        IsActive = first.IsActive,
                        CreatedAt = first.CreatedAt,
                        UpdatedAt = first.UpdatedAt,

                        ConversionDtos = g
                            .Where(x => x.UoMIdFrom != 0)
                            .Select(x => new ProductConversionDto
                            {
                                ProductId = first.Id,
                                ProductName = first.Name,
                                UoMIdFrom = x.UoMIdFrom,
                                UoMNameFrom = x.UoMNameFrom,
                                UoMIdTo = x.UoMIdTo,
                                UoMNameTo = x.UoMNameTo,
                                Factor = x.Factor
                            })
                            .ToList()
                    };
                })
                .ToList();

            return productDtos[0];
        }

        private sealed class ProductRow
        {
            public int Id { get; init; }
            public string Name { get; init; }
            public string SKU { get; set; }
            public string Barcode { get; set; }

            public int CategoryId { get; set; }
            public string CategoryName { get; set; }

            public int BaseUoMId { get; set; }
            public string BaseUoMName { get; set; }
            public decimal SellingPrice { get; set; }
            public decimal MinStockLevel { get; set; }
            public bool IsPerishable { get; set; }
            public bool IsActive { get; set; }

            public DateTime CreatedAt { get; init; }
            public DateTime? UpdatedAt { get; init; }

            public int UoMIdFrom { get; set; }
            public string UoMNameFrom { get; set; }
            public int UoMIdTo { get; set; }
            public string UoMNameTo { get; set; }
            public decimal Factor { get; set; }
        }
    }
}
