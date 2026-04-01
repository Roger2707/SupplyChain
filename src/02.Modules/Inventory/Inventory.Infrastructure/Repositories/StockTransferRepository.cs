using Inventory.Application.Interfaces.Repositories;
using Inventory.Domain.Entities.StockTransfer;
using Inventory.Infrastructure.Data;

namespace Inventory.Infrastructure.Repositories;

public class StockTransferRepository : Repository<StockTransfer>, IStockTransferRepository
{
    public StockTransferRepository(ApplicationDbContext context) : base(context)
    {
    }
}

