using Inventory.Application.Interfaces.Repositories;
using Inventory.Domain.Entities.Inventory;
using Inventory.Infrastructure.Data;

namespace Inventory.Infrastructure.Repositories;

public class InventoryLedgerRepository : Repository<InventoryLedger>, IInventoryLedgerRepository
{
    public InventoryLedgerRepository(ApplicationDbContext context) : base(context)
    {
    }
}

