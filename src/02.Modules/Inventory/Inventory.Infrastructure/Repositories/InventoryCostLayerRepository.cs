using Inventory.Application.Interfaces.Repositories;
using Inventory.Domain.Entities.Inventory;
using Inventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Repositories;

public class InventoryCostLayerRepository : Repository<InventoryCostLayer>, IInventoryCostLayerRepository
{
    public InventoryCostLayerRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<InventoryCostLayer>> GetAvailableLayersByProductId(int productId, CancellationToken cancellationToken = default)
    {
        var layers = await _context.InventoryCostLayers
            .Where(i => i.ProductId == productId && i.RemainingQty - i.ReservedQty > 0)
            .OrderBy(i => i.ReceiptDate)
            .ToListAsync(cancellationToken);

        return layers;
    }

    public async Task<List<InventoryCostLayer>> GetLayersByProductId(int productId, CancellationToken cancellationToken = default)
    {
        var layers = await _context.InventoryCostLayers
            .Where(i => i.ProductId == productId)
            .OrderBy(i => i.ReceiptDate)
            .ToListAsync(cancellationToken);

        return layers;
    }

    public async Task<List<InventoryCostLayer>> GetByIdsAsync(IEnumerable<int> layerIds, CancellationToken cancellationToken = default)
    {
        return await _context.InventoryCostLayers
            .Where(x => layerIds.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }
}

