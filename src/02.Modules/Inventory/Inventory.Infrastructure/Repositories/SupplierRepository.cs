using Inventory.Application.Interfaces.Repositories;
using Inventory.Domain.Entities.Suppliers;
using Inventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Repositories;

public class SupplierRepository : Repository<Supplier>, ISupplierRepository
{
    public SupplierRepository(ApplicationDbContext context) 
        : base(context)
    {
    }

    public async Task<Supplier?> GetByCodeAsync(string supplierCode, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(s => s.SupplierCode == supplierCode, cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(string supplierCode, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(s => s.SupplierCode == supplierCode, cancellationToken);
    }

    public async Task<List<string>> GetAllSupplierCodeAsync(CancellationToken cancellationToken)
    {
        var codes = await _dbSet.Select(w => w.SupplierCode).ToListAsync(cancellationToken);
        return codes;
    }
}

