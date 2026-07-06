---
tags: [migration, archive, services]
module: ArchiveSystem
status: draft
priority: critical
depends-on: []
---

# 02 — Implement Missing Archive Service Implementations

## Problem

The Archive module has **5 controllers** that inject services with no registered implementation. Interfaces exist but infrastructure implementations were never ported.

## Missing Implementations

| Interface | Controller(s) Using It | Existing Impl in Old |
|---|---|---|
| `IArchiveRecordService` | `ArchiveRecordsController` | `ArchiveRecordService.cs` |
| `IArchiveDeletionWorkflowService` | `ArchiveDeletionRequestsController` | `ArchiveDeletionWorkflowService.cs` |
| `IArchiveEditWorkflowService` | `ArchiveEditRequestsController` | `ArchiveEditWorkflowService.cs` |
| `IArchiveFormTemplateService` | (no controller — yet) | `ArchiveFormTemplateService.cs` |
| `IArchiveLeaderService` | (via DepartmentsController) | `ArchiveLeaderService.cs` |
| `IArchiveRecordReportService` | `ArchiveReportController` | `ArchiveRecordReportService.cs` |
| `IArchiveResourceAuthorizationService` | (referenced by other services) | `ArchiveResourceAuthorizationService.cs` |

## Action Plan

1. **Create** `src/Modules/ArchiveSystem/ModernPaySystem.Module.Archive.Infrastructure/Services/ArchiveRecordService.cs`
   - Largest service — port file management, ZIP streaming, pagination, audit logging
   - Use `IArchiveUnitOfWork`, `ArchiveDbContext`

2. **Create** `Services/ArchiveDeletionWorkflowService.cs`
   - Port from old, adapt to modular DI

3. **Create** `Services/ArchiveEditWorkflowService.cs`
   - Port from old

4. **Create** `Services/ArchiveFormTemplateService.cs`
   - Port from old `DynamicFormService.cs` (note: old name mismatch)

5. **Create** `Services/ArchiveLeaderService.cs`
   - Port from old

6. **Create** `Services/ArchiveRecordReportService.cs`
   - Port from old — report queries

7. **Create** `Services/ArchiveResourceAuthorizationService.cs`
   - Port from old — resource-level auth checks

8. **Register** all 7 in `ArchiveModuleRegistration.cs`

## Porting Notes

- `IArchiveUnitOfWork` provides per-entity repos instead of monolithic `IUnitOfWork`
- Replace `IRepositoryBase<T, Guid>` with module-specific repos from UoW
- Audit interceptor is already registered in DbContext — services no longer need to manually set audit fields
- File operations (streaming, ZIP) remain the same pattern

## Verification

```bash
dotnet build src/ModernPaySystem.Boot/ModernPaySystem.Boot.csproj
# Then test archive endpoints
```

## References

- Old: `ModernPaySystem.Infrastructure/Services/Archive*.cs`
- Registration target: `ArchiveModuleRegistration.cs`
