---
tags: [migration, di, conflict]
module: IdentitySystem, TransactionSystem
status: draft
priority: high
depends-on: [01-fix-transaction-services, 03-fix-di-registrations]
---

# 07 — Resolve DepartmentService DI Conflict

## Problem

Both the **Identity** module and the **Transaction** module define and register `IDepartmentService`:

- `Identity.Application/Interfaces/IDepartmentService.cs`
- `Transaction.Application/Interfaces/IDepartmentService.cs`
- Both map to `DepartmentService.cs` in their respective registrations

When `Boot/Program.cs` calls both `AddIdentityModule()` and `AddTransactionModule()`, the **last registration wins**. If Transaction registers `IDepartmentService` after Identity, all Identity controllers that inject `IDepartmentService` will get the Transaction module's `DepartmentService`, which may have different behavior or different DbContext.

## Options

### Option A: Single Source of Truth (Recommended)

Move `IDepartmentService` out of both modules into `SharedKernel.Application`:

```
src/ModernPaySystem.SharedKernel.Application/Interfaces/IDepartmentService.cs
src/ModernPaySystem.SharedKernel.Infrastructure/Services/DepartmentService.cs
```

Both module registrations call `AddSharedKernel()` which already registers common services. Departments are a shared domain concept — this is architecturally correct.

**Changes**:
1. Move `IDepartmentService` → `SharedKernel.Application/Interfaces/`
2. Move `DepartmentService` → `SharedKernel.Infrastructure/Services/`
3. Register in `SharedKernelServiceRegistration.cs` not in module registrations
4. Remove from `IdentityModuleRegistration.cs` and `TransactionModuleRegistration.cs`
5. Both modules reference `SharedKernel` already — no project reference changes needed

### Option B: Separate Interface Names

Keep both but rename one to avoid shadowing:
- `IIdentityDepartmentService` in Identity
- `ITransactionDepartmentService` in Transaction

**Downside**: Code duplication, confusing, unnecessary.

### Option C: Transaction References Identity

Transaction module depends on Identity module and re-exports via its own interface using type forwarding.

**Downside**: Circular dependency risk, complex.

## Recommendation

**Go with Option A**. Departments are a cross-cutting concern (used by Identity for user-department assignment, by Transaction for request routing, by Archive for record ownership). SharedKernel is the right home.

## Migration Steps

1. Move interface file
2. Move implementation file
3. Update namespaces
4. Move registration to `SharedKernelServiceRegistration.cs`
5. Remove duplicate registrations from Identity and Transaction modules
6. Update imports in both modules' controllers and services
7. Build and verify

## Verification

```bash
dotnet build src/ModernPaySystem.Boot/ModernPaySystem.Boot.csproj
# Ensure no ambiguous reference or duplicate registration errors
```
