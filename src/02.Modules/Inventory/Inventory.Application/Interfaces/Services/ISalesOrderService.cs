using Inventory.Application.DTOs.SalesOrder;
using Inventory.Domain.Entities.SalesOrder;
using SharedKernel.Entities;

namespace Inventory.Application.Interfaces.Services
{
    public interface ISalesOrderService
    {
        Task<Result<List<SalesOrderDto>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<SalesOrderDto>> GetWithLinesAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<List<SalesOrder>>> GetConfirmedSalesOrders(CancellationToken cancellationToken = default);
        Task<Result<SalesOrderDto>> CreateAsync(CreateSalesOrderDto createSalesOrderDto, CancellationToken cancellationToken = default);
        Task<Result<SalesOrderDto>> UpdateAsync(int id, UpdateSalesOrderDto updateSalesOrderDto, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<Result> ConfirmAsync(int id, CancellationToken cancellationToken = default);
        Task<Result> CancelAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<bool>> ExistAsync(int id, CancellationToken cancellationToken = default);
    }
}


