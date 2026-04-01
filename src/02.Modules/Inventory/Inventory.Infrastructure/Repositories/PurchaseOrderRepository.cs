using Inventory.Application.Interfaces.Repositories;
using Inventory.Domain.Entities.PurchaseOrder;
using Inventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Repositories;

public class PurchaseOrderRepository : Repository<PurchaseOrder>, IPurchaseOrderRepository
{
    public PurchaseOrderRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PurchaseOrder>> GetAllWithLinesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(x => x.Lines)
            .ToListAsync(cancellationToken);
    }

    public async Task<PurchaseOrder?> GetWithLinesAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}

