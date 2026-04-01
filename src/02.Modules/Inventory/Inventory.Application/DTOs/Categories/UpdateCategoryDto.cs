namespace Inventory.Application.DTOs.Categories
{
    public class UpdateCategoryDto
    {
        public string Name { get; set; }
        public int? ParentId { get; set; } = null;
    }
}
