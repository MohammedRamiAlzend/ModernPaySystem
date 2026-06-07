# Application Skill — ModernPaySystem.Application

## Purpose

The Application layer is a **contract-only layer**. It contains ZERO implementations — only interfaces, DTO references, and specifications. It defines **what the system does** without knowing **how it does it**.

**Key difference from standard Clean Architecture:** Services are NOT implemented here. They are implemented in **Infrastructure**. The Application layer defines the interface contracts (`IRequestService`, `IUserService`, etc.) and the repository contract (`IRepositoryBase<TEntity, TKey>`).

---

## Responsibilities

| Responsibility | Details |
|---------------|---------|
| **Service Interfaces** | `IUserService`, `IRequestService`, `IArchiveRecordService`, etc. |
| **Repository Interface** | `IRepositoryBase<TEntity, TKey>` (generic base) — accessed ONLY via `IUnitOfWork` in services |
| **Unit of Work Interface** | `IUnitOfWork` |
| **Auth Interfaces** | `IAuthenticationService`, `ITokenService`, `IPasswordHasher` |
| **DTO References** | DTOs are defined in Domain, but Application references them in interface signatures |
| **Specifications** | Query specifications if used (currently minimal) |

---

## Folder Structure

```
ModernPaySystem.Application/
├── Interfaces/
│   ├── IUserService.cs
│   ├── IRoleService.cs
│   ├── IRequestService.cs
│   ├── IResponseService.cs
│   ├── ITemplateService.cs
│   ├── IRequestTransactionService.cs
│   ├── IAttachmentService.cs
│   ├── IWebAttachmentService.cs
│   ├── IFolderService.cs
│   ├── IDynamicFormService.cs
│   ├── IArchiveFormTemplateService.cs
│   ├── IArchiveRecordService.cs
│   ├── IArchiveLeaderService.cs
│   ├── IArchiveDeletionWorkflowService.cs
│   ├── IArchiveEditWorkflowService.cs
│   ├── IArchiveAuthorizationService.cs
│   ├── IReportService.cs
│   ├── IRequestTransactionService.cs
│   ├── ILookUpFieldService.cs
│   ├── ILookUpFiledValuesService.cs
│   ├── IDepartmentService.cs
│   ├── IPermissionSeederService.cs
│   ├── IPasswordHasher.cs
│   ├── IOcrService.cs
│   ├── INumberSpellingWrapperService.cs
│   ├── IHttpContextServiceManager.cs
│   ├── ITokenService.cs
│   ├── IAuthenticationService.cs
│   └── TransactionSystemInterfaces/
│       └── IReportService.cs
├── Repos/
│   └── IRepositoryBase.cs     # Generic repository interface
├── Services/
│   ├── ITokenService.cs
│   ├── IHttpContextServiceManager.cs
│   └── IAuthenticationService.cs
├── DTOs/
│   └── Auth/
│       └── AuthDtos.cs        # LoginRequest, etc.
├── ModernPaySystem.Application.csproj
└── (NO DependencyInjection.cs — registration is in Infrastructure)
```

---

## Design Rules

### Rule 1 — Application Contains ZERO Implementations

```csharp
// Application/Interfaces/IRequestService.cs — ONLY the interface
namespace ModernPaySystem.Application.Interfaces;

public interface IRequestService
{
    Task<Result<PagedList<RequestDto>>> GetPagedAsync(RequestPagedFilterDto? filterDto);
    Task<Result<RequestDto>> GetByIdAsync(Guid id);
    Task<Result<RequestDto>> CreateAsync(CreateRequestDto request, List<IFormFile> files);
    Task<Result<bool>> DeleteAsync(Guid id);
    // ...
}
```

**Never add a class implementation here.** The implementation goes in `Infrastructure/Services/RequestService.cs`.

### Rule 2 — Repository Contract is Generic

```csharp
// Application/Repos/IRepositoryBase.cs
namespace ModernPaySystem.Application.Repos;

public interface IRepositoryBase<TEntity, TKey> where TEntity : Entity<TKey>
{
    Task<Result<Success>> AddAsync(TEntity entity, bool bypassAuth = false);
    Task<Result<List<TEntity>>> GetAllAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? transform = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool bypassAuth = false,
        List<Expression<Func<TEntity, bool>>>? additionalFilters = null);
    Task<Result<PagedList<TEntity>>> GetPagedAsync(
        int page, int pageSize,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? transform = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool bypassAuth = false,
        List<Expression<Func<TEntity, bool>>>? additionalFilters = null,
        LogicalOperator logicalOperator = LogicalOperator.And);
    Task<Result<PagedList<TResult>>> GetPagedProjectedAsync<TResult>(
        int page, int pageSize,
        Expression<Func<TEntity, bool>>? filter = null,
        Expression<Func<TEntity, TResult>>? selector = null,
        // ... );
    Task<Result<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool bypassAuth = false,
        List<Expression<Func<TEntity, bool>>>? additionalFilters = null);
    Task<Result<Deleted>> RemoveAsync(Expression<Func<TEntity, bool>> filter, bool bypassAuth = false);
    Task<Result<Updated>> UpdateAsync(TEntity entity, bool bypassAuth = false);
    Task<Result<TEntity?>> GetByIdAsync(TKey id, bool bypassAuth = false);
    Task<Result<List<TEntity>>> FindAsync(
        Expression<Func<TEntity, bool>> filter,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? transform = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool bypassAuth = false,
        List<Expression<Func<TEntity, bool>>>? additionalFilters = null);
    Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> filter,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? transform = null,
        List<Expression<Func<TEntity, bool>>>? additionalFilters = null);
}
```

**Key:** `TEntity : Entity<TKey>` — enforces that all entities inherit from Domain's `Entity<TKey>`.

