using Identity.Application.DTOs;
using Identity.Domain.Entities;
using SharedKernel.Entities;

namespace Identity.Application.Interfaces;

public interface IUserService
{
    Task<Result<UserDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<UserDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<UserDto>> CreateAsync(CreateUserDto createDto, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> UpdateAsync(int id, UpdateUserDto updateDto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> AssignRolesAsync(int userId, List<int> roleIds, CancellationToken cancellationToken = default);
    Task<Result<UserWarehouse>> GetUserWarehouseAsync(int userId, int warehouseId, CancellationToken cancellationToken = default);
}



