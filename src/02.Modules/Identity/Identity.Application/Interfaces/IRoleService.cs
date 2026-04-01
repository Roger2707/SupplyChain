using Identity.Application.DTOs;
using SharedKernel.Entities;

namespace Identity.Application.Interfaces;

public interface IRoleService
{
    Task<Result<RoleDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<RoleDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<RoleDto>> CreateAsync(CreateRoleDto createDto, CancellationToken cancellationToken = default);
    Task<Result<RoleDto>> UpdateAsync(int id, UpdateRoleDto updateDto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}




