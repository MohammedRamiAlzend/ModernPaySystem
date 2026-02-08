global using Microsoft.AspNetCore.Authorization;
global using Microsoft.EntityFrameworkCore;
global using System.Threading.Tasks;
global using System;
using System.Linq;

namespace ModernPaySystem.Infrastructure.Auth;

public class PermissionAuthorizationHandler(AppDbContext dbContext) : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User == null)
        {
            context.Fail();
            return;
        }

        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            context.Fail();
            return;
        }

        if (!Guid.TryParse(userIdClaim.Value, out Guid userId))
        {
            context.Fail();
            return;
        }

        bool hasPermission = await dbContext.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.Roles)
            .SelectMany(r => r.Permissions)
            .AnyAsync(p => p.Key == requirement.PermissionKey);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}