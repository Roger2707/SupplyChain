using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Infrastructure.Data;
using SharedKernel.Repositories;

namespace ECommerce.Infrastructure.Repositories
{
    public class UnitOfWork : EfUnitOfWorkBase<ECommerceDbContext>, IUnitOfWork
    {
        private readonly ECommerceDbContext _context;
        private IBasketRepository _basketRepository;
        private IOrderRepository _orderRepository;

        public UnitOfWork(ECommerceDbContext context)
            : base(context)
        {
            _context = context;
        }

        public IBasketRepository BasketRepository => _basketRepository ??= new BasketRepository(_context);

        public IOrderRepository OrderRepository => _orderRepository ??= new OrderRepository(_context);
    }
}



