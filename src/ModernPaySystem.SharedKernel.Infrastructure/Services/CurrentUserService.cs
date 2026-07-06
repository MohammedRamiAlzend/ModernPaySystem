using Microsoft.AspNetCore.Http;
using ModernPaySystem.SharedKernel.Application.Interfaces;
using ModernPaySystem.SharedKernel.Domain.Entities;
using ModernPaySystem.SharedKernel.Domain.Identity;
using System.Security.Claims;

namespace ModernPaySystem.SharedKernel.Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public CurrentUser GetCurrentUser()
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return new CurrentUser();
        }

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return new CurrentUser();
        }

        var permissions = user.FindAll("permission").Select(c => c.Value).ToList();
        var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var subSystemStr = user.FindFirst("System")?.Value;

        SubSystem? subSystem = null;
        if (!string.IsNullOrEmpty(subSystemStr) && Enum.TryParse<SubSystem>(subSystemStr, out var parsed))
        {
            subSystem = parsed;
        }

        var isDepartmentHead = user.FindFirst("IsDepartmentHead")?.Value == "true";
        var departmentIdStr = user.FindFirst("DepartmentId")?.Value;
        Guid? departmentId = null;
        if (!string.IsNullOrEmpty(departmentIdStr) && Guid.TryParse(departmentIdStr, out var deptId))
        {
            departmentId = deptId;
        }

        return new CurrentUser
        {
            UserId = userGuid,
            UserName = user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty,
            Roles = roles,
            Permissions = permissions,
            DepartmentId = departmentId,
            IsDepartmentHead = isDepartmentHead,
            SubSystem = subSystem
        };
    }

    public Guid GetCurrentUserId()
    {
        var user = httpContextAccessor.HttpContext?.User;
        var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Guid.Empty;
        }
        return userGuid;
    }

    public bool IsAuthenticated()
    {
        return httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
    }

    public bool HasPermission(string permission)
    {
        return httpContextAccessor.HttpContext?.User?.FindAll("permission")
            .Any(c => c.Value == permission) == true;
    }
}
