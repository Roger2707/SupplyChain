using Inventory.Application.DTOs.Warehouses;
using SharedKernel.Entities;

namespace Inventory.Application.Interfaces;

public interface IWarehouseService
{
    Task<Result<WarehouseDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<WarehouseDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<WarehouseDto>> CreateAsync(CreateWarehouseDto createDto, CancellationToken cancellationToken = default);
    Task<Result<WarehouseDto>> UpdateAsync(int id, UpdateWarehouseDto updateDto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<bool>> ExistsAsync(int id, CancellationToken cancellationToken = default);
}




