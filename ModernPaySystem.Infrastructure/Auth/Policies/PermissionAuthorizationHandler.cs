using Microsoft.AspNetCore.Authorization;

namespace ModernPaySystem.Infrastructure.Auth.Policies;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var permissionClaim = context.User.FindAll("permission");

        if (permissionClaim.Any(c => c.Value == requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
