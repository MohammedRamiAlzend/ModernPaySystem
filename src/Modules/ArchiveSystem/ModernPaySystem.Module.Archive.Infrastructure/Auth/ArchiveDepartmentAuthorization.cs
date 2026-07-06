using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Module.Archive.Application.Interfaces;
using ModernPaySystem.Module.Archive.Infrastructure.Persistence;
using System.Security.Claims;

namespace ModernPaySystem.Module.Archive.Infrastructure.Auth;

public sealed record ArchiveDepartmentScope(Guid DepartmentId);
public sealed record DeleteArchiveRequestScope(Guid DepartmentId, Guid RequestId);

public sealed class DepartmentArchiveLeaderRequirement : IAuthorizationRequirement;
public sealed class DepartmentHeadRequirement : IAuthorizationRequirement;

public class DepartmentArchiveLeaderAuthorizationHandler(ArchiveDbContext dbContext)
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

public class DepartmentHeadAuthorizationHandler(IArchiveAuthorizationService authService)
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

        var result = await authService.IsDepartmentHeadAsync(userId, resource.DepartmentId);
        if (result.IsSuccess && result.Value)
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

public class DeleteArchiveRequestHeadAuthorizationHandler(IArchiveAuthorizationService authService)
    : AuthorizationHandler<DepartmentHeadRequirement, DeleteArchiveRequestScope>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DepartmentHeadRequirement requirement,
        DeleteArchiveRequestScope resource)
    {
        if (!TryGetUserId(context.User, out var userId))
        {
            context.Fail();
            return;
        }

        var result = await authService.IsDepartmentHeadAsync(userId, resource.DepartmentId);
        if (result.IsSuccess && result.Value)
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

public static class ArchiveAuthorizationPolicyExtensions
{
    public const string RequireDepartmentArchiveLeader = "RequireDepartmentArchiveLeader";
    public const string RequireDepartmentHead = "RequireDepartmentHead";
}
