using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Microsoft.Extensions.Configuration;
using SharedKernel.Interfaces;
using SharedKernel.Entities;

namespace Identity.Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly ICacheService _cacheService;
    private readonly int _refreshTokenExpirationDays;
    private readonly int _forceLogoutDays;

    public AuthenticationService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        ICacheService cacheService,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _cacheService = cacheService;
        _refreshTokenExpirationDays = int.Parse(configuration["JWTSettings:RefreshTokenExpirationDays"] ?? "7");
        _forceLogoutDays = int.Parse(configuration["JWTSettings:ForceLogoutBlacklistDays"] ?? "30");
    }

    public async Task<Result<AuthSessionDto>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.UserRepository.GetByUsernameAsync(loginDto.Username, cancellationToken);

        if (user == null || !user.IsActive)
        {
            return Result<AuthSessionDto>.Failure("Invalid username or password.");
        }

        if (!_passwordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
        {
            return Result<AuthSessionDto>.Failure("Invalid username or password.");
        }

        // Get user with roles and permissions
        var userWithDetails = await _unitOfWork.UserRepository.GetWithRolesAsync(user.Id, cancellationToken);
        
        if (userWithDetails == null)
        {
            return Result<AuthSessionDto>.Failure("User not found.");
        }

        // Extract roles
        var roles = userWithDetails.UserRoles
            .Select(ur => ur.Role.RoleName)
            .Distinct()
            .ToList();

        // Generate token
        var token = _jwtService.GenerateToken(userWithDetails, roles);

        var refreshToken = CreateRefreshToken();
        var authResponse = new AuthResponseDto
        {
            Token = token,
            ExpiresAt = _jwtService.GetTokenExpiration(),
            User = MapToUserDto(userWithDetails, roles)
        };

        await StoreRefreshTokenSessionAsync(refreshToken, userWithDetails, roles);

        return Result<AuthSessionDto>.Success(new AuthSessionDto
        {
            AuthResponse = authResponse,
            RefreshToken = refreshToken
        });
    }

    public async Task<Result<AuthSessionDto>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Result<AuthSessionDto>.Failure("Refresh token is required.");

        var refreshLockKey = $"refresh:lock:{refreshToken}";
        var lockAcquired = await _cacheService.SetIfNotExistsAsync(refreshLockKey, "1", TimeSpan.FromSeconds(30));

        if (!lockAcquired)
            return Result<AuthSessionDto>.Failure("Refresh token is already being used.");

        try
        {
            var refreshKey = GetRefreshTokenKey(refreshToken);
            var session = await _cacheService.GetAsync<RefreshTokenSessionDto>(refreshKey);
            if (session == null)
                return Result<AuthSessionDto>.Failure("Invalid or expired refresh token.");

            var blacklistKey = $"blacklist:user:{session.UserId}";
            if (await _cacheService.ExistsAsync(blacklistKey))
                return Result<AuthSessionDto>.Failure("User is blacklisted.");

            var user = await _unitOfWork.UserRepository.GetWithRolesAsync(session.UserId, cancellationToken);
            if (user == null || !user.IsActive)
                return Result<AuthSessionDto>.Failure("User not found or inactive.");

            var roles = user.UserRoles
                .Select(ur => ur.Role.RoleName)
                .Distinct()
                .ToList();

            var accessToken = _jwtService.GenerateToken(user, roles);
            var newRefreshToken = CreateRefreshToken();

            await _cacheService.RemoveAsync(refreshKey);
            await StoreRefreshTokenSessionAsync(newRefreshToken, user, roles);

            return Result<AuthSessionDto>.Success(new AuthSessionDto
            {
                AuthResponse = new AuthResponseDto
                {
                    Token = accessToken,
                    ExpiresAt = _jwtService.GetTokenExpiration(),
                    User = MapToUserDto(user, roles)
                },
                RefreshToken = newRefreshToken
            });
        }
        finally
        {
            await _cacheService.RemoveAsync(refreshLockKey);
        }
    }

    public async Task<Result> LogoutAsync(int userId, string? refreshToken, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(refreshToken))
            await _cacheService.RemoveAsync(GetRefreshTokenKey(refreshToken));

        var blacklistKey = $"blacklist:user:{userId}";
        var ttl = _jwtService.GetTokenExpiration() - DateTime.UtcNow;
        if (ttl <= TimeSpan.Zero)
            ttl = TimeSpan.FromMinutes(5);

        await _cacheService.SetAsync(blacklistKey, "1", ttl);
        return Result.Success();
    }

    public async Task<Result> ForceLogoutAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            return Result.Failure($"User with ID {userId} not found.");

        var blacklistKey = $"blacklist:user:{userId}";
        await _cacheService.SetAsync(blacklistKey, "1", TimeSpan.FromDays(_forceLogoutDays));
        return Result.Success();
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

    private string CreateRefreshToken() => Guid.NewGuid().ToString("N");

    private static string GetRefreshTokenKey(string refreshToken) => $"refresh:token:{refreshToken}";

    private async Task StoreRefreshTokenSessionAsync(string refreshToken, User user, List<string> roles)
    {
        var session = new RefreshTokenSessionDto
        {
            UserId = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Roles = roles
        };

        var refreshKey = GetRefreshTokenKey(refreshToken);
        await _cacheService.SetAsync(refreshKey, session, TimeSpan.FromDays(_refreshTokenExpirationDays));
    }

}