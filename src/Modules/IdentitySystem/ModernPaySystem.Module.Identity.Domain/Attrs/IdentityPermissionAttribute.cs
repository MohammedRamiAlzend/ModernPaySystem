using ModernPaySystem.SharedKernel.Domain.Attrs;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Identity.Domain.Attrs;

public class IdentityPermissionAttribute(string key, PermissionType type, string? name = null, string? description = null)
    : EndpointPermissionAttribute(key, SubSystem.Shared, type, name, description);
