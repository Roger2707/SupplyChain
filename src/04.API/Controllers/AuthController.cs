using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SupplyChain.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private const string RefreshTokenCookieName = "refreshToken";

    public AuthController(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    /// <summary>
    /// Login with username and password
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto, CancellationToken cancellationToken)
    {
        var result = await _authenticationService.LoginAsync(loginDto, cancellationToken);

        if (!result.IsSuccess)
        {
            return Unauthorized(new { message = result.ErrorMessage });
        }

        AppendRefreshTokenCookie(result.Data.RefreshToken);
        return Ok(result.Data.AuthResponse);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken))
            return Unauthorized(new { message = "Refresh token is missing." });

        var result = await _authenticationService.RefreshTokenAsync(refreshToken, cancellationToken);
        if (!result.IsSuccess)
            return Unauthorized(new { message = result.ErrorMessage });

        AppendRefreshTokenCookie(result.Data.RefreshToken);
        return Ok(result.Data.AuthResponse);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized(new { message = "Invalid user token" });

        Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshToken);
        var result = await _authenticationService.LogoutAsync(userId, refreshToken, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        Response.Cookies.Delete(RefreshTokenCookieName);
        return Ok(new { message = "Logged out successfully." });
    }

    [HttpPost("force-logout/{userId:int}")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<IActionResult> ForceLogout([FromRoute] int userId, CancellationToken cancellationToken)
    {
        var result = await _authenticationService.ForceLogoutAsync(userId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new { message = $"User {userId} has been forced logout and blacklisted." });
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto, CancellationToken cancellationToken)
    {
        var result = await _authenticationService.RegisterAsync(registerDto, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Change password for the current user
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(new { message = "Invalid user token" });
        }

        var result = await _authenticationService.ChangePasswordAsync(userId, changePasswordDto, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(new { message = "Password changed successfully" });
    }

    private void AppendRefreshTokenCookie(string refreshToken)
    {
        Response.Cookies.Append(RefreshTokenCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Path = "/api/Auth/refresh",
            //Path = "/",
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });
    }
}