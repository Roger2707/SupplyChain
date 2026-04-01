using Inventory.Domain.Entities.Accounts;

namespace Inventory.Application.Interfaces.Repositories
{
    public interface IJournalEntryRepository : IRepository<JournalEntry>
    {
        Task<List<JournalEntry>> GetAllWithLinesAsync(CancellationToken cancellationToken = default);
        Task<JournalEntry> GetWithLinesAsync(int id, CancellationToken cancellationToken = default);
    }
}
