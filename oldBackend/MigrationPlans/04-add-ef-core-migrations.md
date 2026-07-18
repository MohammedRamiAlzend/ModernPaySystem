---
tags: [migration, efcore, database]
module: all
status: draft
priority: critical
depends-on: []
---

# 04 — Add EF Core Migrations for Module DbContexts

## Problem

The old `AppDbContext` had an `InitialCreate` migration. The three new DbContexts (`IdentityDbContext`, `TransactionDbContext`, `ArchiveDbContext`) have **zero migrations**. EF Core will fail to apply migrations on startup.

## Action Plan

### 1. Create Migration for IdentityDbContext

```bash
dotnet ef migrations add InitialCreate \
  --project src/Modules/IdentitySystem/ModernPaySystem.Module.Identity.Infrastructure \
  --startup-project src/ModernPaySystem.Boot/ModernPaySystem.Boot.csproj \
  --context IdentityDbContext
```

### 2. Create Migration for TransactionDbContext

```bash
dotnet ef migrations add InitialCreate \
  --project src/Modules/TransactionSystem/ModernPaySystem.Module.Transaction.Infrastructure \
  --startup-project src/ModernPaySystem.Boot/ModernPaySystem.Boot.csproj \
  --context TransactionDbContext
```

### 3. Create Migration for ArchiveDbContext

```bash
dotnet ef migrations add InitialCreate \
  --project src/Modules/ArchiveSystem/ModernPaySystem.Module.Archive.Infrastructure \
  --startup-project src/ModernPaySystem.Boot/ModernPaySystem.Boot.csproj \
  --context ArchiveDbContext
```

### 4. Create Design-Time Factories (if needed)

If migrations fail due to missing design-time factories, create one per DbContext:

```
src/Modules/IdentitySystem/ModernPaySystem.Module.Identity.Infrastructure/Persistence/DesignTime/
src/Modules/TransactionSystem/ModernPaySystem.Module.Transaction.Infrastructure/Persistence/DesignTime/
src/Modules/ArchiveSystem/ModernPaySystem.Module.Archive.Infrastructure/Persistence/DesignTime/
```

Each factory should read connection string from `appsettings.json` in Boot.

### 5. Apply Migrations

```bash
dotnet ef database update \
  --project src/Modules/IdentitySystem/ModernPaySystem.Module.Identity.Infrastructure \
  --startup-project src/ModernPaySystem.Boot/ModernPaySystem.Boot.csproj \
  --context IdentityDbContext

dotnet ef database update \
  --project src/Modules/TransactionSystem/ModernPaySystem.Module.Transaction.Infrastructure \
  --startup-project src/ModernPaySystem.Boot/ModernPaySystem.Boot.csproj \
  --context TransactionDbContext

dotnet ef database update \
  --project src/Modules/ArchiveSystem/ModernPaySystem.Module.Archive.Infrastructure \
  --startup-project src/ModernPaySystem.Boot/ModernPaySystem.Boot.csproj \
  --context ArchiveDbContext
```

## Entity Model Verification

Ensure each DbContext includes the correct entity sets:

| DbContext | Entities |
|---|---|
| `IdentityDbContext` | User, Role, PermissionEntity, Department, SubSystem, SubSystemUser |
| `TransactionDbContext` | Request, RequestAttachment, RequestAuditLog, RequestRelation, RequestTemplateValues, RequestTransaction, RequestTransactionAttachment, Response, ResponseAttachment, Template, TemplateOwnership, UserTemplateOwnership, InputValue, LookUpField, LookUpFiledValues, Attachment |
| `ArchiveDbContext` | ArchiveRecord, ArchiveRecordFormInputValue, ArchiveRecordTemplateValues, ArchiveConfig, ArchiveFormTemplate, ArchiveGovernance, ArchiveAuditLog, Document, DocumentChunk, DocumentExpressions, Folder, FolderIcon, FolderPermission, PhysicalFile, AccessLevel |

## References

- Old migration: `ModernPaySystem.Infrastructure.Persistence/Migrations/20260630072955_InitialCreate.cs`
- Old DbContext: `AppDbContext.cs` (entity relationships defined via Fluent API in `OnModelCreating`)
- New: Each module DbContext needs equivalent `OnModelCreating` configuration
