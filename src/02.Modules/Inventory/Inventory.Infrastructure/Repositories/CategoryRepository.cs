using Inventory.Application.Interfaces.Repositories;
using Inventory.Domain.Entities.Products;
using Inventory.Infrastructure.Data;

namespace Inventory.Infrastructure.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
