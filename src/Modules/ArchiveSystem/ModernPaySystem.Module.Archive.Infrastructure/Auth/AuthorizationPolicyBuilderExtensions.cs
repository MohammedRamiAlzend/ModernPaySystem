using Microsoft.AspNetCore.Authorization;

namespace ModernPaySystem.Module.Archive.Infrastructure.Auth;

public static class AuthorizationPolicyBuilderExtensions
{
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
