using Identity.Application.DTOs;
using SharedKernel.Entities;

namespace Identity.Application.Interfaces;

public interface IAuthenticationService
{
    Task<Result<AuthSessionDto>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default);
    Task<Result<AuthSessionDto>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<Result> LogoutAsync(int userId, string? refreshToken, CancellationToken cancellationToken = default);
    Task<Result> ForceLogoutAsync(int userId, CancellationToken cancellationToken = default);
    Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken = default);
    Task<Result> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto, CancellationToken cancellationToken = default);
}




