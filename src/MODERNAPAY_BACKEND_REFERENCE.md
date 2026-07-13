# ModernPaySystem Backend — Complete Reference

## Architecture Overview

Modular monolith with **Shared Kernel** + **independent modules** (each with its own DbContext, UnitOfWork, and controllers). Built on .NET 10, PostgreSQL + EF Core 10, JWT auth, Serilog logging.

```
src/
├── ModernPaySystem.Boot/                          # Entry point    
├── ModernPaySystem.SharedKernel.Domain/           # Base entities, Result<T>, Error, PagedList, shared entity stubs
├── ModernPaySystem.SharedKernel.Application/      # IRepositoryBase<T>, ICurrentUserService, IHttpContextServiceManager
├── ModernPaySystem.SharedKernel.Infrastructure/   # RepositoryBase impl, CurrentUserService, ResultExtensions, auth
├── ExpressionBuilderLib/                          # Dynamic expression trees (PropertyFilter → Expression<Func<T,bool>>)
├── FileManager/                                   # File system abstraction (IFileManager, IFilesManagerService)
├── NumberSpelling/                                # Arabic number → words (NumberToArabicText)
├── OcrReader/                                     # Tesseract OCR for images + PDFs
├── SemanticSearchLib/                             # Ollama + Qdrant vector search
└── Modules/
    ├── IdentitySystem/                            # Auth, Users, Roles, Departments, Permissions
    ├── TransactionSystem/                         # Requests, Responses, Transactions, Templates, Attachments, Reports
    └── ArchiveSystem/                             # Archive records, folders, documents, OCR, search
```

## Module Contract

Each module follows this structure:

```
Modules/<ModuleName>/
├── ModernPaySystem.Module.<ModuleName>.Domain/        # Entities, DTOs, Enums, Errors
├── ModernPaySystem.Module.<ModuleName>.Application/   # Interfaces, UnitOfWork contract
├── ModernPaySystem.Module.<ModuleName>.Infrastructure/# DbContext, UnitOfWork impl, Services, DI registration
└── ModernPaySystem.Module.<ModuleName>.Api/           # Controllers, Extensions
```

Each module must provide:
1. A `*ModuleRegistration.cs` extension method on `IServiceCollection`
2. Its own `DbContext` (only its own entity `DbSet`s)
3. Its own controllers with an isolated route prefix
4. Its own `UnitOfWork` that wraps `RepositoryBase<T, Guid>` from SharedKernel

## Module DbContext Strategies

