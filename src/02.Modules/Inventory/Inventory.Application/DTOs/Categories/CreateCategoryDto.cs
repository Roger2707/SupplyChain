namespace Inventory.Application.DTOs.Categories
{
    public class CreateCategoryDto
    {
        public string Name { get; set; }
        public int? ParentId { get; set; } = null;
    }
}
