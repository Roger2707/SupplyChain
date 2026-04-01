using Inventory.Application.Interfaces.Repositories;
using Inventory.Domain.Entities.Accounts;
using Inventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Repositories
{
    public class JournalEntryRepository : Repository<JournalEntry>, IJournalEntryRepository
    {
        public JournalEntryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<JournalEntry>> GetAllWithLinesAsync(CancellationToken cancellationToken = default)
        {
            var journals = await _context.JournalEntries.Include(j => j.Lines).ThenInclude(l => l.Account).ToListAsync(cancellationToken);
            return journals;
        }

        public async Task<JournalEntry> GetWithLinesAsync(int id, CancellationToken cancellationToken = default)
        {
            var journal = await _context.JournalEntries
                .Include(j => j.Lines)
                .ThenInclude(l => l.Account)
                .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
            return journal;
        }
    }
}
