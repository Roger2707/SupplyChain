using Inventory.Infrastructure.Data;
using SharedKernel.Repositories;

namespace Inventory.Infrastructure.Queries;

public sealed class DapperExecutor : EfDapperExecutorBase<InventoryDbContext>
{
    public DapperExecutor(InventoryDbContext db) : base(db)
    {
    }
}