### Rule 3 — UnitOfWork Interface

```csharp
// Persistence/UnitOfWork/IUnitOfWork.cs (referenced by Application)
public interface IUnitOfWork : IDisposable
{
    IRepositoryBase<User, Guid> Users { get; }
    IRepositoryBase<Request, Guid> Requests { get; }
    IRepositoryBase<ArchiveRecord, Guid> ArchiveRecords { get; }
    // ... one property per DbSet

    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
```

Services access repositories via `unitOfWork.Requests.GetAsync(...)`.

### Rule 4 — DTOs Defined in Domain, Referenced Here

Interface methods use DTOs defined in `ModernPaySystem.Domain.DTOs`:

```csharp
public interface IRequestService
{
    Task<Result<PagedList<RequestDto>>> GetPagedAsync(RequestPagedFilterDto? filterDto);
    Task<Result<RequestDto>> GetByIdAsync(Guid id);
    Task<Result<RequestDto>> CreateAsync(CreateRequestDto request, List<IFormFile> files);
}
```

### Rule 5 — Auth Interfaces

```csharp
public interface IAuthenticationService
{
    Task<Result<string>> AuthenticateAsync(string username, string password);
    Task<Result<string>> RegisterAsync(...);
    Task<Result<string>> RefreshTokenAsync(...);
}

public interface ITokenService
{
    Task<string> GenerateAccessTokenAsync(User user);
    Task<string> GenerateRefreshTokenAsync();
    // ...
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
```

### Rule 6 — No Dependency Injection in Application

The Application layer has NO `DependencyInjection.cs` extension method. Registration happens in:
- `Infrastructure/InfrastructureServiceRegistration.cs` for services
- `Infrastructure/Persistence/DependencyInjection.cs` for repositories and DbContext

---

## UnitOfWork Rule

### IUnitOfWork lives in Persistence — NOT in Application

The `IUnitOfWork` interface is defined in:
```
ModernPaySystem.Infrastructure.Persistence.UnitOfWork.IUnitOfWork
```

**Application does NOT define IUnitOfWork.** It only references it for type constraints in generic interfaces.

### Services in Infrastructure access repositories via IUnitOfWork

```csharp
// Infrastructure/Services/RequestService.cs
public class RequestService(IUnitOfWork unitOfWork) : IRequestService
{
    public async Task<Result<RequestDto>> GetByIdAsync(Guid id)
    {
        var result = await unitOfWork.Requests.GetByIdAsync(id);
        if (result.IsError) return result.Errors;
        return Result<RequestDto>.Success(result.Value!.ToDto());
    }
}
```

### ❌ FORBIDDEN — Direct IRepositoryBase injection

```csharp
// ❌ This is FORBIDDEN in Infrastructure services
public class RequestService(
    IUnitOfWork unitOfWork,
    IRepositoryBase<Request, Guid> requestRepo) : IRequestService  // ← FORBIDDEN
```

### AI Coding Rule

```
When creating an Infrastructure service:
1. Inject ONLY IUnitOfWork (plus any other infrastructure interfaces needed)
2. Access all repositories through unitOfWork.{EntityName} properties
3. NEVER inject IRepositoryBase<T, TKey> directly
4. NEVER reference ModernPaySystem.Infrastructure.Persistence.Repos directly in services
```

---

## Naming Conventions

| Element | Style | Example |
|---------|-------|---------|
| Service Interface | `I{Name}Service` | `IRequestService`, `IArchiveRecordService` |
| Repository Interface | `IRepositoryBase` (generic) | `IRepositoryBase<Request, Guid>` |
| UnitOfWork property | `{PluralEntityName}` | `unitOfWork.Requests`, `unitOfWork.Users` |
| DTO (response) | `{Entity}Dto` | `RequestDto` |
| DTO (create) | `Create{Entity}Dto` | `CreateRequestDto` |
| DTO (update) | `Update{Entity}Dto` | `UpdateRequestDto` |
| DTO (filter) | `{Entity}PagedFilterDto` | `RequestPagedFilterDto` |
| Files | Match interface name | `IRequestService.cs` |

---

## AI Generation Rules

### When creating a new service interface

```markdown
1. Place in `Application/Interfaces/{Category}I{Entity}Service.cs` or flat in `Interfaces/`
2. Name: `I{Entity}Service` (e.g., `IArchiveRecordService`, `IRequestService`)
3. Methods follow CRUD + domain-specific operations pattern
4. Use Result<T> return types
5. Accept CancellationToken ct where applicable
6. Reference Domain DTOs in method signatures (not Application DTOs)
7. Accept List<IFormFile> for file upload endpoints
```

### When adding to IUnitOfWork

```markdown
1. UnitOfWork is defined in Persistence (Infrastructure.Persistence.UnitOfWork.IUnitOfWork)
2. Add new repository property: `IRepositoryBase<NewEntity, Guid> NewEntities { get; }`
3. Register in Persistence's DependencyInjection.cs
4. Implementation injects: `IUnitOfWork` + any needed infrastructure interfaces. All data access goes through `unitOfWork.{EntityName}` properties.
```

### Service interface checklist

```markdown
- [ ] ONLY interface — NO implementation class
- [ ] Methods use Result<T> return type
- [ ] DTOs from Domain.DTOs used in signatures
- [ ] CancellationToken accepted on async methods
- [ ] File upload parameters use List<IFormFile>
- [ ] No references to Infrastructure or Persistence
- [ ] Uses file-scoped namespace: `namespace ModernPaySystem.Application.Interfaces;`
- [ ] Service injects IUnitOfWork (not IRepositoryBase directly)
- [ ] All repository access via unitOfWork.{EntityName}.{Method}()
- [ ] No direct using of ModernPaySystem.Infrastructure.Persistence.Repos
```
