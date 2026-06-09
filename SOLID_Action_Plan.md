# ModernPaySystem — AI Refactoring Action Plan

> **Goal:** Fix all weaknesses identified in SOLID_Report.md incrementally (agile-friendly)  
> **Strategy:** One issue per sprint, each independently deployable

---

## Sprint 1 — 🐛 Critical Bug Fixes

### 1.1 ResponseService self-referencing bug

**File:** `ModernPaySystem.Infrastructure\Services\ResponseService.cs:112`

```csharp
// CURRENT (bug):
filters.AddRange(filters);  // adds itself to itself → infinite loop or wrong data

// FIX:
// filters.AddRange(filters); → DELETE THIS LINE
```

**Change:** Remove the self-referencing line. The `filters` list already contains the `InputValueFilters` from above.

**Test:** `dotnet test --filter "FullyQualifiedName~ResponseService"`

---

### 1.2 DepartmentService sync-over-async

**File:** `ModernPaySystem.Infrastructure\Services\DepartmentService.cs:490-520`

```csharp
// CURRENT (blocking — risk of deadlock):
public bool CanAssignParent(Guid departmentId, Guid parentDepartmentId)
{
    var result = unitOfWork.Departments.GetByIdAsync(currentId).GetAwaiter().GetResult();
    ...
}

// FIX:
public async Task<bool> CanAssignParent(Guid departmentId, Guid parentDepartmentId)
{
    var result = await unitOfWork.Departments.GetByIdAsync(currentId);
    ...
}
```

**Impact propagation:** Update the interface `IDepartmentService` and the single caller (controller) to be async.

---

### 1.3 AttachmentService raw Exception throws

**Files:** `AttachmentService.cs:632,644`

```csharp
// CURRENT:
throw new Exception("Error checking attachmentDto associations: ...");

// FIX:
return Result<Success>.Failure(new Error("ATTACHMENT_CHECK", "Error checking associations", ErrorKind.Failure));
```

---

## Sprint 2 — 🚀 Performance (N+1 + In-Memory)

### 2.1 Fix N+1 in AttachmentService

**Files:** `AttachmentService.cs` — 3 methods (`GetAttachmentsForRequestAsync`, `GetAttachmentsForResponseAsync`, `GetAttachmentsForTransactionAsync`)

**Pattern currently:**
```csharp
var attachmentIds = requestAttachments.Value!.ConvertAll(ra => ra.AttachmentId);
var attachmentDtos = new List<AttachmentDto>();
foreach (var attachmentId in attachmentIds)       // ← N queries
{
    var attachment = await unitOfWork.Attachments.GetByIdAsync(attachmentId);
    ...
}
```

**Fix — add batch method to RepositoryBase:**
```csharp
// In IRepositoryBase<T, TKey>:
Task<Result<List<TEntity>>> GetAllByIdsAsync(IEnumerable<TKey> ids, CancellationToken ct = default);
```

**Then use:**
```csharp
var attachments = await unitOfWork.Attachments.GetAllByIdsAsync(attachmentIds);
return attachments.Value!.Select(a => a.ToDto()).ToList();
```

### 2.2 Fix in-memory paging

**File:** `AttachmentService.cs:663-679`

```csharp
// CURRENT:
var allAttachments = await GetAllAsync();  // loads EVERYTHING
var pagedAttachments = allAttachments.Skip((page - 1) * pageSize).Take(pageSize).ToList();

// FIX — use repository's built-in GetPagedAsync:
var paged = await unitOfWork.Attachments.GetPagedAsync(page, pageSize);
```

**Also fix:** `GetByFileTypeAsync`, `GetByFileNameAsync`, `DepartmentService.SearchAsync` — push filters down to EF Core.

---

## Sprint 3 — 🧹 Remove Query Duplication

### 3.1 Extract shared Include specifications

**Problem:** The same `.Include()` chains repeated 5-10x across services.

**Create:** `ModernPaySystem.Infrastructure\Specifications\` folder

**Pattern:**
```csharp
// RequestIncludes.cs
public static class RequestIncludes
{
    public static IQueryable<Request> WithFullDetails(this IQueryable<Request> query) =>
        query.Include(x => x.RequestTemplateValues).ThenInclude(x => x!.Template)
             .Include(x => x.RequestTemplateValues).ThenInclude(x => x!.InputValues)
             .Include(x => x.OutgoingRelations).ThenInclude(r => r.TargetRequest)
             .Include(x => x.Requester).ThenInclude(r => r!.Department)
             .Include(x => x.Approver).ThenInclude(a => a!.Department)
             .Include(x => x.RequestAttachments);

