using Inventory.Application.DTOs.Warehouses;
using Microsoft.AspNetCore.Authorization;
using SharedKernel.Ultilities;
using System.Security.Claims;

namespace SupplyChain.WebApi.ResourceBases
{
    // Requirement
    public class WarehouseScopeRequirement : IAuthorizationRequirement { }

    // Handler
    public class WarehouseScopeHandler
        : BaseScopeHandler<WarehouseScopeRequirement, WarehouseDto>
    {
        protected override Task HandleScopeAsync(AuthorizationHandlerContext context, WarehouseScopeRequirement requirement, WarehouseDto resource)
        {
            var userId = CF.GetInt(context.User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Only the warehouse manager can access the one's warehouse
            if (resource.ManagerId == userId)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
