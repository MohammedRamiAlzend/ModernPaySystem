---
tags: [migration, di, registration]
module: all
status: draft
priority: critical
depends-on: [01-fix-transaction-services, 02-fix-archive-services]
---

# 03 — Fix DI Registrations Across All Modules

## Problem

Several critical infrastructure pieces are missing from the DI container. Even after fixing services, these gaps remain.

## Missing Registrations

### Transaction Module (`TransactionModuleRegistration.cs`)

Add after existing registrations (line 38–42):

```csharp
services.AddScoped<IRequestService, RequestService>();
services.AddScoped<IRequestTransactionService, RequestTransactionService>();
services.AddScoped<IResponseService, ResponseService>();
services.AddScoped<ITemplateService, TemplateService>();
services.AddScoped<IReportService, ReportService>();
services.AddScoped<IWebAttachmentService, WebAttachmentService>();
services.AddScoped<IDepartmentService, DepartmentService>();
services.AddMemoryCache();
```

### Archive Module (`ArchiveModuleRegistration.cs`)

Add after existing registrations (line 40–41):

```csharp
services.AddScoped<IArchiveRecordService, ArchiveRecordService>();
services.AddScoped<IArchiveDeletionWorkflowService, ArchiveDeletionWorkflowService>();
services.AddScoped<IArchiveEditWorkflowService, ArchiveEditWorkflowService>();
services.AddScoped<IArchiveFormTemplateService, ArchiveFormTemplateService>();
services.AddScoped<IArchiveLeaderService, ArchiveLeaderService>();
services.AddScoped<IArchiveRecordReportService, ArchiveRecordReportService>();
services.AddScoped<IArchiveResourceAuthorizationService, ArchiveResourceAuthorizationService>();
services.AddMemoryCache();
```

### Identity Module (`IdentityModuleRegistration.cs`)

```csharp
// All already registered — no changes needed
```

### Boot (`Program.cs`)

```csharp
// FormOptions for large file uploads
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10_737_418_240; // 10 GB
});

// Kestrel limit
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 10_737_418_240;
});
```

## Verification

```bash
dotnet build src/ModernPaySystem.Boot/ModernPaySystem.Boot.csproj
# No DI warnings at build time
```

## References

- Old: `ModernPaySystem.Infrastructure/InfrastructureServiceRegistration.cs`
- New: `IdentityModuleRegistration.cs`, `TransactionModuleRegistration.cs`, `ArchiveModuleRegistration.cs`, `Boot/Program.cs`
