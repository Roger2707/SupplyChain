using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SupplyChain.WebApi.ResourceBases
{
    public abstract class BaseScopeHandler<TRequirement, TResource>
        : AuthorizationHandler<TRequirement, TResource>
        where TRequirement : IAuthorizationRequirement
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            TRequirement requirement,
            TResource resource)
        {
            // Super admin auto pass
            if (context.User.HasClaim(ClaimTypes.Role, "Super_Admin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            return HandleScopeAsync(context, requirement, resource);
        }

        // Subclass implement this method to handle specific scope logic
        protected abstract Task HandleScopeAsync(
            AuthorizationHandlerContext context,
            TRequirement requirement,
            TResource resource);
    }
}
