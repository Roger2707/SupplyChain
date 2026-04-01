using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Repositories;

public class PermissionRepository : Repository<Permission>, IPermissionRepository
{
    public PermissionRepository(IdentityDbContext context)
        : base(context)
    {
    }

    public async Task<Permission?> GetByPermissionNameAsync(string permissionName, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.PermissionName == permissionName, cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetByModuleAsync(string module, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.Module == module)
            .OrderBy(p => p.Action)
            .ToListAsync(cancellationToken);
    }

    public async Task<Permission?> GetByModuleAndActionAsync(string module, string action, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.Module == module && p.Action == action, cancellationToken);
    }
}

