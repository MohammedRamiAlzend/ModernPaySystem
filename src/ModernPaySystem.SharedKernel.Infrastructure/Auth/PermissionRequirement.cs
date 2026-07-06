using Microsoft.AspNetCore.Authorization;

namespace ModernPaySystem.SharedKernel.Infrastructure.Auth;

public class PermissionRequirement(string permissionKey) : IAuthorizationRequirement
{
    public string PermissionKey => permissionKey;
}
