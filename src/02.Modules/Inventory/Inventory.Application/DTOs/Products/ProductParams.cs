using Inventory.Application.Common.Pagination;

namespace Inventory.Application.DTOs.Products
{
    public class ProductParams : PaginationParam
    {
        public string? Name { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public List<int>? CategoryIds { get; set; }
    }
}
