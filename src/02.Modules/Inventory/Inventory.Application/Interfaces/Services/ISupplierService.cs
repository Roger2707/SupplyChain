using Inventory.Application.DTOs.Suppliers;
using SharedKernel.Entities;

namespace Inventory.Application.Interfaces;

public interface ISupplierService
{
    Task<Result<SupplierDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<SupplierDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<SupplierDto>> CreateAsync(CreateSupplierDto createDto, CancellationToken cancellationToken = default);
    Task<Result<SupplierDto>> UpdateAsync(int id, UpdateSupplierDto updateDto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<bool>> ExistsAsync(int id, CancellationToken cancellationToken = default);
}




