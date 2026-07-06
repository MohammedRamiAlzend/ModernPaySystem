using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.SharedKernel.Domain.Identity;

public class CurrentUser
{
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public List<string> Roles { get; init; } = [];
    public List<string> Permissions { get; init; } = [];
    public Guid? DepartmentId { get; init; }
    public bool IsDepartmentHead { get; init; }
    public SubSystem? SubSystem { get; init; }

    public bool IsAuthenticated => UserId != Guid.Empty;
    public bool HasPermission(string permission) => Permissions.Contains(permission);
    public bool IsInRole(string role) => Roles.Contains(role);
}