| Module | Strategy | DbContext Location |
|--------|----------|--------------------|
| Identity | Shared DbContext (reuses monolith's AppDbContext) | `Identity.Infrastructure/Persistence/IdentityDbContext.cs` |
| Transaction | Own DbContext | `Transaction.Infrastructure/Persistence/TransactionDbContext.cs` |
| Archive | Own DbContext | `Archive.Infrastructure/Persistence/ArchiveDbContext.cs` |

---

## Shared Kernel (src/ModernPaySystem.SharedKernel.*)

### Domain (`SharedKernel.Domain`)

#### Entity Base (`Entities/Abstraction/Entity.cs`)
```csharp
namespace ModernPaySystem.SharedKernel.Domain.Entities.Abstraction;
public class Entity<TKey>
{
    [Key]
    public virtual TKey Id { get; set; }
}
```

#### IAuditableEntity (`Entities/Abstraction/IAuditableEntity.cs`)
```csharp
namespace ModernPaySystem.SharedKernel.Domain.Entities.Abstraction;
public interface IAuditableEntity
{
    string? CreatedByUserId { get; set; }
    DateTime? CreatedAt { get; set; }
    string? UpdatedByUserId { get; set; }
    DateTime? UpdatedAt { get; set; }
}
```

#### Result<T> (`Commons/ResultOfT.cs`)
```csharp
namespace ModernPaySystem.SharedKernel.Domain.Commons;

public interface IResult
{
    List<Error>? Errors { get; }
    bool IsSuccess { get; }
}

public interface IResult<out TValue> : IResult
{
    TValue? Value { get; }
}

public sealed class Result<TValue> : IResult<TValue>
{
    public bool IsSuccess { get; }
    public bool IsError => !IsSuccess;
    public List<Error> Errors => IsError ? _errors! : [];
    public TValue? Value => IsSuccess ? _value! : default;
    public Error TopError => (_errors?.Count > 0) ? _errors[0] : default;

    // Implicit conversions:
    public static implicit operator Result<TValue>(TValue value) => new(value);
    public static implicit operator Result<TValue>(Error error) => new(error);
    public static implicit operator Result<TValue>(List<Error> errors) => new(errors);
}

// Marker types for action results:
public readonly record struct Success(object? Data = null);
public readonly record struct Created(object? Data = null);
public readonly record struct Deleted;
public readonly record struct Updated(object? Data = null);
public readonly record struct Assigned;

public static class Result
{
    public static Success Success => default;
    public static Created Created => default;
    public static Deleted Deleted => default;
    public static Updated Updated => default;
    public static Assigned Assigned => default;
}
```

#### Error (`Commons/Error.cs`)
```csharp
namespace ModernPaySystem.SharedKernel.Domain.Commons;

public readonly record struct Error
{
    public string Code { get; }
    public string Description { get; }
    public ErrorKind Type { get; }
    public string? ArabicDescription { get; }
    public HttpStatusCode HttpStatus { get; }

    public static Error Failure(string code, string description, string? arabicDescription = null)
        => new(code, description, ErrorKind.Failure, arabicDescription, HttpStatusCode.BadRequest);
    public static Error Unexpected(string code, string description, string? arabicDescription = null)
        => new(code, description, ErrorKind.Unexpected, arabicDescription, HttpStatusCode.InternalServerError);
    public static Error Validation(string code, string description, string? arabicDescription = null)
        => new(code, description, ErrorKind.Validation, arabicDescription, HttpStatusCode.BadRequest);
    public static Error Conflict(string code, string description, string? arabicDescription = null)
        => new(code, description, ErrorKind.Conflict, arabicDescription, HttpStatusCode.Conflict);
    public static Error NotFound(string code, string description, string? arabicDescription = null)
        => new(code, description, ErrorKind.NotFound, arabicDescription, HttpStatusCode.NotFound);
    public static Error Unauthorized(string code, string description, string? arabicDescription = null)
        => new(code, description, ErrorKind.Unauthorized, arabicDescription, HttpStatusCode.Unauthorized);
    public static Error Forbidden(string code, string description, string? arabicDescription = null)
        => new(code, description, ErrorKind.Forbidden, arabicDescription, HttpStatusCode.Forbidden);
}

public enum ErrorKind { Failure, Unexpected, Validation, Conflict, NotFound, Unauthorized, Forbidden }
```

#### PagedList (`Commons/PagedList.cs`)
```csharp
public class PagedList<TData>
{
    public IEnumerable<TData> Items { get; set; }
    public int TotalItems { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);

    public PagedList(IEnumerable<TData> items, int totalItems, int page, int pageSize) { ... }
}
```

#### EndpointPermissionAttribute (`Attrs/EndpointPermissionAttribute.cs`)
```csharp
[AttributeUsage(AttributeTargets.Method)]
public class EndpointPermissionAttribute(string key, SubSystem subSystem, PermissionType type,
    string? name = null, string? description = null) : Attribute
{
    public string? Name => name;
    public string Key => key;
    public SubSystem SubSystem => subSystem;
    public PermissionType Type => type;
}

public enum PermissionType { Read, Insert, Delete, Update }
public enum SubSystem { None, TransactionSystem, Diwan, Shared, Archiving }
```

#### CurrentUser (`Identity/CurrentUser.cs`)
```csharp
public record CurrentUser(
    string UserId,
    string UserName,
    List<string> Roles,
    List<string> Permissions,
    Guid? DepartmentId,
    bool IsDepartmentHead,
    SubSystem SubSystem
);
```

### Application (`SharedKernel.Application`)

#### ICurrentUserService
```csharp
public interface ICurrentUserService
{
    CurrentUser? GetCurrentUser();
    string? GetCurrentUserId();
    bool IsAuthenticated();
    bool HasPermission(string permissionKey);
}
```

#### IHttpContextServiceManager
```csharp
public interface IHttpContextServiceManager
{
    string? GetCurrentUserId();
    HttpContext? GetContext();
    string? GetClientIpAddress();
    string? GetUserAgent();
}
```

#### IRepositoryBase<TEntity, TKey>
```csharp
public interface IRepositoryBase<TEntity, TKey> where TEntity : Entity<TKey>
{
    Task<Result<Success>> AddAsync(TEntity entity, bool bypassAuth = false);
    Task<Result<Success>> AddRangeAsync(IEnumerable<TEntity> entities, bool bypassAuth = false);
    Task<Result<List<TEntity>>> GetAllByIdsAsync(List<TKey> ids, PropertyFilter[]? filters = null, Func<IQueryable<TEntity>, IQueryable<TEntity>>? transform = null, bool bypassAuth = false);
    Task<Result<List<TEntity>>> GetAllAsync(Expression<Func<TEntity, bool>>? filter = null, Func<IQueryable<TEntity>, IQueryable<TEntity>>? transform = null, bool bypassAuth = false);
    Task<Result<List<TEntity>>> FindAsync(Expression<Func<TEntity, bool>> filter, Func<IQueryable<TEntity>, IQueryable<TEntity>>? transform = null, bool bypassAuth = false);
    Task<Result<PagedList<TEntity>>> GetPagedAsync(int page, int pageSize, Expression<Func<TEntity, bool>>? filter = null, Func<IQueryable<TEntity>, IQueryable<TEntity>>? transform = null, PropertyFilter[]? propertyFilters = null, LogicalOperator logicalOperator = LogicalOperator.And, bool bypassAuth = false);
    Task<Result<PagedList<TResult>>> GetPagedProjectedAsync<TResult>(int page, int pageSize, Expression<Func<TEntity, bool>>? filter = null, Func<IQueryable<TEntity>, IQueryable<TEntity>>? transform = null, PropertyFilter[]? propertyFilters = null, Expression<Func<TEntity, TResult>>? selector = null, LogicalOperator logicalOperator = LogicalOperator.And, bool bypassAuth = false);
    Task<Result<TEntity?>> GetByIdAsync(TKey id, bool bypassAuth = false);
    Task<Result<TEntity>> GetAsync(Expression<Func<TEntity, bool>>? filter = null, Func<IQueryable<TEntity>, IQueryable<TEntity>>? transform = null, PropertyFilter[]? propertyFilters = null, bool bypassAuth = false);
    Task<Result<Deleted>> RemoveAsync(Expression<Func<TEntity, bool>> filter, bool bypassAuth = false);
    Task<Result<Deleted>> RemoveRangeAsync(Expression<Func<TEntity, bool>> filter, bool bypassAuth = false);
    Task<Result<Updated>> UpdateAsync(TEntity entity, bool bypassAuth = false);
    Task<Result<Updated>> UpdateRangeAsync(IEnumerable<TEntity> entities, bool bypassAuth = false);
    Task<Result<TEntity>> ReplaceAsync(Expression<Func<TEntity, bool>> filter, TEntity entity, bool bypassAuth = false);
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter, PropertyFilter[]? propertyFilters = null, bool bypassAuth = false);
    Task<int> CountAsync(Expression<Func<TEntity, bool>> filter, PropertyFilter[]? propertyFilters = null, bool bypassAuth = false);
    Task<Result<Success>> EnsureCreatedAsync();
}
```

#### ISharedReadRepository<T>
```csharp
public interface ISharedReadRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<List<T>> GetAllAsync();
    Task<bool> AnyAsync(Expression<Func<T, bool>> filter);
}
```

### Infrastructure (`SharedKernel.Infrastructure`)

#### RepositoryBase<TEntity, TKey> (EF Core implementation)
- Full generic implementation using `DbContext` (works with any module's DbContext)
- Uses `ExpressionCombiner` from ExpressionBuilderLib for combining filters (AND/OR)
- Default ordering by `CreatedAt` descending for `IAuditableEntity`, else by `Id`
- Error handling with FK violation extraction via regex
- All methods return `Result<T>` wrapping

#### CurrentUserService
- Extracts claims from JWT: NameIdentifier, Name, permission[], System, IsDepartmentHead, DepartmentId

#### HttpContextServiceManager
- Wraps `IHttpContextAccessor`

#### ResultExtensions
```csharp
public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result) where T : notnull
    {
        if (result.IsError)
        {
            return result.TopError.Type switch
            {
                ErrorKind.NotFound => new NotFoundObjectResult(new { error = result.TopError }),
                ErrorKind.Unauthorized => new UnauthorizedObjectResult(new { error = result.TopError }),
                ErrorKind.Forbidden => new ObjectResult(new { error = result.TopError }) { StatusCode = 403 },
                ErrorKind.Conflict => new ConflictObjectResult(new { error = result.TopError }),
                ErrorKind.Validation => new BadRequestObjectResult(new { error = result.TopError }),
                _ => new BadRequestObjectResult(new { error = result.TopError })
            };
        }
        return result.Value switch
        {
            Created created => new CreatedResult("/", new { data = created.Data }),
            Deleted => new NoContentResult(),
            Updated updated => new OkObjectResult(new { data = updated.Data }),
            Success success => new OkObjectResult(new { data = success.Data }),
            _ => new OkObjectResult(new { data = result.Value })
        };
    }
}
```

#### Auth (PermissionRequirement + PermissionAuthorizationHandler)
- `PermissionRequirement(string PermissionKey)` — IAuthorizationRequirement
- `PermissionAuthorizationHandler` — queries user→roles→permissions in DB for authorization

#### SharedKernelServiceRegistration.cs
```csharp
public static IServiceCollection AddSharedKernel(this IServiceCollection services)
{
    services.AddScoped(typeof(IRepositoryBase<,>), typeof(RepositoryBase<,>));
    services.AddScoped<ICurrentUserService, CurrentUserService>();
    services.AddScoped<IHttpContextServiceManager, HttpContextServiceManager>();
    services.AddHttpContextAccessor();
    return services;
}
```

---

## Modules

### Identity Module

**Route prefix:** `api/` (AuthController: `api/auth/login`)

#### Controllers (`Identity.Api/Controllers`)
| Controller | Routes |
|------------|--------|
| AuthController | `POST api/auth/login` → JWT with roles + permissions |
| UsersController | CRUD + get-by-username, by-sub-system, current-department-users |
| RolesController | CRUD + get-by-name, permissions management |
| DepartmentsController | Tree, subtree, get-by-id, children, search, parent, level, path-to-root, users-in-department, assign/unassign, head |

#### Services (`Identity.Infrastructure/Services`)
| Service | Key Details |
|---------|-------------|
| AuthenticationService | Validates credentials, calls `ITokenService.GenerateAccessToken()` with roles + permissions |
| JwtTokenService | Creates JWT with claims: NameIdentifier, Name, System, IsDepartmentHead, permission[], DepartmentId |
| PasswordHasher | SHA256-based (salt + hash) |
| UserService | Full CRUD with logging, paging, includes (Roles, Department, SubSystemUsers) |
| RoleService | CRUD + permissions assignment |
| DepartmentService | Tree management, materialized path, level, user assignment |
| PermissionSeederService | Discovers `EndpointPermission` attributes, creates permissions, assigns to SuperAdmin |

#### IdentityDbContext
- 5 DbSets: Users, Roles, Permissions, Departments, SubSystemUsers
- Many-to-many: UserRoles, RolePermissions
- Self-referencing Department

#### Seeding
| Seeder | Data |
|--------|------|
| RoleSeeder | SuperAdmin, Admin, NormalUser |
| UserSeeder | 15+ users (Arabic names) with roles + SubSystem |
| DepartmentSeeder | Full Syrian geography hierarchy (governorates, districts, sub-districts) |
| DepartmentUserLinkSeeder | Links users ↔ departments |
| PermissionSeederService | Attribute discovery → SuperAdmin gets all |

#### Validators (FluentValidation)
- `CreateUserDtoValidator` — Username max 100, password min 6
- `CreateRoleDtoValidator` — Name max 100, description max 500
- `CreateDepartmentDtoValidator` — Name max 200, code max 50, description max 1000

### Transaction Module

**Route prefix:** `api/transaction/`

#### Controllers (`Transaction.Api/Controllers`)
| Controller | Key Endpoints |
|------------|---------------|
| RequestsController | Get-by-id, relations (CRUD), create (with file upload), paged filter, by-requester/approver/template, add-files, delete |
| RequestTransactionsController | Get-by-id/by-request, children, root, tree, add-initial/child-transaction, mark-as-managed |
| ResponsesController | Get-by-id, paged by-request/by-responder/by-requester, create, update, add-files, delete |
| TemplatesController | CRUD + get-by-name, ownerships (department/user), by-department, by-user-direct |
| AttachmentsController | Get-by-id, download files (single + zip), remove |
| DepartmentsController | Tree operations |
| LookUpFieldsController | CRUD |
| LookUpFiledValuesController | CRUD + by-lookup-field-id |
| ReportsController | Requests/responses report, dashboard, daily/weekly/monthly, user activity, storage, charts |
| NumberSpellingController | Convert decimal/int/long to Arabic words |

#### Domain Entities (`Transaction.Domain/Entities`)
| Entity | Key Properties |
|--------|----------------|
| Request | Status enum (Pending/Delivered/InProcess/Managed), RequestNumber, Requester/Approver, Response, Transaction tree |
| RequestTransaction | Tree with ParentTransactionId, Level, Path, Status, CurrentUserHolderId |
| Response | RequestId, RespondedByUserId, Comment, Attachments |
| Template | ContentAsJson, TemplateName, IsRequireAttachments, DefaultReceiverDepartmentId |
| RequestAttachment / ResponseAttachment / RequestTransactionAttachment | Junction entities |
| RequestAuditLog | Action enum, UserId, Details, IpAddress, UserAgent |
| InputValue | Key/Value for dynamic form data |
| RequestTemplateValues | Links Template + Request + InputValues |
| RequestRelation | Relation types (Reference/FollowUp/Replacement/Duplicate) + Source→Target |

#### TransactionDbContext
- 17 DbSets with full Fluent API (composite keys, indexes, cascading, relationships)
- TransactionAuditInterceptor — auto-sets CreatedAt/CreatedByUserId/UpdatedAt/UpdatedByUserId

#### Services (`Transaction.Infrastructure/Services`)
RequestService, ResponseService, RequestTransactionService, TemplateService, AttachmentService, WebAttachmentService, LookUpFieldService, LookUpFiledValuesService, ReportService, NumberSpellingWrapperService, RequestAuditService, DepartmentService

### Archive Module

**Route prefix:** `api/archive/`

#### Controllers (`Archive.Api/Controllers`)
| Controller | Key Endpoints |
|------------|---------------|
| ArchiveRecordsController | CRUD, file upload/download, paginated query, search |
| FoldersController | CRUD, hierarchy, permissions |
| DocumentIndexingController | Document upload + OCR + indexing |
| OcrController | OCR trigger |
| SemanticSearchController | Vector search |
| ArchiveConfigController | Configuration management |
| ArchiveDeletionRequestsController | Deletion workflow |
| ArchiveEditRequestsController | Edit workflow |
| DynamicFormsController | Dynamic form management |
| FolderIconsController | Folder icon management |

#### Key Domain Entities
ArchiveRecord, Folder (self-referencing + soft-delete), Document, DocumentChunk, PhysicalFile, ArchiveConfig, ArchiveAuditLog, ArchiveFormTemplate, ArchiveGovernance

#### ArchiveDbContext
- Own DbContext with Archive-specific DbSets
- Folder soft-delete: `HasQueryFilter(f => !f.IsDeleted)`
- ArchiveRecord soft-delete: `HasQueryFilter(ar => !ar.IsDeleted)`

---

## Cross-Cutting Concerns

### AuditableEntity (SaveChangesInterceptor)
```csharp
public class AuditableInterceptor(IHttpContextAccessor httpContextAccessor) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(SaveChangesInterceptorContext context, ...)
    {
        foreach (var entry in context.Context.ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.CreatedByUserId = userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedByUserId = userId;
            }
        }
    }
}
```

### Boot / Program.cs Pattern
```csharp
builder.Services.AddSharedKernel();
// Conditional module registration:
if (config.GetValue<bool>("Modules:<Name>:Enabled"))
    builder.Services.Add<ModuleName>Module(config);
// JWT Auth
builder.Services.AddAuthentication(JwtBearer).AddJwtBearer(...);
builder.Services.AddCors(options => options.AddPolicy("AllowAll", ...));
// Seeding after build:
if (seedingEnabled)
{
    using var scope = app.Services.CreateScope();
    await orchestrator.SeedDatabaseAsync();
}
app.UseSerilogRequestLogging();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/healthz");
```

### Module Registration Pattern
```csharp
public static class TransactionModuleRegistration
{
    public static IServiceCollection AddTransactionModule(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TransactionDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<ITransactionUnitOfWork, TransactionUnitOfWork>();
        services.AddScoped<IRequestService, RequestService>();
        services.AddScoped<ITemplateService, TemplateService>();
        // ... all other module services
        return services;
    }
}
```

### Module UnitOfWork Pattern
```csharp
public interface ITransactionUnitOfWork : IDisposable
{
    IRepositoryBase<Request, Guid> Requests { get; }
    IRepositoryBase<RequestTransaction, Guid> RequestTransactions { get; }
    IRepositoryBase<Response, Guid> Responses { get; }
    IRepositoryBase<Template, Guid> Templates { get; }
    // ... all other repos
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

public class TransactionUnitOfWork(TransactionDbContext context) : ITransactionUnitOfWork
{
    private IRepositoryBase<Request, Guid>? _requests;
    public IRepositoryBase<Request, Guid> Requests =>
        _requests ??= new RepositoryBase<Request, Guid>(context);
    // ... lazy-init all repos
}
```

### Controller Pattern (Primary Constructor)
```csharp
[ApiController]
[Route("api/transaction/[controller]")]
[Authorize]
public class RequestsController(
    IRequestService requestService,
    ILogger<RequestsController> logger) : ControllerBase
{
    [HttpPost]
    [EndpointPermission("requests.create", SubSystem.TransactionSystem, PermissionType.Insert)]
    public async Task<IActionResult> Create([FromForm] CreateRequestDto dto)
    {
        logger.LogInformation("Creating request");
        var result = await requestService.CreateAsync(dto);
        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await requestService.GetByIdAsync(id);
        return result.ToActionResult();
    }
}
```

---

## Supporting Libraries

### ExpressionBuilderLib
- `IExpressionBuilder<T>` — Builds `Expression<Func<T, bool>>` from `PropertyFilter[]`
- `ExpressionCombiner` — Combines expressions with And/Or
- `ComparisonOperator`, `LogicalOperator`, `StringComparisonMode` enums

### FileManager
- `IFileManager` — File system ops: copy/move/rename/delete, search, metadata
- `IFilesManagerService` — Web-focused: SaveFileAsync, DeleteFileAsync, GetFileStreamAsync, CleanupOldFilesAsync
- Files saved with generated safe filenames

### NumberSpelling
- `INumberSpellingService` — `ConvertNumberToArabicWords(decimal/int/long)`
- Uses `NumberToArabicText` NuGet (ArabicWordConverter)

### OcrReader
- `IOcrGenerator` — `ExtractTextFromImageAsync(path, lang)`, `ExtractTextFromPdfAsync(path, lang)`
- Tesseract 5.2.0 + local tessdata directory
- PDF: converts to JPEG via `pdftoppm` → OCR each page

### SemanticSearchLib
- `ITextChunker`, `IFileParser` (docx via DocumentFormat.OpenXml, md via Markdig)
- `IEmbeddingProvider` — connects to Ollama API
- Qdrant vector storage integration

---

## Conventions & Best Practices

### File Structure (per project)
- File-scoped namespaces: `namespace ModernPaySystem.Module.Transaction.Api.Controllers;`
- Order: using → namespace → class → fields → constructor → public methods → private methods

### Naming
- Classes/Interfaces: `PascalCase`
- Methods/Properties: `PascalCase`
- Private fields: `_camelCase`
- Parameters: `camelCase`
- Interfaces: Prefix `I`

### Type Usage
- `<Nullable>enable</Nullable>`, use `string?`
- Always `Task<T>` for async (never `.Result` or `.Wait()`)
- Use `Result<T>` wrapper pattern for all operation outcomes

### Error Handling
- Static error factory: `Error.NotFound("REQ-001", "Request not found", "الطلب غير موجود")`
- Pattern: `var x = await service.GetByIdAsync(id); if (x.IsError) return x.ToActionResult();`
- No exceptions for business logic — use `Result<Error>.Failure(...)`

### Dependency Injection
- Constructor injection with primary constructors
- Modules registered via `Add<Module>Module(IConfiguration)` extension methods
- Shared kernel via `AddSharedKernel()`

### Database
- `Guid` for primary keys (always `Entity<Guid>`)
- Inherit from `Entity<TKey>` + optionally `IAuditableEntity`
- Configure relationships in module's DbContext (`OnModelCreating`)
- Per-module migrations (`dotnet ef migrations add --context <Module>DbContext`)

### Async/Await
- Always `async Task` for I/O
- Never `.Result` or `.Wait()`
- Use `ConfigureAwait(false)` for library code

### Security
- JWT Bearer with short expiration
- `[Authorize]` + `[EndpointPermission(Key, SubSystem, Type)]`
- `PermissionAuthorizationHandler` checks DB on each request
- Validate all input (FluentValidation on DTOs)
- Never commit secrets — use environment variables or user secrets

---

## EF Core Migrations Commands

```bash
# Install EF CLI
dotnet tool install --global dotnet-ef

# Transaction module
dotnet ef migrations add <Name> --context TransactionDbContext --project src/Modules/TransactionSystem/ModernPaySystem.Module.Transaction.Infrastructure --startup-project src/ModernPaySystem.Boot --output-dir Persistence/Migrations

dotnet ef database update --context TransactionDbContext --project src/Modules/TransactionSystem/ModernPaySystem.Module.Transaction.Infrastructure --startup-project src/ModernPaySystem.Boot

# Archive module
dotnet ef migrations add <Name> --context ArchiveDbContext --project src/Modules/ArchiveSystem/ModernPaySystem.Module.Archive.Infrastructure --startup-project src/ModernPaySystem.Boot --output-dir Persistence/Migrations

# Always specify --context when multiple DbContexts exist
```

## Running

```bash
dotnet run --project src/ModernPaySystem.Boot
```

API docs: `http://localhost:64513/scalar/v1` (development only)

Module toggles in `src/ModernPaySystem.Boot/appsettings.json`:
```json
"Modules": {
  "TransactionSystem": { "Enabled": true },
  "Archive": { "Enabled": false },
  "Identity": { "Enabled": true }
}
```

---

## Code Check Commands

```bash
dotnet build src/Refactored.slnx          # Build all projects
dotnet restore src/Refactored.slnx         # Restore packages
dotnet run --project src/ModernPaySystem.Boot  # Run
```
