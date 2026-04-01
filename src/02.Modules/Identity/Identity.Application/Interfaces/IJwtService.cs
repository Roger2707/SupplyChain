using Identity.Domain.Entities;
using System.Security.Claims;

namespace Identity.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user, IEnumerable<string> roles);
    ClaimsPrincipal ValidateToken(string token);
    DateTime GetTokenExpiration();
}

