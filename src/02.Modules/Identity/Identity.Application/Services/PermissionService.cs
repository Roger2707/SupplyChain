using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using SharedKernel.Entities;

namespace Identity.Application.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PermissionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<IEnumerable<PermissionDto>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var permissions = await _unitOfWork.PermissionRepository.GetAllAsync(cancellationToken);
            var permissionDtos = new List<PermissionDto>();

            foreach (var permission in permissions)
            {
                permissionDtos.Add(MapToDto(permission));
            }

            return Result<IEnumerable<PermissionDto>>.Success(permissionDtos);
        }

        public async Task<Result<PermissionDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var permission = await _unitOfWork.PermissionRepository.GetByIdAsync(id, cancellationToken);

            if (permission == null)
            {
                return Result<PermissionDto>.Failure($"Permission with ID {id} not found.");
            }

            var permissionDto = MapToDto(permission);
            return Result<PermissionDto>.Success(permissionDto);
        }

        public async Task<Result<PermissionDto>> CreateAsync(CreatePermissionDto createDto, CancellationToken cancellationToken = default)
        {
            string module = createDto.Module;
            string action = createDto.Action;

            var existedPermission = await _unitOfWork.PermissionRepository.GetByModuleAndActionAsync(module, action, cancellationToken);
            if(existedPermission != null)
            {
                return Result<PermissionDto>.Failure($"Permission with Module: {module} AND Action: {action} already existed !");
            }

            var permission = new Permission()
            {
                PermissionName = createDto.PermissionName,
                Module = module,
                Action = action,
                Description = createDto.Description,
                PermissionScope = PermissionScope.Warehouse
            };

            await _unitOfWork.PermissionRepository.AddAsync(permission, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var permissionDto = MapToDto(permission);

            return Result<PermissionDto>.Success(permissionDto);
        }

        public async Task<Result<PermissionDto>> UpdateAsync(int id, UpdatePermissionDto updateDto, CancellationToken cancellationToken = default)
        {
            var permission = await _unitOfWork.PermissionRepository.GetByIdAsync(id, cancellationToken);
            if(permission == null)
            {
                return Result<PermissionDto>.Failure($"Permission with ID {id} not found !");
            }

            if(!string.IsNullOrEmpty(updateDto.Description))
                permission.Description = updateDto.Description;

            await _unitOfWork.PermissionRepository.UpdateAsync(permission, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var permissionDto = MapToDto(permission);
            return Result<PermissionDto>.Success(permissionDto);
        }
        public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var permission = await _unitOfWork.PermissionRepository.GetByIdAsync(id, cancellationToken);
            if (permission == null)
            {
                return Result.Failure($"Permission with ID {id} not found !");
            }

            // Soft delete
            permission.IsDeleted = true;
            await _unitOfWork.PermissionRepository.UpdateAsync(permission);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        private static PermissionDto MapToDto(Permission permission)
        {
            return new PermissionDto
            {
                Id = permission.Id,
                PermissionName = permission.PermissionName,
                Module = permission.Module,
                Action = permission.Action,
                Description = permission.Description,
                CreatedAt = permission.CreatedAt,
                UpdatedAt = permission.UpdatedAt
            };
        }

        public async Task<Result> CanRoleHasPermissionAsync(int roleId, string module, string action, CancellationToken cancellationToken = default)
        {
            var permission = await _unitOfWork.PermissionRepository.GetByModuleAndActionAsync(module, action, cancellationToken);
            if(permission == null)
                return Result.Failure($"Permission with Module: {module} AND Action: {action} not found !");

            int permissionId = permission.Id;
            var rolePermission = await _unitOfWork.GetRepository<RolePermission>()
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);

            if(rolePermission == null)
                return Result.Failure($"Role with ID {roleId} does not have permission with Module: {module} AND Action: {action} !");

            return Result.Success();
        }
    }
}


