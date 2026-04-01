using Inventory.Domain.Entities.Invoice;

namespace Inventory.Application.Interfaces.Repositories
{
    public interface IInvoiceRepository : IRepository<Invoice>
    {
        Task<IEnumerable<Invoice>> GetAllWithLinesAsync(CancellationToken cancellationToken = default);
        Task<Invoice> GetWithLinesAsync(int id, CancellationToken cancellationToken = default);
    }
}
