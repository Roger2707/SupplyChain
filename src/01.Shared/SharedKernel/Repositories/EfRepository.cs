using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace SharedKernel.Repositories;

public class EfRepository<TContext, TEntity>
    where TContext : DbContext
    where TEntity : class
{
    protected readonly TContext Context;
    protected readonly DbSet<TEntity> DbSet;

    public EfRepository(TContext context)
    {
        Context = context;
        DbSet = context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<TEntity?> GetByConditionAsync(
        Expression<Func<TEntity, bool>> condition,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(condition, cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> FindByConditionAsync(
        Expression<Func<TEntity, bool>> condition,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(condition).ToListAsync(cancellationToken);
    }

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await DbSet.AddRangeAsync(entities, cancellationToken);
    }

    public virtual void Update(TEntity entity)
    {
        DbSet.Update(entity);
    }

    public virtual void UpdateRange(IEnumerable<TEntity> entities)
    {
        DbSet.UpdateRange(entities);
    }

    public virtual void Delete(TEntity entity)
    {
        DbSet.Remove(entity);
    }

    public virtual void DeleteRange(IEnumerable<TEntity> entities)
    {
        DbSet.RemoveRange(entities);
    }

    public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(cancellationToken);
    }

    public virtual async Task<int> CountByConditionAsync(
        Expression<Func<TEntity, bool>> condition,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(condition, cancellationToken);
    }

    public virtual async Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> condition,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(condition, cancellationToken);
    }
}
