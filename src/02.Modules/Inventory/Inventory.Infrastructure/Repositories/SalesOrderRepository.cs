using Inventory.Application.Interfaces.Repositories;
using Inventory.Domain.Entities.SalesOrder;
using Inventory.Domain.Enums;
using Inventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Repositories
{
    public class SalesOrderRepository : Repository<SalesOrder>, ISalesOrderRepository
    {
        public object DSalesOrderStatus { get; private set; }

        public SalesOrderRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<SalesOrder>> GetAllWithLinesAsync(CancellationToken cancellationToken = default)
        {
            var salesOrder = await _context.SalesOrders
                .Include(s => s.Customer)
                .Include(s => s.Lines)
                .ThenInclude(l => l.Product)
                .ToListAsync(cancellationToken);

            return salesOrder;
        }

        public async Task<SalesOrder> GetWithLinesAsync(int id, CancellationToken cancellationToken = default)
        {
            var salesOrder = await _context.SalesOrders
                                        .Include(s => s.Customer)
                                        .Include(s => s.Lines)
                                        .ThenInclude(l => l.Product)
                                        .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            return salesOrder;
        }

        public async Task<List<SalesOrder>> GetConfirmedSalesOrders(CancellationToken cancellationToken = default)
        {
            var salesOrders = await _context.SalesOrders
                            .Include(s => s.Customer)
                            .Include(s => s.Lines)
                            .ThenInclude(l => l.Product)
                            .Where(s => s.Status == SalesOrderStatus.Confirmed)
                            .ToListAsync(cancellationToken);

            return salesOrders;
        }
    }
}
