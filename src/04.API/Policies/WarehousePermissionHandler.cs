using Identity.Application.Interfaces;
using Inventory.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using SharedKernel.Ultilities;
using System.Security.Claims;

namespace SupplyChain.WebApi.Policies
{
    public class WarehousePermissionHandler : AuthorizationHandler<WarehousePermissionRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPermissionService _permissionService;
        private readonly IWarehouseService _warehouseService;
        private readonly IUserService _userService;

        public WarehousePermissionHandler(IHttpContextAccessor httpContextAccessor, IPermissionService permissionService, IWarehouseService warehouseService, IUserService userService)
        {
            _httpContextAccessor = httpContextAccessor;
            _permissionService = permissionService;
            _warehouseService = warehouseService;
            _userService = userService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, WarehousePermissionRequirement requirement)
        {
            int userId = CF.GetInt(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if(userId == 0)
            {
                context.Fail();
                return;
            }

            int warehouseId = GetWarehouseIdFromRequest() ?? 0;
            if (warehouseId == 0)
            {
                context.Fail();
                return;
            }

            if (context.User.IsInRole("Super_Admin"))
            {
                context.Succeed(requirement);
                return;
            }
            else if(context.User.IsInRole("Regional_Manager"))
            {
                var regionIdResult = await _warehouseService.GetRegionIdByWarehouseIdAsync(warehouseId);
                if (!regionIdResult.IsSuccess)
                {
                    context.Fail();
                    return;
                }
                int regionId = regionIdResult.Data;

                var isExistedUserRegionResult = true;
                if(!isExistedUserRegionResult)
                {
                    context.Fail();
                    return;
                }
                else
                {
                    context.Succeed(requirement);
                    return;
                }
            }
            else if(context.User.IsInRole("Warehouse_Manager"))
            {
                var userWarehouseResult = await _userService.GetUserWarehouseAsync(userId, warehouseId);
                if(!userWarehouseResult.IsSuccess)
                {
                    context.Fail();
                    return;
                }
                bool isWarehouseManager = userWarehouseResult.Data.IsWarehouseManager;
                if(!isWarehouseManager)
                {
                    context.Fail();
                    return;
                }
                else
                {
                    context.Succeed(requirement);
                    return;
                }
            }
            else
            {
                // This time 100% sure that STAFF role (RoleId = 4)
                var hasPermissionResult = await _permissionService.CanRoleHasPermissionAsync(4, requirement.Module, requirement.Action);
                var userWarehouseResult = await _userService.GetUserWarehouseAsync(userId, warehouseId);      
                if (hasPermissionResult.IsSuccess && userWarehouseResult.IsSuccess)
                {
                    context.Succeed(requirement);
                }
                else
                {
                    context.Fail();
                }
            }
        }

        private int? GetWarehouseIdFromRequest()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return null;

            // Try route values first
            if (httpContext.Request.RouteValues.TryGetValue("id", out var routeValue))
            {
                if (int.TryParse(routeValue?.ToString(), out int warehouseId))
                    return warehouseId;
            }

            // Try query string
            if (httpContext.Request.Query.TryGetValue("id", out var queryValue))
            {
                if (int.TryParse(queryValue.ToString(), out int warehouseId))
                    return warehouseId;
            }

            return null;
        }
    }

    public class WarehousePermissionRequirement : IAuthorizationRequirement
    {
        public string Module { get; }
        public string Action { get; }

        public WarehousePermissionRequirement(string module, string action)
        {
            Module = module;
            Action = action;
        }
    }
}
