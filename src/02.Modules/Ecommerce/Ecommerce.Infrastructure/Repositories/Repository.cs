using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Infrastructure.Data;
using SharedKernel.Repositories;
using System.Linq.Expressions;

namespace ECommerce.Infrastructure.Repositories
{
    public class Repository<T> : EfRepository<ECommerceDbContext, T>, IRepository<T> where T : class
    {
        protected readonly ECommerceDbContext _context;
        protected readonly Microsoft.EntityFrameworkCore.DbSet<T> _dbSet;

        public Repository(ECommerceDbContext context)
            : base(context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public override async Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
        {
            return await base.GetByIdAsync(id, cancellationToken);
        }

        public override async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await base.GetAllAsync(cancellationToken);
        }

        public override async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await base.AddAsync(entity, cancellationToken);
            return entity;
        }

        public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            base.Update(entity);
            return Task.CompletedTask;
        }

        public override async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await base.ExistsAsync(predicate, cancellationToken);
        }
    }
}



