using Identity.Application.DTOs;
using SharedKernel.Entities;

namespace Identity.Application.Interfaces;

public interface IPermissionService
{
    Task<Result<PermissionDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<PermissionDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<PermissionDto>> CreateAsync(CreatePermissionDto createDto, CancellationToken cancellationToken = default);
    Task<Result<PermissionDto>> UpdateAsync(int id, UpdatePermissionDto updateDto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<Result> CanRoleHasPermissionAsync(int roleId, string module, string action, CancellationToken cancellationToken = default);
}




