---
tags: [migration, seeding, database]
module: all
status: draft
priority: high
depends-on: [04-add-ef-core-migrations]
---

# 05 — Add Seeding Infrastructure

## Problem

The old system seeded **departments, roles, permissions, users, and department-user links** on startup. The new system has **no seeding at all**. On first run the database tables will be empty — no admin user exists, no permissions are seeded, and the permission check endpoints return empty results.

## Files to Port

| Old File | New Location |
|---|---|
| `Seeding/EntitySeederBase.cs` | `src/Modules/IdentitySystem/.../Seeding/EntitySeederBase.cs` |
| `Seeding/IEntitySeeder.cs` | `src/Modules/IdentitySystem/.../Seeding/IEntitySeeder.cs` |
| `Seeding/SeederOrchestrator.cs` | `src/Modules/IdentitySystem/.../Seeding/IdentitySeederOrchestrator.cs` |
| `Seeding/SeedingConfiguration.cs` | `src/Modules/IdentitySystem/.../Seeding/SeedingConfiguration.cs` |
| `Seeding/SeedingServiceRegistration.cs` | `src/Modules/IdentitySystem/.../Seeding/SeedingServiceRegistration.cs` |
| `Seeding/Seeders/DefaultDataSeeder.cs` | `.../Seeders/DefaultDataSeeder.cs` |
| `Seeding/Seeders/DepartmentSeeder.cs` | `.../Seeders/DepartmentSeeder.cs` |
| `Seeding/Seeders/DepartmentUserLinkSeeder.cs` | `.../Seeders/DepartmentUserLinkSeeder.cs` |
| `Seeding/Seeders/PermissionSeeder.cs` | `.../Seeders/PermissionSeeder.cs` |
| `Seeding/Seeders/RoleSeeder.cs` | `.../Seeders/RoleSeeder.cs` |
| `Seeding/Seeders/UserSeeder.cs` | `.../Seeders/UserSeeder.cs` |

Also: `IPermissionSeederService` + `PermissionSeederService` need to be ported for runtime permission seeding.

## Decision Point

**Option A**: Single seeder orchestrator in Identity module (simpler)
**Option B**: Per-module seeding (Identity seeds departments/users/roles, Transaction seeds templates, Archive seeds config)

**Recommendation**: Option A to start — seeding is primarily identity data. Can split later.

## Seed Call in Boot Program.cs

In `Boot/Program.cs`, add after module registrations:

```csharp
if (builder.Configuration.GetValue<bool>("Seeding:Enabled"))
{
    using var scope = app.Services.CreateScope();
    var orchestrator = scope.ServiceProvider.GetRequiredService<ISeederOrchestrator>();
    await orchestrator.SeedDatabaseAsync();
}

using (var scope = app.Services.CreateScope())
{
    var permissionSeeder = scope.ServiceProvider.GetRequiredService<IPermissionSeederService>();
    await permissionSeeder.SeedPermissionsAsync();
}
```

## References

- Old: `ModernPaySystem.Infrastructure.Persistence/Seeding/`
- Old: `ModernPaySystem.Infrastructure/Services/PermissionSeederService.cs`
- Old: `ModernPaySystem.Infrastructure/Services/IWebAttachmentService.cs`
- Config key: `Seeding:Enabled` in `appsettings.json`
