using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Volo.Abp.Authorization;
using System.Linq;

namespace W2.Authorization.Handlers;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (!context.User.Identity.IsAuthenticated)
        {
            return Task.CompletedTask;
        }

        var permissions = context.User.Claims
            .Where(x => x.Type == "permissions")
            .Select(x => x.Value)
            .ToList();

        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
