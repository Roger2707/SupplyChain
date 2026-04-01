using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using SharedKernel.Entities;

namespace Identity.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserQueries _userQueries;

    public UserService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IUserQueries userQueries)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _userQueries = userQueries;
    }

    public async Task<Result<UserDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.UserRepository.GetWithRolesAsync(id, cancellationToken);

        if (user == null)
        {
            return Result<UserDto>.Failure($"User with ID {id} not found.");
        }

        var roles = user.UserRoles.Select(ur => ur.Role.RoleName).Distinct().ToList();

        var userDto = MapToDto(user, roles);
        return Result<UserDto>.Success(userDto);
    }

    public async Task<Result<IEnumerable<UserDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userQueries.GetUsersWithRolesAsync(cancellationToken);
        return Result<IEnumerable<UserDto>>.Success(users);
    }

    public async Task<Result<UserDto>> CreateAsync(CreateUserDto createDto, CancellationToken cancellationToken = default)
    {
        // Check if username already exists
        if (await _unitOfWork.UserRepository.ExistsByUsernameAsync(createDto.Username, cancellationToken))
        {
            return Result<UserDto>.Failure($"Username '{createDto.Username}' already exists.");
        }

        // Check if email already exists
        if (await _unitOfWork.UserRepository.ExistsByEmailAsync(createDto.Email, cancellationToken))
        {
            return Result<UserDto>.Failure($"Email '{createDto.Email}' already exists.");
        }

        // Hash password
        var passwordHash = _passwordHasher.HashPassword(createDto.Password);

        // Create user
        var user = new User
        {
            Username = createDto.Username,
            PasswordHash = passwordHash,
            FullName = createDto.FullName,
            Email = createDto.Email,
            PhoneNumber = createDto.PhoneNumber,
            Address = createDto.Address,
            IsActive = createDto.IsActive
        };

        await _unitOfWork.UserRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Assign roles
        if (createDto.RoleIds.Any())
        {
            await AssignRolesInternalAsync(user.Id, createDto.RoleIds, cancellationToken);
        }

        var createdUser = await _unitOfWork.UserRepository.GetWithRolesAsync(user.Id, cancellationToken);
        if (createdUser == null)
        {
            return Result<UserDto>.Failure("Failed to create user.");
        }

        var roles = createdUser.UserRoles.Select(ur => ur.Role.RoleName).Distinct().ToList();
        var userDto = MapToDto(createdUser, roles);
        return Result<UserDto>.Success(userDto);
    }

    public async Task<Result<UserDto>> UpdateAsync(int id, UpdateUserDto updateDto, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(id, cancellationToken);

        if (user == null)
        {
            return Result<UserDto>.Failure($"User with ID {id} not found.");
        }

        // Check if email is being changed and if new email already exists
        if (!string.IsNullOrEmpty(updateDto.Email) && user.Email != updateDto.Email)
        {
            if (await _unitOfWork.UserRepository.ExistsByEmailAsync(updateDto.Email, cancellationToken))
            {
                return Result<UserDto>.Failure($"Email '{updateDto.Email}' already exists.");
            }
        }

        // Update user properties
        if (!string.IsNullOrEmpty(updateDto.FullName))
            user.FullName = updateDto.FullName;
        if (!string.IsNullOrEmpty(updateDto.Email))
            user.Email = updateDto.Email;
        if (updateDto.PhoneNumber != null)
            user.PhoneNumber = updateDto.PhoneNumber;
        if (updateDto.Address != null)
            user.Address = updateDto.Address;
        if (updateDto.IsActive.HasValue)
            user.IsActive = updateDto.IsActive.Value;

        await _unitOfWork.UserRepository.UpdateAsync(user, cancellationToken);

        // Update roles if provided
        if (updateDto.RoleIds != null)
        {
            await AssignRolesInternalAsync(id, updateDto.RoleIds, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedUser = await _unitOfWork.UserRepository.GetWithRolesAsync(id, cancellationToken);
        if (updatedUser == null)
        {
            return Result<UserDto>.Failure("Failed to update user.");
        }
        var roles = updatedUser.UserRoles.Select(ur => ur.Role.RoleName).Distinct().ToList();
        var userDto = MapToDto(updatedUser, roles);
        return Result<UserDto>.Success(userDto);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(id, cancellationToken);

        if (user == null)
        {
            return Result.Failure($"User with ID {id} not found.");
        }

        // Soft delete
        user.IsDeleted = true;
        await _unitOfWork.UserRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<UserDto>> AssignRolesAsync(int userId, List<int> roleIds, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            return Result<UserDto>.Failure($"User with ID {userId} not found.");
        }

        await AssignRolesInternalAsync(userId, roleIds, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedUser = await _unitOfWork.UserRepository.GetWithRolesAsync(userId, cancellationToken);
        if (updatedUser == null)
        {
            return Result<UserDto>.Failure("Failed to assign roles.");
        }
        var roles = updatedUser.UserRoles.Select(ur => ur.Role.RoleName).Distinct().ToList();
        var userDto = MapToDto(updatedUser, roles);
        return Result<UserDto>.Success(userDto);
    }

    private async Task AssignRolesInternalAsync(int userId, List<int> roleIds, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.UserRepository.GetWithRolesAsync(userId, cancellationToken);
        if (user == null) return;

        // Remove existing roles
        var existingUserRoles = user.UserRoles.ToList();
        foreach (var userRole in existingUserRoles)
        {
            var userRoleEntity = await _unitOfWork.GetRepository<UserRole>()
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == userRole.RoleId, cancellationToken);
            if (userRoleEntity != null)
            {
                await _unitOfWork.GetRepository<UserRole>().DeleteAsync(userRoleEntity, cancellationToken);
            }
        }

        // Add new roles
        foreach (var roleId in roleIds)
        {
            var role = await _unitOfWork.RoleRepository.GetByIdAsync(roleId, cancellationToken);
            if (role != null)
            {
                var userRole = new UserRole
                {
                    UserId = userId,
                    RoleId = roleId,
                    AssignedAt = DateTime.UtcNow
                };
                await _unitOfWork.GetRepository<UserRole>().AddAsync(userRole, cancellationToken);
            }
        }
    }

    private static UserDto MapToDto(User user, List<string> roles)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Roles = roles,
        };
    }

    public async Task<Result<UserWarehouse>> GetUserWarehouseAsync(int userId, int warehouseId, CancellationToken cancellationToken = default)
    {
        var user_warehouse = await _unitOfWork.GetRepository<UserWarehouse>().FirstOrDefaultAsync(ur => ur.UserId == userId && ur.WarehouseId == warehouseId);
        if (user_warehouse == null)
        {
            return Result<UserWarehouse>.Failure($"UserWarehouse with User ID {userId} and Warehouse ID {warehouseId} not found.");
        }
        return Result<UserWarehouse>.Success(user_warehouse);
    }
}