    public static IQueryable<Request> WithTemplateValues(this IQueryable<Request> query) =>
        query.Include(x => x.RequestTemplateValues).ThenInclude(x => x!.Template)
             .Include(x => x.RequestTemplateValues).ThenInclude(x => x!.InputValues);
}
```

**Then use everywhere:**
```csharp
transform: i => i.WithFullDetails()
```

**Similarly for:** `ArchiveRecordIncludes`, `ResponseIncludes`, `RequestTransactionIncludes`, `UserIncludes`

---

## Sprint 4 — 🔪 Split Oversized Services

### 4.1 Split ArchiveRecordService (~1600 lines)

**Current:** CRUD + file storage + ZIP + caching + template values + cleanup

**New structure:**

| New Service | Responsibility | Moved Methods |
|------------|---------------|---------------|
| `ArchiveRecordCrudService` | CRUD + validation | Create, Update, Delete, GetAll, GetPaged, GetById, GetByFolder, GetByForm |
| `ArchiveFileService` | File operations | StoreFiles, CleanupStoredFiles, DeleteStoredFile, GetPhysicalFileStream, AddFiles, RemoveFile, CheckFileConsistency, CleanupOrphanFiles, GetFilesMetadata, GetPaginatedFiles, BuildExpectedStoragePath, BuildPagedFilesCacheKey |
| `ArchiveTemplateService` | Template value management | BuildTemplateValues, AddTemplateValuesIfPresent, ResolveForm, ResolveFilesToRemove |

**Dependencies:**
```
ArchiveRecordCrudService → IArchiveFileService, IArchiveTemplateService, IUnitOfWork
ArchiveFileService → IFilesManagerService, IFileManager, IMemoryCache, IUnitOfWork
ArchiveTemplateService → IUnitOfWork
```

### 4.2 Split RequestService (~931 lines)

**Extract:** `RequestRelationService` — all relation CRUD methods (GetRelationsByRequestId, GetRelationById, CreateRelation, UpdateRelation, DeleteRelation)

**New interface:**
```csharp
public interface IRequestRelationService
{
    Task<Result<List<RequestRelationDto>>> GetByRequestIdAsync(Guid requestId);
    Task<Result<RequestRelationDto>> GetByIdAsync(Guid id);
    Task<Result<RequestRelationDto>> CreateAsync(CreateRequestRelationDto dto);
    Task<Result<RequestRelationDto>> UpdateAsync(Guid id, UpdateRequestRelationDto dto);
    Task<Result<bool>> DeleteAsync(Guid id);
}
```

---

## Sprint 5 — 🏗️ Segregate God Interfaces

### 5.1 Split IUnitOfWork

**Current:** 27 repository properties + 4 transaction methods

**Strategy:** Domain-specific sub-interfaces

```csharp
public interface IUnitOfWorkTransaction
{
    bool HasActiveTransaction { get; }
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    Task<int> SaveChangesAsync();
}

public interface IUnitOfWorkArchiving : IUnitOfWorkTransaction
{
    IRepositoryBase<Folder, Guid> Folders { get; }
    IRepositoryBase<ArchiveRecord, Guid> ArchiveRecords { get; }
    IRepositoryBase<PhysicalFile, Guid> PhysicalFiles { get; }
    IRepositoryBase<ArchiveFormTemplate, Guid> DynamicForms { get; }
    IRepositoryBase<ArchiveRecordTemplateValues, Guid> ArchiveRecordTemplateValues { get; }
    IRepositoryBase<DepartmentArchiveLeader, Guid> DepartmentArchiveLeaders { get; }
    // ...
}

