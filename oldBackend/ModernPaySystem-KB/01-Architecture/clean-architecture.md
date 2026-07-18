# Clean Architecture — ModernPaySystem

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                  ModernPaySystem (Web API)                       │
│            Controllers, Middleware, DI, Auth                     │
├─────────────────────────────────────────────────────────────────┤
│           ModernPaySystem.Application                            │
│   Interfaces (contracts), DTOs, Specifications                  │
├─────────────────────────────────────────────────────────────────┤
│              ModernPaySystem.Domain                              │
│         Entities, Value Objects, DTOs, Commons, Business Rules   │
├─────────────────────────────────────────────────────────────────┤
│           ModernPaySystem.Infrastructure                         │
│       Service Implementations, Auth, OCR, External Integrations  │
├─────────────────────────────────────────────────────────────────┤
│      ModernPaySystem.Infrastructure.Persistence                  │
│  EF Core + Npgsql, RepositoryBase<T>, UnitOfWork, Migrations     │
├─────────────────────────────────────────────────────────────────┤
│          Library Projects (no layer hierarchy)                   │
│  ExpressionBuilderLib · FileManager · NumberSpelling · OcrReader  │
├─────────────────────────────────────────────────────────────────┤
│              External (PostgreSQL, File System)                   │
└─────────────────────────────────────────────────────────────────┘
```

Dependency direction: **Outer → Inner**. Inner layers know nothing about outer layers.

---

## Layer Responsibilities

| Layer | Responsibility | Knows About |
|-------|---------------|-------------|
| **Domain** | Entities (Entity<T>), value objects, DTOs, expression helpers, Result<T>, business rules | Nothing (except library projects and System.*) |
| **Application** | Interface definitions (services, repositories), DTOs, application-level contracts | Domain |
| **Infrastructure** | Service implementations, JWT auth, OCR integration, file management, number spelling | Application interfaces, Domain, Library projects |
| **Persistence** | EF Core + Npgsql DbContext, generic RepositoryBase, UnitOfWork, interceptors, migrations, seeding | Application, Domain |
| **API (ModernPaySystem)** | Controllers, middleware, routing, DI wiring, Scalar API docs, authentication | Application, Infrastructure, Persistence |
| **Libraries** | Utility libraries referenced by multiple layers | No architectural layer — consumed by Domain/Persistence/Infrastructure |

---

## Request Flow

```
HTTP Request
    │
    ▼
Controller (API)
    │  Injects I*Service from Infrastructure
    │  Applies [EndpointPermission] / [Authorize]
    │
    ▼
Service (Infrastructure)
    │  Injects IUnitOfWork, library services
    │  Builds Expression filters via ExpressionBuilderLib
    │
    ▼
RepositoryBase<T> (Persistence)
    │  Uses AppDbContext (Npgsql)
    │  Applies Expression filters, pagination, ordering
    │
    ▼
EF Core DbContext → PostgreSQL
    │
    ▼
Entity returned → mapped to Domain DTO → wrapped in Result<T>
    │
    ▼
Controller returns Result.ToActionResult()
```

---

## UnitOfWork as Mandatory Gateway

All data access MUST go through `IUnitOfWork` or a domain-specific sub-interface (`IUnitOfWorkArchiving`, `IUnitOfWorkTransactionSystem`). Sub-interfaces inherit the transaction contract and expose only the repositories relevant to their domain.

Direct injection of `IRepositoryBase<TEntity, TKey>` in services is **forbidden**.

```
Service (Infrastructure)
    │
    ▼
IUnitOfWork                          ← ONLY gateway to repositories
    │
    ├── IRepositoryBase<User, Guid> Users
    ├── IRepositoryBase<Request, Guid> Requests
    ├── IRepositoryBase<ArchiveRecord, Guid> ArchiveRecords
    └── ... (one property per entity)
