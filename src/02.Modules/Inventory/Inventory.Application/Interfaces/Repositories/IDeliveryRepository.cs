using Inventory.Domain.Entities.Delivery;

namespace Inventory.Application.Interfaces.Repositories
{
    public interface IDeliveryRepository : IRepository<Delivery>
    {
        Task<IEnumerable<Delivery>> GetAllWithLinesAsync(CancellationToken cancellationToken = default);
        Task<Delivery> GetWithLinesAsync(int id, CancellationToken cancellationToken = default);
        Task<List<Delivery>> GetPostedDeliveriesWithLinesAsync(CancellationToken cancellationToken = default);
    }
}
