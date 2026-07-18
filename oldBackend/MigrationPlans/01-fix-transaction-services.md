---
tags: [migration, transaction, services]
module: TransactionSystem
status: draft
priority: critical
depends-on: []
---

# 01 — Implement Missing Transaction Service Implementations

## Problem

The Transaction module has **6 controllers** that inject services with no registered implementation. The app will throw `InvalidOperationException` at runtime when these controllers are reached.

## Missing Implementations

| Interface | Controller(s) Using It | Existing Impl in Old |
|---|---|---|
| `IRequestService` | `RequestsController` | `RequestService.cs` |
| `IRequestTransactionService` | `RequestTransactionsController` | `RequestTransactionService.cs` |
| `IResponseService` | `ResponsesController` | `ResponseService.cs` |
| `ITemplateService` | `TemplatesController` | `TemplateService.cs` |
| `IReportService` | `ReportsController` | `ReportService.cs` |
| `IWebAttachmentService` | (via DI in services) | `WebAttachmentService.cs` |

## Action Plan

1. **Create** `src/Modules/TransactionSystem/ModernPaySystem.Module.Transaction.Infrastructure/Services/RequestService.cs`
   - Port from `ModernPaySystem.Infrastructure/Services/RequestService.cs`
   - Update namespaces, use `ITransactionUnitOfWork` instead of `IUnitOfWork`
   - Use `TransactionDbContext` via UoW, not `AppDbContext`

2. **Create** `src/Modules/TransactionSystem/ModernPaySystem.Module.Transaction.Infrastructure/Services/RequestTransactionService.cs`
   - Port from `ModernPaySystem.Infrastructure/Services/RequestTransactionService.cs`
   - Same namespace/DI updates

3. **Create** `src/Modules/TransactionSystem/ModernPaySystem.Module.Transaction.Infrastructure/Services/ResponseService.cs`
   - Port from `ModernPaySystem.Infrastructure/Services/ResponseService.cs`

4. **Create** `src/Modules/TransactionSystem/ModernPaySystem.Module.Transaction.Infrastructure/Services/TemplateService.cs`
   - Port from `ModernPaySystem.Infrastructure/Services/TemplateService.cs`

5. **Create** `src/Modules/TransactionSystem/ModernPaySystem.Module.Transaction.Infrastructure/Services/ReportService.cs`
   - Port from `ModernPaySystem.Infrastructure/Services/ReportService.cs`

6. **Create** `src/Modules/TransactionSystem/ModernPaySystem.Module.Transaction.Infrastructure/Services/WebAttachmentService.cs`
   - Port from `ModernPaySystem.Infrastructure/Services/WebAttachmentService.cs`

7. **Register** all 6 in `TransactionModuleRegistration.cs`

## Porting Notes

- Replace `IUnitOfWork` → `ITransactionUnitOfWork`
- Replace `AppDbContext` → `TransactionDbContext`
- Replace `_httpContextAccessor` / manual user extraction → `IHttpContextServiceManager.GetCurrentUserId()`
- Use `ILogger<T>` instead of `ILogger<T>` (same pattern, update namespace)
- Return types using `Result<T>` from `SharedKernel.Domain.Commons`
- Entity types from `Transaction.Domain.Entities` not `Domain.Entities.TransactionSystemEntities`

## Verification

```bash
dotnet build src/ModernPaySystem.Boot/ModernPaySystem.Boot.csproj
# No DI resolution errors at build time; smoke-test by hitting an endpoint
```

## References

- Old: `ModernPaySystem.Infrastructure/Services/RequestService.cs`
- New: `src/Modules/TransactionSystem/ModernPaySystem.Module.Transaction.Infrastructure/Services/`
- Registration target: `TransactionModuleRegistration.cs` line 38–43
