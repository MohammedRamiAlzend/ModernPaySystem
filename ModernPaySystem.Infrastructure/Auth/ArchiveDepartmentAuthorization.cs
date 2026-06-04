using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ModernPaySystem.Infrastructure.Auth;

public sealed record ArchiveDepartmentScope(Guid DepartmentId);
public sealed record DeleteArchiveRequestScope(Guid DepartmentId, Guid RequestId);

public sealed class DepartmentArchiveLeaderRequirement : IAuthorizationRequirement;
public sealed class DepartmentHeadRequirement : IAuthorizationRequirement;

public class DepartmentArchiveLeaderAuthorizationHandler(AppDbContext dbContext)
    : AuthorizationHandler<DepartmentArchiveLeaderRequirement, ArchiveDepartmentScope>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DepartmentArchiveLeaderRequirement requirement,
        ArchiveDepartmentScope resource)
    {
        if (!TryGetUserId(context.User, out var userId))
        {
            context.Fail();
            return;
        }

        var isLeader = await dbContext.DepartmentArchiveLeaders
            .AnyAsync(x => x.DepartmentId == resource.DepartmentId && x.UserId == userId && !x.IsDeleted);

        if (isLeader)
        {
            context.Succeed(requirement);
            return;
        }

        context.Fail();
    }

    private static bool TryGetUserId(ClaimsPrincipal? principal, out Guid userId)
    {
        userId = Guid.Empty;
        var claim = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim is not null && Guid.TryParse(claim, out userId);
    }
}

public class DepartmentHeadAuthorizationHandler(AppDbContext dbContext)
    : AuthorizationHandler<DepartmentHeadRequirement, ArchiveDepartmentScope>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DepartmentHeadRequirement requirement,
        ArchiveDepartmentScope resource)
    {
        if (!TryGetUserId(context.User, out var userId))
        {
            context.Fail();
            return;
        }

        var isHead = await dbContext.Departments
            .AnyAsync(x => x.Id == resource.DepartmentId && x.DepartmentHeadId == userId);

        if (isHead)
        {
            context.Succeed(requirement);
            return;
        }

        context.Fail();
    }

    private static bool TryGetUserId(ClaimsPrincipal? principal, out Guid userId)
    {
        userId = Guid.Empty;
        var claim = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim is not null && Guid.TryParse(claim, out userId);
    }
}

public class DeleteArchiveRequestHeadAuthorizationHandler(AppDbContext dbContext)
    : AuthorizationHandler<DepartmentHeadRequirement, DeleteArchiveRequestScope>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DepartmentHeadRequirement requirement,
        DeleteArchiveRequestScope resource)
    {
        var claim = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (claim is null || !Guid.TryParse(claim, out var userId))
        {
            context.Fail();
            return;
        }

        var isHead = await dbContext.Departments
            .AnyAsync(x => x.Id == resource.DepartmentId && x.DepartmentHeadId == userId);

        if (isHead)
        {
            context.Succeed(requirement);
            return;
        }

        context.Fail();
    }
}

public static class ArchiveAuthorizationPolicyExtensions
{
    public const string RequireDepartmentArchiveLeader = "RequireDepartmentArchiveLeader";
    public const string RequireDepartmentHead = "RequireDepartmentHead";
}
