using Inventory.Infrastructure.Data;
using SharedKernel.Repositories;

namespace Inventory.Infrastructure.Queries;

public sealed class DapperExecutor : EfDapperExecutorBase<ApplicationDbContext>
{
    public DapperExecutor(ApplicationDbContext db) : base(db)
    {
    }
}




