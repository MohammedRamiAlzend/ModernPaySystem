---
tags: [migration, authorization, security]
module: IdentitySystem
status: draft
priority: high
depends-on: []
---

# 08 — Restore Permission-Based Authorization

## Problem

Over **50 controller endpoints** are decorated with `[EndpointPermission("...")]` attributes. In the old system, these were enforced by `PermissionAuthorizationHandler` which checked the user's role permissions against the required permission key. **This handler is not ported** — all permission attributes are decorative only.

## Files to Port

| Old File | New Location |
|---|---|
| `Auth/PermissionRequirement.cs` | `SharedKernel.Infrastructure/Auth/PermissionRequirement.cs` |
| `Auth/PermissionAuthorizationHandler.cs` | `Identity.Infrastructure/Auth/PermissionAuthorizationHandler.cs` |
| `Auth/AuthorizationPolicyBuilderExtensions.cs` | Already exists in `Archive.Infrastructure/Auth/` — add generic permission extension |
| `Attrs/EndpointPermissionAttribute.cs` | Already exists in `SharedKernel.Domain/Attrs/EndpointPermissionAttribute.cs` ✅ |

## PermissionAuthorizationHandler Logic

```csharp
public class PermissionAuthorizationHandler(IdentityDbContext dbContext)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Extract userId from claims
        // Query: dbContext.Users.Where(u => u.Id == userId)
        //   .SelectMany(u => u.Roles)
        //   .SelectMany(r => r.Permissions)
        //   .AnyAsync(p => p.Key == requirement.PermissionKey)
        // If found: context.Succeed(requirement)
        // Else: context.Fail()
    }
}
```

## Registration

In `IdentityModuleRegistration.cs` or a new `AuthorizationServiceRegistration.cs`:

```csharp
services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
```

## Verification

Test by calling an endpoint with `[EndpointPermission]` using a user who lacks the required permission — should get `403 Forbidden`.

## References

- Old: `ModernPaySystem.Infrastructure/Auth/PermissionAuthorizationHandler.cs`
- Old: `ModernPaySystem.Infrastructure/Auth/PermissionRequirement.cs`
- Old: `ModernPaySystem.Infrastructure/Auth/AuthorizationPolicyBuilderExtensions.cs`
