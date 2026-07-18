using Microsoft.AspNetCore.Authorization;

namespace ModernPaySystem.Infrastructure.Auth;

public static class AuthorizationPolicyBuilderExtensions
{
    public static AuthorizationPolicyBuilder RequirePermission(this AuthorizationPolicyBuilder builder, string permissionKey)
    {
        builder.Requirements.Add(new PermissionRequirement(permissionKey));
        return builder;
    }

    public static AuthorizationPolicyBuilder RequireDepartmentArchiveLeader(this AuthorizationPolicyBuilder builder)
    {
        builder.Requirements.Add(new DepartmentArchiveLeaderRequirement());
        return builder;
    }

    public static AuthorizationPolicyBuilder RequireDepartmentHead(this AuthorizationPolicyBuilder builder)
    {
        builder.Requirements.Add(new DepartmentHeadRequirement());
        return builder;
    }
}
