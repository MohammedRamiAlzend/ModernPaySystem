# Dependency Rules — ModernPaySystem

## Dependency Flow Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                    ModernPaySystem (Web API)                         │
│   References: Application, Infrastructure, Persistence               │
│   (Composition Root — wires everything via Program.cs)              │
├─────────────────────────────────────────────────────────────────────┤
│              ModernPaySystem.Application                             │
│   References: Domain                                                 │
│   Contains: ZERO implementations — only interfaces                   │
├─────────────────────────────────────────────────────────────────────┤
│                 ModernPaySystem.Domain                               │
│   References: (none — except library projects)                       │
│   "The Root Layer" — entities, DTOs, Result<T>, commons             │
├─────────────────────────────────────────────────────────────────────┤
│            ModernPaySystem.Infrastructure                             │
│   References: Application, Domain, library projects                  │
│   Contains: ALL service implementations (IUserService → UserService)│
├─────────────────────────────────────────────────────────────────────┤
│       ModernPaySystem.Infrastructure.Persistence                       │
│   References: Application, Domain, library projects                   │
│   Contains: DbContext, RepositoryBase<T>, UnitOfWork, Seeders        │
├─────────────────────────────────────────────────────────────────────┤
│            Library Projects (no layer hierarchy)                      │
│   ExpressionBuilderLib · FileManager · NumberSpelling · OcrReader    │
└─────────────────────────────────────────────────────────────────────┘
```

---

## Allowed Dependencies

### Table: Project → Allowed References

| Project | May Reference | Rationale |
|---------|--------------|-----------|
| **Domain** | Nothing (except `system.*` namespaces and library projects) | Must remain pure |
| **Application** | Domain | Needs entities, DTOs, Result<T> for interface contracts |
| **Infrastructure** | Application, Domain, library projects | Implements service interfaces from Application |
| **Persistence** | Application, Domain, library projects | Implements `IRepositoryBase<T>`; maps entities to DB |
| **API** | Application, Infrastructure, Persistence | Composition root — registers and wires all implementations |
| **Libraries** | Nothing (standalone | Pure utilities with no application knowledge |

### NuGet package rules per layer

| Layer | Allowed Packages |
|-------|-----------------|
| Domain | None (zero-package policy). Exception: `Microsoft.AspNetCore.Http` (for IFormFile in DTOs) |
| Application | None (contract-only project). Exception: `Microsoft.AspNetCore.Http` (for IFormFile) |
| Infrastructure | JWT auth packages, EF Core (for Include/ThenInclude in services), BCrypt, Serilog, Npgsql, Tesseract, library packages |
| Persistence | `Microsoft.EntityFrameworkCore`, `Npgsql.EntityFrameworkCore.PostgreSQL`, EF tools, Bogus (for seeding), library packages |
| API | `Scalar.AspNetCore`, `Serilog.AspNetCore`, JWT auth middleware packages, Swashbuckle alternative via Scalar |
| Libraries | Only packages required for their specific utility (e.g., Tesseract for OcrReader, NumberToArabicText for NumberSpelling) |

---

## Forbidden Dependencies

### ❌ Never do these

| Violation | Example | Why It's Wrong |
|-----------|---------|----------------|
| Application → Persistence | `using ModernPaySystem.Infrastructure.Persistence` | Application has NO implementations |
| Application → Infrastructure | `using ModernPaySystem.Infrastructure` | Application has NO implementations |
| Infrastructure → API | `using ModernPaySystem` | Infrastructure should not know HTTP layer |
| Persistence → API | `using ModernPaySystem` | Persistence should not know HTTP layer |
| Persistence → Infrastructure | `using ModernPaySystem.Infrastructure` | Creates cross-infrastructure coupling |
| API → Domain (with exceptions) | `using ModernPaySystem.Domain` | Controllers should reference DTOs, not entities directly |
| Infrastructure → IRepositoryBase | `using ModernPaySystem.Application.Repos` in service constructor | Services must use IUnitOfWork, not direct repository injection |
| Application → IUnitOfWork | `using ModernPaySystem.Infrastructure.Persistence.UnitOfWork` in Application | IUnitOfWork lives in Persistence; Application only knows IRepositoryBase interface (for type constraints) |

### API → Domain Exceptions

The API project IS allowed to reference Domain in exactly these cases:

```
✅ Using Domain DTOs as action parameters/return types (e.g., LoginRequest, RequestDto)
✅ Using Domain enums in route constraints or validation
✅ Referencing Domain in Program.cs for entity registration
✅ Referencing Domain exceptions in global error middleware (if used)
```

The API project is NOT allowed to:

```
❌ Injecting RepositoryBase or DbContext in controllers
❌ Querying EF Core directly in controllers
❌ Returning Domain entities from controller actions (use DTOs via Result<T>)
❌ Referencing Infrastructure or Persistence namespaces in controllers
```

---

## Examples

### ✅ Correct — Controller using Infrastructure service

```csharp
// Controllers/TransactionsSystemControllers/RequestsController.cs
namespace ModernPaySystem.Controllers.TransactionsSystemControllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RequestsController(IRequestService requestService, ILogger<RequestsController> logger) : ControllerBase
{
    [HttpGet("{id}")]
    [EndpointPermission("requests.get-by-id", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await requestService.GetByIdAsync(id);
        return result.ToActionResult();
    }
}
```

### ✅ Correct — Infrastructure service using Persistence

```csharp
// Infrastructure/Services/RequestService.cs
public class RequestService(
    IUnitOfWork unitOfWork,
    ILogger<RequestService> logger,
    IWebAttachmentService webAttachmentService,
    IHttpContextServiceManager httpContextServiceManager) : IRequestService
{
    public async Task<Result<PagedList<RequestDto>>> GetPagedAsync(RequestPagedFilterDto? filterDto = null)
    {
        var pagedRequests = await unitOfWork.Requests.GetPagedAsync(
            page, pageSize,
            transform: i => i.Include(x => x.RequestTemplateValues).ThenInclude(x => x!.Template),
            additionalFilters: filters
        );
        // ...
    }
}
```

### ✅ Correct — Dependency injection in Program.cs

```csharp
// Program.cs
global using ModernPaySystem.Infrastructure;
global using ModernPaySystem.Infrastructure.Auth;
global using ModernPaySystem.Infrastructure.Persistence;
global using ModernPaySystem.Infrastructure.Persistence.Seeding;

builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddSeeding(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorizationPolicies();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddCors(options => { ... });
builder.Services.AddControllers();
builder.Services.AddOpenApi("v1", options => { ... });
```

### ❌ Incorrect — Application referencing Persistence

```csharp
// ❌ BAD — Application should have ZERO implementations
using ModernPaySystem.Infrastructure.Persistence.Repos;

public class RequestService : IRequestService  // ❌ Implementation in Application
{
    public async Task<Result<PagedList<RequestDto>>> GetPagedAsync(...) { ... }
}
```

### ❌ Incorrect — EF Core in Infrastructure without RepositoryBase

```csharp
// ❌ BAD — Infrastructure should use IUnitOfWork + RepositoryBase
public class RequestService(IUnitOfWork uow) : IRequestService
{
    public async Task<Result<PagedList<RequestDto>>> GetPagedAsync(...)
    {
        // ❌ Direct EF Core in service — bypasses RepositoryBase
        var results = await _dbcontext.Requests.Where(...).ToListAsync();
    }
}
```

### ❌ Incorrect — Library depending on application layer

```csharp
// ❌ BAD — ExpressionBuilderLib referencing ModernPaySystem.Domain
using ModernPaySystem.Domain.Entities;

public class ExpressionBuilder
{
    public Expression<Func<Request, bool>> BuildRequestFilter(...) // ❌ Domain coupling
}
```

---

## Common Mistakes

| # | Mistake | Detection | Fix |
|---|---------|-----------|-----|
| 1 | Service implementation in Application | grep for `class` in Application/Services | Move to Infrastructure/Services/ |
| 2 | Repository class in Persistence not implementing IRepositoryBase | Code review | Inherit from RepositoryBase<T, TKey> |
| 3 | DTOs defined outside Domain/DTOs | grep for `record.*Dto` | Move to Domain/DTOs/ with correct namespace |
| 4 | Controller injecting DbContext or Repository | grep for `DbContext` or `Repository` in Controllers | Inject I*Service from Infrastructure |
| 5 | Service directly using _dbcontext.Set<T>() | grep for `Set<` in Infrastructure/Services | Use IUnitOfWork + RepositoryBase instead |
| 6 | Domain using [Key] on individual entities | Code review | Keep [Key] only on Entity<T> base class |
| 7 | Application project has service implementations | Code review | Move all implementations to Infrastructure |
| 8 | Using SqlServer packages in Persistence | grep for `SqlServer` in Persistence csproj | Use Npgsql packages only |

---

## AI Coding Rules

### Rule 1 — Layer Boundary Check
```
Every file must answer: "Which layer does this belong to?"
- Entity, DTO, Result, expression helper → Domain
- Interface definition → Application
- Service implementation → Infrastructure
- Repository, DbContext, migration → Persistence
- Controller, middleware → API
- Utility class → Library (ExpressionBuilderLib, FileManager, etc.)
```

### Rule 2 — Import Audit
Before writing any `using` statement in a new file, verify the source project:

```
File in Domain     → only using System.*, ModernPaySystem.Domain.*, library projects
File in Application → only using ModernPaySystem.Application.*, ModernPaySystem.Domain.*
File in Persistence → only using ModernPaySystem.Application.*, ModernPaySystem.Domain.*, Microsoft.EntityFrameworkCore.*, Microsoft.Extensions.*
File in Infrastructure → only using ModernPaySystem.Application.*, ModernPaySystem.Domain.*, ModernPaySystem.Infrastructure.*, ModernPaySystem.Infrastructure.Persistence.UnitOfWork.*, library projects
File in API        → only using ModernPaySystem.Controllers.*, ModernPaySystem.Application.*, ModernPaySystem.Infrastructure.*, ModernPaySystem.Infrastructure.Persistence.*
File in Library    → only using System.* and library-specific namespaces
```

### Rule 3 — Constructor Injection Only
Never use `new` on a service, repository, infrastructure class, or DbContext. Always inject through constructor. Exception: `new()` for DTO record initialization.

### Rule 4 — Interface Ownership
```
Repository interfaces  → owned by Application (ModernPaySystem.Application.Repos)
Service interfaces     → owned by Application (ModernPaySystem.Application.Interfaces)
Repository implementations → owned by Persistence (inherits RepositoryBase<T, TKey>)
Service implementations    → owned by Infrastructure (Infrastructure/Services/)
IUnitOfWork               → owned by Persistence (Infrastructure.Persistence.UnitOfWork)
```

### Rule 5 — Generic RepositoryBase Pattern
```
All repositories MUST inherit from RepositoryBase<TEntity, TKey>
They implement IRepositoryBase<TEntity, TKey> from ModernPaySystem.Application.Repos
Services access repositories via IUnitOfWork.{EntityName} property
```

### Rule 6 — DTOs in Domain
```
All DTOs live in ModernPaySystem.Domain.DTOs
Entities have ToDto() extension methods or methods
Services map entities → DTOs using these methods
Never define DTOs in Application, Infrastructure, or Persistence
```

### Rule 7 — Libraries Are Cross-Cutting
```
ExpressionBuilderLib → used by RepositoryBase for dynamic filtering
FileManager → used by Infrastructure services for file operations
NumberSpelling → used by Infrastructure for currency/amount text conversion
OcrReader → used by Infrastructure for image text extraction
Libraries have no awareness of Domain entities or Application interfaces
```

---

## Refactoring Guidance

### Fixing a layer violation

```csharp
// STEP 1 — Identify the violation
// ❌ Application/Services/RequestService.cs — implementation in wrong layer
public class RequestService : IRequestService { ... }

// STEP 2 — Move to Infrastructure
// ✅ Infrastructure/Services/RequestService.cs
public class RequestService(IUnitOfWork uow, ...) : IRequestService { ... }
// Keep ONLY the interface in Application:
// ✅ Application/Interfaces/IRequestService.cs
public interface IRequestService { ... }
```

### Moving service from Application to Infrastructure

```csharp
// ❌ Current: Service Service in Application
// ModernPaySystem.Application/Services/UserService.cs
using ModernPaySystem.Infrastructure.Persistence;  // ❌ FORBIDDEN
public class UserService(...) { ... }

// STEP 1 — Keep interface in Application
// ModernPaySystem.Application/Interfaces/IUserService.cs
public interface IUserService { ... }

// STEP 2 — Move implementation to Infrastructure
// ModernPaySystem.Infrastructure/Services/UserService.cs
public class UserService(IUnitOfWork uow, IPasswordHasher hasher) : IUserService { ... }

// STEP 3 — Update DI in Program.cs
builder.Services.AddInfrastructureServices(configuration);
// InfrastructureServiceRegistration handles scoped registration
```

---

## UnitOfWork Registration Rule

### Every new entity MUST be registered in IUnitOfWork

When creating a new Domain entity, you MUST also:

1. Add `DbSet<NewEntity>` to `AppDbContext`
2. Add `IRepositoryBase<NewEntity, Guid> NewEntities { get; }` to `IUnitOfWork`
3. Implement the property in `UnitOfWork.cs`

### Services access repositories ONLY through IUnitOfWork

```csharp
// ❌ FORBIDDEN
public class MyService(IRepositoryBase<MyEntity, Guid> myRepo) : IMyService

// ✅ REQUIRED
public class MyService(IUnitOfWork unitOfWork) : IMyService
{
    public async Task<Result<MyDto>> GetAsync(Guid id)
    {
        var result = await unitOfWork.MyEntities.GetByIdAsync(id);
        // ...
    }
}
```

### AI Coding Rule — Repository Access

```
Before writing any service, ask: "Am I injecting IUnitOfWork?"
If yes → proceed
If no → you are doing it wrong
```

---

## Summary Decision Matrix

| Layer Contains →<br>↓ Needs Access | Entity | Repository Interface | Service Interface | Repository Impl | Service Impl | DTO | DbContext | Library |
| |:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| **Domain** | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ | ❌ | ✅ |
| **Application** | ✅ | ✅ | ✅ | ❌ | ❌ | ✅ | ❌ | ❌ |
| **Persistence** | ✅ | ✅ | ❌ | ✅ | ❌ | ❌ | ✅ | ✅ |
| **Infrastructure** | ✅* | ✅ | ✅ | ❌ | ✅ | ✅ | ❌ | ✅ |
| **API** | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |

*\*Infrastructure may use Domain entities returned from repositories.*

---

## AI Coding Rules (Quick Reference)

```
✅ Application = interfaces only (ZERO implementations)
✅ Infrastructure = service implementations
✅ Persistence = RepositoryBase<T>, UnitOfWork, DbContext, migrations
✅ Domain = entities, DTOs, Result<T>, business logic
✅ Libraries = standalone utilities, referenced by any layer

❌ Application must NEVER contain a class (only interfaces)
❌ Infrastructure must NOT contain controllers
❌ Persistence must NOT contain services
❌ Domain must NOT reference HTTP, EF Core config, or outer layers
❌ Controllers must NOT inject DbContext or RepositoryBase
❌ Libraries must NOT reference application layer types

❌ Services must NOT inject IRepositoryBase<T, TKey> directly
✅ Services must inject IUnitOfWork and access repositories via its properties
✅ Every new entity requires a corresponding property in IUnitOfWork
```
