using Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace SupplyChain.WebApi.Middlewares
{
    public class PermissionMiddleWare
    {
        private readonly RequestDelegate _next;

        public PermissionMiddleWare(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context, IdentityDbContext db)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var roles = context.User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

                var permissionNames = await db.RolePermissions
                                    .Include(rp => rp.Permission)
                                    .Include(rp => rp.Role)
                                    .Where(rp => roles.Contains(rp.Role.RoleName.ToString()))
                                    .Select(rp => rp.Permission.PermissionName)
                                    .ToListAsync();

                context.Items["permissions"] = permissionNames;
            }

            await _next(context);
        }
    }
}
