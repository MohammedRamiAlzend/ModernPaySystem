---
tags: [migration, cross-cutting, logging, health]
module: Boot
status: draft
priority: medium
depends-on: []
---

# 09 — Restore Cross-Cutting Concerns

## Items

### 1. Serilog Structured Logging

Old `Program.cs` used Serilog with request logging middleware:

```csharp
// Old (Infrastructure)
builder.Services.AddSerilog((services, lc) => lc.ReadFrom.Configuration(builder.Configuration)
    .ReadFrom.Services(services));
builder.Logging.ClearProviders();
// ...
app.UseSerilogRequestLogging(opts => { /* level based on status code */ });
```

**New**: Add to `Boot/Program.cs`. Add `Serilog` and `Serilog.AspNetCore` NuGet packages.

### 2. Health Checks

Old system exposed `/healthz` and ran a `SystemHealthService` check on startup:

```csharp
builder.Services.AddHealthChecks();
// ...
app.MapHealthChecks("/healthz");
// Startup check:
using (var healthScope = app.Services.CreateScope())
{
    var healthService = healthScope.ServiceProvider.GetRequiredService<SystemHealthService>();
    var dbContext = healthScope.ServiceProvider.GetRequiredService<AppDbContext>();
    await healthService.CheckAsync(dbContext);
}
```

**New**: 
- Use `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore` NuGet
- Add `builder.Services.AddHealthChecks().AddDbContextCheck<IdentityDbContext>()` (and for each DbContext)
- Remove `SystemHealthService` dependency (use built-in instead)

### 3. Options Classes

Port these options and their registrations:

| Old | Bind Section | New Module |
|---|---|---|
| `ArchiveRecordFileUploadOptions` | `ArchiveRecordFiles` | Archive.Infrastructure/Options/ |
| `ArchiveRecordZipOptions` | `ArchiveRecordZip` | Archive.Infrastructure/Options/ |
| ~~QdrantOptions~~ | `Qdrant` | Deferred (see [[10-port-ocr-and-semantic-search]]) |
| ~~ServerSettings~~ | `ServerSettings` | Deferred |

### 4. Specification Pattern

Old file `Specifications/RequestIncludes.cs` defined EF Core include expressions. If services used it, port as a static helper class in `Transaction.Domain` or `Transaction.Infrastructure`.

### 5. Static Files / SPA

Old system served an SPA via `UseStaticFiles()` and `MapFallbackToFile("index.html")`. Add if the new architecture also serves a frontend.

### 6. DepartmentRepositoryExtensions

Port to `SharedKernel.Infrastructure/Persistence/DepartmentRepositoryExtensions.cs`:

```csharp
public static class DepartmentRepositoryExtensions
{
    Task<bool> HasChildrenAsync(this IRepositoryBase<Department, Guid>, Guid)
    Task<Result<List<Department>>> GetRootDepartmentsAsync(...)
    Task<Result<List<Department>>> GetChildrenAsync(...)
    Task<Result<List<Department>>> GetPathToRootAsync(...)
    Task<bool> WouldCreateCircularReferenceAsync(...)
}
```

These are used by `DepartmentService` for tree operations.

## Verification

```bash
dotnet build src/ModernPaySystem.Boot/ModernPaySystem.Boot.csproj
# Check /healthz endpoint returns 200
# Check logs directory is created with Serilog output
```
