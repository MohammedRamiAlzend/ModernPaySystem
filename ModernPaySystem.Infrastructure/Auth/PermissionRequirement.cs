using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernPaySystem.Infrastructure.Auth;

public class PermissionRequirement(string permissionKey) : IAuthorizationRequirement
{
    public string PermissionKey => permissionKey;
}
