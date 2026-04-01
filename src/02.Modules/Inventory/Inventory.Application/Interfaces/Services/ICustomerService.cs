using Inventory.Application.DTOs.Customers;
using SharedKernel.Entities;

namespace Inventory.Application.Interfaces;

public interface ICustomerService
{
    Task<Result<CustomerDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<CustomerDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<CustomerDto>> CreateAsync(CreateCustomerDto createDto, CancellationToken cancellationToken = default);
    Task<Result<CustomerDto>> UpdateAsync(int id, UpdateCustomerDto updateDto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<bool>> ExistsAsync(int id, CancellationToken cancellationToken = default);
}




