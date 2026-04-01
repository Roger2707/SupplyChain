using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using SharedKernel.Entities;
using SharedKernel.Ultilities;
namespace Identity.Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public AuthenticationService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService; 
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.UserRepository.GetByUsernameAsync(loginDto.Username, cancellationToken);

        if (user == null || !user.IsActive)
        {
            return Result<AuthResponseDto>.Failure("Invalid username or password.");
        }

        if (!_passwordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
        {
            return Result<AuthResponseDto>.Failure("Invalid username or password.");
        }

        // Get user with roles and permissions
        var userWithDetails = await _unitOfWork.UserRepository.GetWithRolesAsync(user.Id, cancellationToken);
        
        if (userWithDetails == null)
        {
            return Result<AuthResponseDto>.Failure("User not found.");
        }

        // Extract roles
        var roles = userWithDetails.UserRoles
            .Select(ur => ur.Role.RoleName)
            .Distinct()
            .ToList();

        // Generate token
        var token = _jwtService.GenerateToken(userWithDetails, roles);

        var response = new AuthResponseDto
        {
            Token = token,
            ExpiresAt = _jwtService.GetTokenExpiration(),
            User = MapToUserDto(userWithDetails, roles)
        };

        return Result<AuthResponseDto>.Success(response);
    }

    public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken = default)
    {
        // Check if username already exists
        if (await _unitOfWork.UserRepository.ExistsByUsernameAsync(registerDto.Username, cancellationToken))
        {
            return Result<AuthResponseDto>.Failure($"Username '{registerDto.Username}' already exists.");
        }

        // Check if email already exists
        if (await _unitOfWork.UserRepository.ExistsByEmailAsync(registerDto.Email, cancellationToken))
        {
            return Result<AuthResponseDto>.Failure($"Email '{registerDto.Email}' already exists.");
        }

        // Hash password
        var passwordHash = _passwordHasher.HashPassword(registerDto.Password);

        // Create user
        var user = new User
        {
            Username = registerDto.Username,
            PasswordHash = passwordHash,
            FullName = registerDto.FullName,
            Email = registerDto.Email,
            PhoneNumber = registerDto.PhoneNumber,
            Address = registerDto.Address,
            IsActive = true
        };

        await _unitOfWork.UserRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Get the created user with details
        var createdUser = await _unitOfWork.UserRepository.GetWithRolesAsync(user.Id, cancellationToken);
        
        if (createdUser == null)
        {
            return Result<AuthResponseDto>.Failure("Failed to create user.");
        }

        var roles = new List<string>();
        var token = _jwtService.GenerateToken(createdUser, roles);

        var response = new AuthResponseDto
        {
            Token = token,
            ExpiresAt = _jwtService.GetTokenExpiration(),
            User = MapToUserDto(createdUser, roles)
        };

        return Result<AuthResponseDto>.Success(response);
    }

    public async Task<Result> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            return Result.Failure("User not found.");
        }

        if (!_passwordHasher.VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
        {
            return Result.Failure("Current password is incorrect.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(changePasswordDto.NewPassword);
        await _unitOfWork.UserRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static UserDto MapToUserDto(User user, List<string> roles)
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
}