public interface IUnitOfWorkTransactionSystem : IUnitOfWorkTransaction
{
    IRepositoryBase<Request, Guid> Requests { get; }
    IRepositoryBase<Response, Guid> Responses { get; }
    IRepositoryBase<Template, Guid> Templates { get; }
    IRepositoryBase<RequestRelation, Guid> RequestRelations { get; }
    IRepositoryBase<RequestTransaction, Guid> RequestTransactions { get; }
    // ...
}
```

**DI Registration:**
```csharp
services.AddScoped<IUnitOfWorkArchiving, UnitOfWork>();
services.AddScoped<IUnitOfWorkTransactionSystem, UnitOfWork>();
services.AddScoped<IUnitOfWork, UnitOfWork>();  // keep full for migration period
```

**Benefit:** Services only see the repositories they need. Easier to unit test and reason about.

### 5.2 Segregate IRepositoryBase

Split into read-only and write-only:
```csharp
public interface IReadRepository<TEntity, TKey>
{
    Task<Result<TEntity?>> GetByIdAsync(TKey id, bool bypassAuth = false);
    Task<Result<List<TEntity>>> GetAllAsync(...);
    Task<Result<PagedList<TEntity>>> GetPagedAsync(...);
    Task<Result<TEntity>> GetAsync(...);
    Task<bool> AnyAsync(...);
}

public interface IWriteRepository<TEntity, TKey>
{
    Task<Result<Success>> AddAsync(TEntity entity, bool bypassAuth = false);
    Task<Result<Updated>> UpdateAsync(TEntity entity, bool bypassAuth = false);
    Task<Result<Deleted>> RemoveAsync(Expression<Func<TEntity, bool>> filter, bool bypassAuth = false);
}
```

---

## Sprint 6 — 🧹 Standardize Code Style

### 6.1 Convert all services to primary constructors

**Files to update:**
- `RoleService.cs` — currently uses traditional constructor + `private readonly` fields

**Pattern:**
```csharp
// FROM:
public class RoleService : IRoleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RoleService> _logger;
    public RoleService(IUnitOfWork unitOfWork, ILogger<RoleService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
}

// TO:
public class RoleService(IUnitOfWork unitOfWork, ILogger<RoleService> logger) : IRoleService
```

### 6.2 Add .editorconfig + run dotnet format

```bash
dotnet new editorconfig
dotnet format ModernPaySystem.slnx
```

---

## Sprint 7 — 🛡️ Add FluentValidation

### 7.1 Install and configure

```bash
dotnet add ModernPaySystem.Application package FluentValidation
dotnet add ModernPaySystem.Application package FluentValidation.DependencyInjectionExtensions
```

### 7.2 Add validators

```csharp
// Application/Validators/CreateRequestDtoValidator.cs
public class CreateRequestDtoValidator : AbstractValidator<CreateRequestDto>
{
    public CreateRequestDtoValidator()
    {
        RuleFor(x => x.TemplateId).NotEmpty();
        RuleFor(x => x.DepartmentId).NotEmpty();
        RuleFor(x => x.Content).NotEmpty();
    }
}
```

### 7.3 Register in DI

```csharp
services.AddValidatorsFromAssembly(typeof(CreateRequestDtoValidator).Assembly);
```

---

## Appendix: Effort Estimates

| Sprint | Task | Files Touched | Estimated Effort |
|--------|------|---------------|-----------------|
| 1 | Bug fixes | 3 | 2-3 hours |
| 2 | Performance | 4 | 4-6 hours |
| 3 | Query specs | 8-12 | 6-8 hours |
| 4 | Split services | 6-8 | 8-12 hours |
| 5 | Interface segregation | 10-15 | 6-10 hours |
| 6 | Code style | 15-20 | 2-4 hours |
| 7 | FluentValidation | 8-12 | 4-6 hours |

Each sprint is designed to be independently deployable without breaking changes.

---

## Prerequisite: Add IRepositoryBase.GetAllByIdsAsync

Before Sprint 2, add this method — it's foundational for fixing N+1:

```csharp
// IRepositoryBaseT.cs
public async Task<Result<List<TEntity>>> GetAllByIdsAsync(
    IEnumerable<TKey> ids,
    Func<IQueryable<TEntity>, IQueryable<TEntity>>? transform = null,
    CancellationToken ct = default)
{
    try
    {
        IQueryable<TEntity> query = dbcontext.Set<TEntity>();
        if (transform != null) query = transform(query);
        return await query.Where(e => ids.Contains(e.Id)).ToListAsync(ct);
    }
    catch (Exception e)
    {
        logger.LogError(e, "Error batch-fetching {EntityType} by ids", typeof(TEntity).Name);
        return new Error("00", $"Error fetching entities", ErrorKind.Failure);
    }
}
```
