using ECommerce.Domain.Entities.Orders;

namespace ECommerce.Application.Interfaces.Repositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<List<Order>> GetAllWithLinesAsync(CancellationToken cancellationToken = default);
        Task<Order> GetWithLinesAsync(int orderId, CancellationToken cancellationToken = default);
        Task<List<Order>> GetOrdersWithLinesByUserId(int userId, CancellationToken cancellationToken = default);
    }
}
