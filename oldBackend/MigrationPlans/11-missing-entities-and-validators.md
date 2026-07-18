---
tags: [migration, entities, validators]
module: all
status: draft
priority: medium
depends-on: []
---

# 11 — Port Missing Entities, DTOs, and Validators

## Problem

Several domain types from the old monolith were overlooked during the initial module extraction.

## Items to Port

### 1. Fast Operation Entities (PaySystemEntities)

**8 entities** from `Domain/Entities/PaySystemEntities/FastOperations/` are completely absent:

| Entity | Description | Likely Module |
|---|---|---|
| `Client.cs` | Client/customer data | Transaction or new module |
| `Gender.cs` | Gender lookup | SharedKernel |
| `Gov.cs` | Government entity lookup | SharedKernel |
| `KindShip.cs` | Kinship relationship type | SharedKernel |
| `National.cs` | Nationality lookup | SharedKernel |
| `Operations.cs` | Financial operations | Transaction |
| `OperationServiceType.cs` | Service type for operations | Transaction |
| `OperationStatus.cs` | Status enum for operations | Transaction |

**Decision needed**: Are these still in use? If yes, determine module placement. If dead code, skip.

### 2. IEntityDesc Interface

Old `Domain/Entities/Abstraction/IEntityDesc.cs` (interface with `Desc` property) — **dead code**? No entity implements it. Skip unless needed.

### 3. UserExpressions.cs, RoleExpressions.cs, LookUpFiledValuesExpressions.cs

Old `Domain/Entities/SharedEntities/` expression/partial files — these may be EF Core configuration or computed properties. Check if they contain actual logic and port accordingly.

### 4. FluentValidation Validators

**4 files** from `Application/Validators/`:

| Validator | For | New Location |
|---|---|---|
| `CreateDepartmentDtoValidator.cs` | Department creation | `Identity.Application/Validators/` |
| `CreateRoleDtoValidator.cs` | Role creation | `Identity.Application/Validators/` |
| `CreateUserDtoValidator.cs` | User creation | `Identity.Application/Validators/` |
| `DependencyInjection.cs` | Validator registration | `Identity.Application/Validators/` |

Add `FluentValidation.DependencyInjectionExtensions` NuGet and register:

```csharp
// In Identity module registration or a separate extension
services.AddValidatorsFromAssemblyContaining<CreateUserDtoValidator>();
```

### 5. DepartmentDto in Wrong Location

The new `SharedKernel.Domain/Entities/DepartmentDto.cs` is a DTO but placed in the `Entities/` folder. Move to `SharedKernel.Domain/DTOs/DepartmentDto.cs` for consistency with other DTOs.

## Verification

```bash
dotnet build src/ModernPaySystem.Boot/ModernPaySystem.Boot.csproj
# No missing type errors
```
