using Inventory.Application.Interfaces.Repositories;
using Inventory.Domain.Entities.Invoice;
using Inventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Repositories
{
    public class InvoiceRepository : Repository<Invoice>, IInvoiceRepository
    {
        public InvoiceRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Invoice>> GetAllWithLinesAsync(CancellationToken cancellationToken = default)
        {
            var invoices = await _context.Invoices.Include(i => i.Lines).ToListAsync(cancellationToken);
            return invoices;
        }

        public async Task<Invoice> GetWithLinesAsync(int id, CancellationToken cancellationToken = default)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Lines)
                .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

            return invoice;
        }
    }
}
