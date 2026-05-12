using Microsoft.AspNetCore.Authorization;

namespace SupplyChain.WebApi.Policies
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PermissionHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var permissions = _httpContextAccessor.HttpContext?.Items["permissions"] as List<string>;

            if (permissions?.Contains(requirement.Permission) == true)
            {
                context.Succeed(requirement);
                return;
            }

            context.Fail();
            return;
        }
    }

    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }
        public PermissionRequirement(string permission) => Permission = permission;
    }
}
