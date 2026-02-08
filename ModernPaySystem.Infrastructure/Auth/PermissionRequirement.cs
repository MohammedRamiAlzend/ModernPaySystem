using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace ModernPaySystem.Infrastructure.Auth;

public class PermissionRequirement(string permissionKey) : IAuthorizationRequirement
{
    public string PermissionKey => permissionKey;
}