```

### Rule: Register every entity in IUnitOfWork

When adding a new entity to the Domain:

1. Create the entity in `Domain/Entities/{Category}/{Entity}.cs`
2. Add a corresponding DbSet in `AppDbContext`
3. **Add an `IRepositoryBase<{Entity}, Guid> {Entities} { get; }` property to `IUnitOfWork`**
4. Implement the property in `UnitOfWork.cs`
5. The entity is now accessible via `unitOfWork.{Entities}.GetAsync(...)`

### Rule: Services access repositories ONLY via UnitOfWork

```csharp
// ❌ FORBIDDEN — direct repository injection
public class RequestService(IRepositoryBase<Request, Guid> requestRepo) : IRequestService

// ✅ CORRECT — UnitOfWork is the only gateway
public class RequestService(IUnitOfWork unitOfWork) : IRequestService
{
    public async Task<Result<RequestDto>> GetByIdAsync(Guid id)
    {
        var result = await unitOfWork.Requests.GetByIdAsync(id);
        // ...
    }
}
```

### Why this rule exists

- **Consistent transaction boundary**: `IUnitOfWork.SaveChangesAsync()` commits all changes atomically
- **Single registration point**: New entities are registered once in `IUnitOfWork`
- **Cleaner service constructors**: Services inject one dependency (`IUnitOfWork`) instead of many repositories
- **Easier testing**: Mock one `IUnitOfWork` instead of N repositories
- **Sub-interfaces allowed**: Domain-specific sub-interfaces (e.g., `IUnitOfWorkArchiving`) may be used for ISP compliance, but they must inherit the transaction contract from the base `IUnitOfWork`

---

## Dependency Flow

```
Domain         ←  libraries (ExpressionBuilderLib)
Application    ←  Domain
Infrastructure ←  Application + Domain + libraries
Persistence    ←  Application + Domain + libraries
API            ←  Application + Infrastructure + Persistence
```

### Why Application only has Interfaces

The Application project is a **contract-only layer**. It defines:
- `IRequestService`, `IUserService`, `IRoleService`, etc.
- `IRepositoryBase<TEntity, TKey>` (via `ModernPaySystem.Application.Repos`)
- `IUnitOfWork` (via `ModernPaySystem.Infrastructure.Persistence`)
- `ITokenService`, `IAuthenticationService`, etc.

Implementations live in **Infrastructure** (services) and **Persistence** (repositories). This allows Infrastructure to contain complex service logic without leaking into Application.

### Why DTOs live in Domain

DTOs are in `ModernPaySystem.Domain.DTOs` because:
1. They are shared between all layers (API reads them, Infrastructure maps to them, Domain entities produce them)
2. They are pure data — no framework dependencies
3. Placing them in Application would create circular references when Infrastructure needs them

---

## Database Technology

| Component | Technology |
|-----------|-----------|
| **Database** | PostgreSQL |
| **ORM** | EF Core 10 + Npgsql.EntityFrameworkCore.PostgreSQL |
| **Migrations** | EF Core migrations in Persistence project |
| **Seeding** | Custom `ISeederOrchestrator` + `IPermissionSeederService` |

The solution uses **PostgreSQL** (not SQL Server). The `AppDbContext` is configured with `UseNpgsql`.

---

## Project Ownership Matrix

| Concept | Owned By | Defined In | Namespace |
|---------|----------|-----------|-----------|
| Entity | Domain | `Domain/Entities/` | `ModernPaySystem.Domain.Entities.*` |
| Base Entity | Domain | `Domain/Entities/Abstraction/Entity.cs` | `ModernPaySystem.Domain.Entities.Abstraction` |
| Value Object | Domain | `Domain/Entities/*` | Same as entity |
| Expression Helper | Domain | `Domain/Entities/*Expressions.cs` | Same as entity |
| DTO | Domain | `Domain/DTOs/` | `ModernPaySystem.Domain.DTOs` |
| Result<T> | Domain | `Domain/Commons/` | `ModernPaySystem.Domain.Commons` |
| Repository Interface | Application | `Application/Repos/` | `ModernPaySystem.Application.Repos` |
| Service Interface | Application | `Application/Interfaces/` | `ModernPaySystem.Application.Interfaces` |
| Service Implementation | Infrastructure | `Infrastructure/Services/` | `ModernPaySystem.Infrastructure.Services` |
| Repository Implementation | Persistence | `Persistence/Repos/` (accessed via IUnitOfWork) | `ModernPaySystem.Infrastructure.Persistence.Repos` |
| DbContext | Persistence | `Persistence/AppDbContext.cs` | `ModernPaySystem.Infrastructure.Persistence` |
| IUnitOfWork interface | Persistence | `Persistence/UnitOfWork/IUnitOfWork.cs` | `ModernPaySystem.Infrastructure.Persistence.UnitOfWork` |
| UnitOfWork implementation | Persistence | `Persistence/UnitOfWork/UnitOfWork.cs` | `ModernPaySystem.Infrastructure.Persistence.UnitOfWork` |
| Interceptors | Persistence | `Persistence/Interceptors/` | `ModernPaySystem.Infrastructure.Persistence.Interceptors` |
| Seeding | Persistence | `Persistence/Seeding/` | `ModernPaySystem.Infrastructure.Persistence.Seeding` |
| Controller | API | `Controllers/{Group}Controllers/` | `ModernPaySystem.Controllers` / `ModernPaySystem.Controllers.{Group}` |
| Middleware | — | Not currently used | — |
| DI Registration | API | `Program.cs` + extension methods | Global usings |

### Library Ownership

| Library | Owned By | Namespace |
|---------|----------|-----------|
| ExpressionBuilderLib | Standalone | `ExpressionBuilderLib.src.Core`, `.Utilities`, `.Core.Enums` |
| FileManager | Standalone | `FileManager.Abstractions`, `FileManager.Services`, etc. |
| NumberSpelling | Standalone | `NumberSpelling` |
| OcrReader | Standalone | `OcrReader` |

**Rule:** Libraries have no place in the architectural layer hierarchy. They are referenced directly by the projects that need them via `<ProjectReference>`.

---

## Clean Architecture Rules

### Rule 1: Domain Is the Root
Domain defines the data model (`Entity<TKey>`), DTOs, Result<T>, and business rules. It has zero HTTP or infra dependencies.

### Rule 2: Application Defines Contracts Only
Application defines interfaces (services, repositories). It contains ZERO implementations.

### Rule 3: Infrastructure Implements Service Interfaces
Services live in `Infrastructure/Services/`. They implement `I*Service` interfaces from Application.

### Rule 4: Persistence Implements Repository Interfaces
Repositories live in `Persistence/Repos/`. The generic `RepositoryBase<TEntity, TKey>` implements `IRepositoryBase<TEntity, TKey>` from Application.

### Rule 5: Controllers Are Thin
Controllers validate HTTP, call one service method, return `Result.ToActionResult()`. No business logic, no EF Core queries.

### Rule 6: Libraries Are Layer-Agnostic
Libraries are referenced by Domain, Persistence, and Infrastructure as needed. They have no knowledge of application layers.

### Rule 7: PostgreSQL + Npgsql
The persistence layer uses PostgreSQL via Npgsql. No SQL Server packages in Persistence project.

### Rule 8: Scalar for API Documentation
Use **Scalar.AspNetCore** (not Swashbuckle/Swagger UI). Configured in `Program.cs` via `AddOpenApi("v1")` with `BearerSecuritySchemeTransformer`.

### Rule 9: UnitOfWork Is the Only Data Access Gateway
All data access goes through IUnitOfWork or domain-specific sub-interfaces. Services never inject IRepositoryBase<T, TKey> directly. IUnitOfWork exposes one IRepositoryBase<T, TKey> property per entity. Sub-interfaces (e.g., `IUnitOfWorkArchiving`) are allowed for Interface Segregation.

---

## Common Anti-Patterns

| Anti-Pattern | Why It's Wrong | Correct Approach |
|-------------|---------------|-----------------|
| **Service implementation in Application** | Application is contracts-only | Services go in Infrastructure |
| **EF Core queries in controllers** | Violates SRP | Delegate to Infrastructure service |
| **DTOs in Application** | Domain needs DTOs too; creates circular refs | DTOs live in Domain.DTOs |
| **IQueryable from RepositoryBase in service** | Leaks persistence concerns | RepositoryBase returns Result<T> |
| **Domain referencing EF Core attributes** | Keeps Domain impure | Use [Key] only in Domain's own `Entity<T>` base |
| **Controllers referencing Persistence directly** | Bypasses service layer | Inject infrastructure service interfaces |
| **Business logic in DTOs** | DTOs are data-only | Logic stays on Domain entities |
| **Direct DbContext in services** | Bypasses RepositoryBase pattern | Use IUnitOfWork + RepositoryBase |
| **Injecting IRepositoryBase directly in services** | Bypasses UnitOfWork, scattered dependencies | Inject IUnitOfWork, access repositories via properties |

---

## Key Architectural Insights from Actual Code

### Entity<T> Base Class
```csharp
// ModernPaySystem.Domain/Entities/Abstraction/Entity.cs
public class Entity<TKey>
{
    [Key]  // <-- EF Core attribute allowed on Domain's own base entity
    public virtual TKey Id { get; set; }
}
```

This is a pragmatic exception to "pure domain" — the `[Key]` attribute is on the Domain's own base class because all entities need it.

### Generic RepositoryBase with Expression Filtering
```csharp
// RepositoryBase<Request, Guid> supports:
await uow.Requests.GetPagedAsync(page, pageSize,
    filter: r => r.Status == RequestStatus.Pending,
    transform: q => q.Include(r => r.Requester),
    additionalFilters: new List<Expression<Func<Request, bool>>> { ... },
    logicalOperator: LogicalOperator.And);
```

### Services in Infrastructure, Not Application
```csharp
// Application/Interfaces/IRequestService.cs — interface only
public interface IRequestService
{
    Task<Result<PagedList<RequestDto>>> GetPagedAsync(RequestPagedFilterDto? filterDto);
    // ...
}

// Infrastructure/Services/RequestService.cs — implementation
public class RequestService(IUnitOfWork, ILogger, IWebAttachmentService, ...) : IRequestService
{
    // Full implementation with EF Core, caching, file I/O
}
```

### UnitOfWork — The Exclusive Gateway to Repositories
```csharp
// Infrastructure.Persistence/UnitOfWork/IUnitOfWork.cs
public interface IUnitOfWork
{
    AppDbContext Context { get; }
    IRepositoryBase<User, Guid> Users { get; }
    IRepositoryBase<Request, Guid> Requests { get; }
    IRepositoryBase<ArchiveRecord, Guid> ArchiveRecords { get; }
    // ... one property per entity
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

// Usage in Infrastructure services:
public class RequestService(IUnitOfWork unitOfWork) : IRequestService
{
    public async Task<Result<PagedList<RequestDto>>> GetPagedAsync(...)
    {
        var result = await unitOfWork.Requests.GetPagedAsync(page, pageSize, ...);
    }
}
```

### Controllers in Sub-Namespaces by Feature
```csharp
// Controllers/ArchivingControllers/ArchiveRecordsController.cs
namespace ModernPaySystem.Controllers.ArchivingControllers;

// Controllers/TransactionsSystemControllers/RequestsController.cs
namespace ModernPaySystem.Controllers.TransactionsSystemControllers;
```

### Custom Permission Attributes
```csharp
[EndpointPermission("requests.get-by-id", SubSystem.TransactionSystem, PermissionType.Read)]
[Authorize]
```

### Library Integration Pattern
```csharp
// ExpressionBuilderLib for dynamic filtering
ExpressionCombiner.AndAll(filters.ToArray());

// FileManager for file I/O
IFileManager fileManager;  // injected in service

// OcrReader for OCR
services.AddOcrTesseract();  // DI extension

// NumberSpelling for Arabic text
services.AddNumberSpelling();
```
