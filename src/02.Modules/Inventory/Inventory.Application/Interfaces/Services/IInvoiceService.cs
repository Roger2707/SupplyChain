using Inventory.Application.DTOs.Invoices;
using SharedKernel.Entities;

namespace Inventory.Application.Interfaces.Services
{
    public interface IInvoiceService
    {
        Task<Result<List<InvoiceDto>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<InvoiceDto>> GetWithLinesAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<InvoiceDto>> CreateAsync(CreateInvoiceDto createInvoiceDto, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<bool>> ExistAsync(int id, CancellationToken cancellationToken = default);
        Task<Result> PostAsync(int id, CancellationToken cancellationToken = default);
    }
}



