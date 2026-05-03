using Microsoft.AspNetCore.Authorization;

namespace ModernPaySystem.Infrastructure.Auth;

public static class AuthorizationPolicyBuilderExtensions
{
    public static AuthorizationPolicyBuilder RequirePermission(this AuthorizationPolicyBuilder builder, string permissionKey)
    {
        builder.Requirements.Add(new PermissionRequirement(permissionKey));
        return builder;
    }
}
