using Inventory.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using SharedKernel.Ultilities;
using System.Security.Claims;

namespace SupplyChain.WebApi.ResourceBases
{
    // Requirement
    public class WarehouseScopeRequirement : IAuthorizationRequirement { }

    // Handler
    public class WarehouseScopeHandler
        : AuthorizationHandler<WarehouseScopeRequirement, Warehouse>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            WarehouseScopeRequirement requirement,
            Warehouse warehouse)
        {
            var userId = CF.GetInt(context.User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Only the warehouse manager can access the one's warehouse
            if (warehouse.ManagerId == userId)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
