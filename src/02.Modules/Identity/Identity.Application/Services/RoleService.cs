using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using SharedKernel.Entities;

namespace Identity.Application.Services;

public class RoleService : IRoleService
{
    private readonly IUnitOfWork _unitOfWork;

    public RoleService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RoleDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var role = await _unitOfWork.RoleRepository.GetWithPermissionsAsync(id, cancellationToken);

        if (role == null)
        {
            return Result<RoleDto>.Failure($"Role with ID {id} not found.");
        }

        var permissions = role.RolePermissions
            .Select(rp => rp.Permission.PermissionName)
            .Distinct()
            .ToList();

        var roleDto = MapToDto(role, permissions);
        return Result<RoleDto>.Success(roleDto);
    }

    public async Task<Result<IEnumerable<RoleDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _unitOfWork.RoleRepository.GetAllAsync(cancellationToken);
        var roleDtos = new List<RoleDto>();

        foreach (var role in roles)
        {
            var roleWithPermissions = await _unitOfWork.RoleRepository.GetWithPermissionsAsync(role.Id, cancellationToken);
            if (roleWithPermissions != null)
            {
                var permissions = roleWithPermissions.RolePermissions
                    .Select(rp => rp.Permission.PermissionName)
                    .Distinct()
                    .ToList();
                roleDtos.Add(MapToDto(roleWithPermissions, permissions));
            }
        }

        return Result<IEnumerable<RoleDto>>.Success(roleDtos);
    }

    public async Task<Result<RoleDto>> CreateAsync(CreateRoleDto createDto, CancellationToken cancellationToken = default)
    {
        // Check if role name already exists
        if (await _unitOfWork.RoleRepository.ExistsByRoleNameAsync(createDto.RoleName, cancellationToken))
        {
            return Result<RoleDto>.Failure($"Role with name '{createDto.RoleName}' already exists.");
        }

        // Create role
        var role = new Role
        {
            RoleName = createDto.RoleName,
            Description = createDto.Description
        };

        await _unitOfWork.RoleRepository.AddAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var createdRole = await _unitOfWork.RoleRepository.GetWithPermissionsAsync(role.Id, cancellationToken);
        if (createdRole == null)
        {
            return Result<RoleDto>.Failure("Failed to create role.");
        }

        var permissions = createdRole.RolePermissions
            .Select(rp => rp.Permission.PermissionName)
            .Distinct()
            .ToList();

        var roleDto = MapToDto(createdRole, permissions);
        return Result<RoleDto>.Success(roleDto);
    }

    public async Task<Result<RoleDto>> UpdateAsync(int id, UpdateRoleDto updateDto, CancellationToken cancellationToken = default)
    {
        var role = await _unitOfWork.RoleRepository.GetByIdAsync(id, cancellationToken);

        if (role == null)
        {
            return Result<RoleDto>.Failure($"Role with ID {id} not found.");
        }

        // Update role properties
        if (!string.IsNullOrEmpty(updateDto.Description))
            role.Description = updateDto.Description;

        await _unitOfWork.RoleRepository.UpdateAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedRole = await _unitOfWork.RoleRepository.GetWithPermissionsAsync(id, cancellationToken);
        if (updatedRole == null)
        {
            return Result<RoleDto>.Failure("Failed to update role.");
        }

        var permissions = updatedRole.RolePermissions
            .Select(rp => rp.Permission.PermissionName)
            .Distinct()
            .ToList();

        var roleDto = MapToDto(updatedRole, permissions);
        return Result<RoleDto>.Success(roleDto);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var role = await _unitOfWork.RoleRepository.GetByIdAsync(id, cancellationToken);

        if (role == null)
        {
            return Result.Failure($"Role with ID {id} not found.");
        }

        // Soft delete
        role.IsDeleted = true;
        await _unitOfWork.RoleRepository.UpdateAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static RoleDto MapToDto(Role role, List<string> permissions)
    {
        return new RoleDto
        {
            Id = role.Id,
            RoleName = role.RoleName,
            Description = role.Description,
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt,
            Permissions = permissions
        };
    }
}



