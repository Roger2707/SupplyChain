using Inventory.Application.Interfaces.Repositories;
using Inventory.Domain.Entities.Products;
using Inventory.Infrastructure.Data;

namespace Inventory.Infrastructure.Repositories
{
    public class UoMRepository : Repository<UoM>, IUoMRepository
    {
        public UoMRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
