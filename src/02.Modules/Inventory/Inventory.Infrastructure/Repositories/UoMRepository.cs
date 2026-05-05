using Inventory.Application.Interfaces.Repositories;
using Inventory.Domain.Entities.Products;
using Inventory.Infrastructure.Data;

namespace Inventory.Infrastructure.Repositories
{
    public class UoMRepository : Repository<UoM>, IUoMRepository
    {
        public UoMRepository(InventoryDbContext context) : base(context)
        {
        }
    }
}
