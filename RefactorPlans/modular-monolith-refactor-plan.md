# Modular Monolith Refactoring Plan

## Target Architecture

```
src/
├── ModernPaySystem.SharedKernel/           # Shared abstractions & primitives
│   ├── Domain/                             # Result<T>, Error, Entity<T>, IAuditableEntity
│   ├── Application/                        # IRepositoryBase<T>, IUnitOfWork (abstraction only), IHttpContextServiceManager
│   └── Infrastructure/                     # EF Core abstractions, common utilities
│
├── Modules/
│   ├── TransactionSystem/                  # Independent module - can be published alone
│   │   ├── ModernPaySystem.Module.Transaction.Domain/        # Entities, Enums, DTOs
│   │   ├── ModernPaySystem.Module.Transaction.Application/   # Interfaces, Use Cases, Validators
│   │   ├── ModernPaySystem.Module.Transaction.Infrastructure/# Service implementations, Persistence
│   │   └── ModernPaySystem.Module.Transaction.Api/           # Controllers, Module registration
│   │
│   ├── Archive/                            # Future module
│   │   └── ...
│   │
│   └── RentSystem/                         # Future new system
│       └── ...
│
└── ModernPaySystem.Boot/                   # The API host - selects which modules to include
    ├── Program.cs
    ├── appsettings.json
    └── ModulesConfiguration.cs             # Toggle modules on/off
```

## Guiding Principles

1. **Zero modifications to the existing codebase** - the monolith stays untouched
2. **Copy + adapt** - copy from monolith, adapt only what's needed for module isolation
3. **Shared Kernel** contains only truly cross-cutting concerns (Result, Entity base, IRepositoryBase)
4. **Each module owns its data** - transaction tables stay in transaction module's DbContext
5. **Module registration** is a single extension method `AddTransactionSystemModule()`
6. **Publish = include the module's project reference in the Boot host**

---

## Phase 0: Setup Project Structure & Solution

### Step 0.1 - Create project directories

```
src/
├── ModernPaySystem.SharedKernel.Domain/
├── ModernPaySystem.SharedKernel.Application/
├── ModernPaySystem.SharedKernel.Infrastructure/
├── Modules/
│   └── TransactionSystem/
│       ├── ModernPaySystem.Module.Transaction.Domain/
│       ├── ModernPaySystem.Module.Transaction.Application/
│       ├── ModernPaySystem.Module.Transaction.Infrastructure/
│       └── ModernPaySystem.Module.Transaction.Api/
└── ModernPaySystem.Boot/
```

### Step 0.2 - Create `.csproj` files

**`src/ModernPaySystem.SharedKernel.Domain/ModernPaySystem.SharedKernel.Domain.csproj`**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Http" Version="2.3.9" />
  </ItemGroup>
</Project>
```

**`src/ModernPaySystem.SharedKernel.Application/ModernPaySystem.SharedKernel.Application.csproj`**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\ModernPaySystem.SharedKernel.Domain\ModernPaySystem.SharedKernel.Domain.csproj" />
  </ItemGroup>
</Project>
```

**`src/ModernPaySystem.SharedKernel.Infrastructure/ModernPaySystem.SharedKernel.Infrastructure.csproj`**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.2" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" Version="10.0.2" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\ModernPaySystem.SharedKernel.Application\ModernPaySystem.SharedKernel.Application.csproj" />
    <ProjectReference Include="..\..\ExpressionBuilderLib\ExpressionBuilderLib.csproj" />
  </ItemGroup>
</Project>
```

**`src/Modules/TransactionSystem/ModernPaySystem.Module.Transaction.Domain/ModernPaySystem.Module.Transaction.Domain.csproj`**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\..\ModernPaySystem.SharedKernel.Domain\ModernPaySystem.SharedKernel.Domain.csproj" />
  </ItemGroup>
</Project>
```

**`src/Modules/TransactionSystem/ModernPaySystem.Module.Transaction.Application/ModernPaySystem.Module.Transaction.Application.csproj`**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\ModernPaySystem.Module.Transaction.Domain\ModernPaySystem.Module.Transaction.Domain.csproj" />
    <ProjectReference Include="..\..\..\ModernPaySystem.SharedKernel.Application\ModernPaySystem.SharedKernel.Application.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="FluentValidation" Version="12.1.1" />
  </ItemGroup>
</Project>
```

**`src/Modules/TransactionSystem/ModernPaySystem.Module.Transaction.Infrastructure/ModernPaySystem.Module.Transaction.Infrastructure.csproj`**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\ModernPaySystem.Module.Transaction.Application\ModernPaySystem.Module.Transaction.Application.csproj" />
    <ProjectReference Include="..\..\..\ModernPaySystem.SharedKernel.Infrastructure\ModernPaySystem.SharedKernel.Infrastructure.csproj" />
    <ProjectReference Include="..\..\..\..\FileManager\FileManager.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.2" />
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.Http" Version="2.3.9" />
  </ItemGroup>
</Project>
```

**`src/Modules/TransactionSystem/ModernPaySystem.Module.Transaction.Api/ModernPaySystem.Module.Transaction.Api.csproj`**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\ModernPaySystem.Module.Transaction.Application\ModernPaySystem.Module.Transaction.Application.csproj" />
    <ProjectReference Include="..\ModernPaySystem.Module.Transaction.Infrastructure\ModernPaySystem.Module.Transaction.Infrastructure.csproj" />
  </ItemGroup>
  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>
</Project>
```

**`src/ModernPaySystem.Boot/ModernPaySystem.Boot.csproj`**
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\Modules\TransactionSystem\ModernPaySystem.Module.Transaction.Api\ModernPaySystem.Module.Transaction.Api.csproj" />
    <!-- Toggle modules on/off by commenting/uncommenting:
    <ProjectReference Include="..\Modules\Archive\..."/>
    -->
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Serilog.AspNetCore" Version="10.0.0" />
    <PackageReference Include="Scalar.AspNetCore" Version="2.14.14" />
  </ItemGroup>
</Project>
```

### Step 0.3 - Update `.slnx`

Add all new projects to the solution file:

```xml
<Solution>
  <!-- existing projects -->
  <Project Path="src/ModernPaySystem.SharedKernel.Domain/ModernPaySystem.SharedKernel.Domain.csproj" />
  <Project Path="src/ModernPaySystem.SharedKernel.Application/ModernPaySystem.SharedKernel.Application.csproj" />
  <Project Path="src/ModernPaySystem.SharedKernel.Infrastructure/ModernPaySystem.SharedKernel.Infrastructure.csproj" />
  <Project Path="src/Modules/TransactionSystem/ModernPaySystem.Module.Transaction.Domain/ModernPaySystem.Module.Transaction.Domain.csproj" />
  <Project Path="src/Modules/TransactionSystem/ModernPaySystem.Module.Transaction.Application/ModernPaySystem.Module.Transaction.Application.csproj" />
  <Project Path="src/Modules/TransactionSystem/ModernPaySystem.Module.Transaction.Infrastructure/ModernPaySystem.Module.Transaction.Infrastructure.csproj" />
  <Project Path="src/Modules/TransactionSystem/ModernPaySystem.Module.Transaction.Api/ModernPaySystem.Module.Transaction.Api.csproj" />
  <Project Path="src/ModernPaySystem.Boot/ModernPaySystem.Boot.csproj" />
</Solution>
```

---

## Phase 1: Shared Kernel (Foundation)

### Step 1.1 - Copy Domain Commons to SharedKernel.Domain

**Copy from** monolith `ModernPaySystem.Domain/Commons/`:
- `IResult.cs` / `IResultOfT.cs` / `ResultOfT.cs` -> `Result<T>`, `Success`, `Error`, `PagedList`
- `Error.cs` -> `readonly record struct Error` with all static factories
- `PagedList.cs`
- `Constants.cs` -> only truly shared constants (not system-specific)

**Copy from** `ModernPaySystem.Domain/Entities/Abstraction/`:
- `Entity.cs` -> `Entity<TKey>` base class
- `IAuditableEntity.cs` -> interface

### Step 1.2 - Copy Application Abstractions to SharedKernel.Application

**Copy from** `ModernPaySystem.Application/Repos/`:
- `IRepositoryBaseT.cs` -> generic repository interface (remove all `global using` that are transaction-specific)

**Copy from** `ModernPaySystem.Application/Services/`:
- `IHttpContextServiceManager.cs` -> the interface

**Create new** shared abstractions:
- `IModuleDbContext.cs` -> interface for module-scoped DbContexts
- `IModuleUnitOfWork.cs` -> smaller interface for module-level unit of work

### Step 1.3 - Create SharedKernel.Infrastructure

**Copy from** `ModernPaySystem.Infrastructure.Persistence/Repos/`:
- `RepositoryBaseT.cs` -> the generic EF Core repository implementation (without mono-specfic global usings)

**Create new**:
- `SharedModuleServiceRegistration.cs` -> registers shared services (HttpContextAccessor, etc.)

**Note**: `IUnitOfWork` should NOT be in shared kernel. Each module will have its own `IUnitOfWork` (or use repositories directly). The monolith's `IUnitOfWork` is a god interface and should not be replicated.

---

## Phase 2: Transaction Module - Domain Layer

### Step 2.1 - Copy domain entities

**Copy from** `ModernPaySystem.Domain/Entities/TransactionSystemEntities/` (17 files):
- `Request.cs` (includes `RequestStatus` enum, `CreateRequestDto`, `RequestDto`, `RelatedRequestDto`)
- `RequestTransaction.cs` (includes `RequestTransactionDto`, `CreateRequestTransactionDto`, `AddInitialRequestTransactionDto`, `TransactionStatus` enum)
- `RequestAttachment.cs`, `Response.cs`, `ResponseAttachment.cs`
- `Template.cs`, `TemplateExpressions.cs`, `TemplateOwnership.cs`, `UserTemplateOwnership.cs`
- `RequestTemplateValues.cs`, `InputValue.cs`
- `RequestRelation.cs`, `RequestExpressions.cs`, `RequestTransactionExpressions.cs`
- `ResponseExpressions.cs`
- `RequestAuditLog.cs`

**Adapt**: Replace any references to `ModernPaySystem.Domain.Entities.SharedEntities.*` with `ModernPaySystem.SharedKernel.Domain.Entities.*` paths.

**Copy from** `ModernPaySystem.Domain/DTOs/`:
- `TransactionReportDtos.cs` -> report DTOs
- `RequestPagedFilterDto.cs` -> paging/filter DTOs
- `DepartmentDto.cs` -> **Note**: DepartmentDto is shared, consider if it should live in SharedKernel

**Copy from** `ModernPaySystem.Domain/Attrs/`:
- `TransactionPermissionAttribute.cs`

> **Important**: The DTOs and `ToDto()` methods that are embedded in entity files (e.g., `RequestDto` inside `Request.cs`) should be **extracted to separate files** in the new module for clean separation.

### Step 2.2 - Fix namespace references

Replace all:
- `namespace ModernPaySystem.Domain.Entities.TransactionSystemEntities;` -> `namespace ModernPaySystem.Module.Transaction.Domain.Entities;`
- `namespace ModernPaySystem.Domain.DTOs;` -> `namespace ModernPaySystem.Module.Transaction.Domain.DTOs;`
- `namespace ModernPaySystem.Domain.Attrs;` -> `namespace ModernPaySystem.Module.Transaction.Domain.Attrs;`

Update all `using ModernPaySystem.Domain.Entities.SharedEntities;` -> `using ModernPaySystem.SharedKernel.Domain.Entities;`
Update all `using ModernPaySystem.Domain.Commons;` -> `using ModernPaySystem.SharedKernel.Domain.Commons;`
Update all `using ModernPaySystem.Domain.Entities.Abstraction;` -> `using ModernPaySystem.SharedKernel.Domain.Entities.Abstraction;`

---

## Phase 3: Transaction Module - Application Layer

### Step 3.1 - Copy service interfaces

**Copy from** `ModernPaySystem.Application/Interfaces/`:
- `IRequestTransactionService.cs`
- `IRequestService.cs`
- `IResponseService.cs`
- `ITemplateService.cs`
- `IAttachmentService.cs`
- `IDepartmentService.cs`
- `ILookUpFieldService.cs`
- `ILookUpFiledValuesService.cs`
- `INumberSpellingWrapperService.cs`

**Copy from** `ModernPaySystem.Application/Interfaces/TransactionSystemInterfaces/`:
- `IReportService.cs`
- `IRequestAuditService.cs`

**Copy from** `ModernPaySystem.Application/Validators/`: any validators for transaction entities

**Adapt**: 
- Replace `global using ModernPaySystem.Domain.Entities.TransactionSystemEntities;` with `global using ModernPaySystem.Module.Transaction.Domain.Entities;`
- Replace any shared kernel references
- **Do NOT use IUnitOfWork in interfaces** - use specific repository interfaces instead

### Step 3.2 - Create module-specific repository interfaces

Instead of the monolith's `IUnitOfWork` god interface, create targeted repository interfaces:

```csharp
// ModernPaySystem.Module.Transaction.Application/Repos/IRequestRepository.cs
public interface IRequestRepository : IRepositoryBase<Request, Guid>
{
    Task<Request?> GetWithFullDetailsAsync(Guid id);
    Task<PagedList<Request>> GetPagedWithFilterAsync(RequestPagedFilterDto filter);
}
```

```csharp
// ModernPaySystem.Module.Transaction.Application/Repos/IRequestTransactionRepository.cs
public interface IRequestTransactionRepository : IRepositoryBase<RequestTransaction, Guid>
{
    Task<RequestTransaction?> GetWithTreeAsync(Guid id);
}
```

### Step 3.3 - Create ITransactionUnitOfWork (module-scoped)

```csharp
// ModernPaySystem.Module.Transaction.Application/ITransactionUnitOfWork.cs
public interface ITransactionUnitOfWork
{
    IRequestRepository Requests { get; }
    IRequestTransactionRepository RequestTransactions { get; }
    IRepositoryBase<Response, Guid> Responses { get; }
    IRepositoryBase<Template, Guid> Templates { get; }
    IRepositoryBase<RequestRelation, Guid> RequestRelations { get; }
    IRepositoryBase<RequestAuditLog, Guid> RequestAuditLogs { get; }
    IRepositoryBase<Attachment, Guid> Attachments { get; }
    // ... etc, only transaction-related entities
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
```

---

## Phase 4: Transaction Module - Infrastructure Layer

### Step 4.1 - Create the module's DbContext

```csharp
// ModernPaySystem.Module.Transaction.Infrastructure/TransactionDbContext.cs
public class TransactionDbContext : DbContext
{
    // Only transaction entity DbSets
    public DbSet<Request> Requests { get; set; }
    public DbSet<RequestTransaction> RequestTransactions { get; set; }
    public DbSet<Template> Templates { get; set; }
    public DbSet<Response> Responses { get; set; }
    // ... etc

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Copy ONLY transaction-related entity configurations from AppDbContext
        // NOT archive, not fast operations
    }
}
```

### Step 4.2 - Copy service implementations

**Copy from** `ModernPaySystem.Infrastructure/Services/`:
- `RequestTransactionService.cs`
- `RequestService.cs`
- `ResponseService.cs`
- `TemplateService.cs`
- `AttachmentService.cs` / `WebAttachmentService.cs`
- `DepartmentService.cs`
- `LookUpFieldService.cs` / `LookUpFiledValuesService.cs`
- `ReportService.cs`
- `RequestAuditService.cs`
- `NumberSpellingWrapperService.cs`

**Adapt each service implementation**:
- Replace `IUnitOfWork` dependency with `ITransactionUnitOfWork`
- Replace `IWebAttachmentService` with a module-local interface
- Replace `IHttpContextServiceManager` (kept from SharedKernel.Application)
- Update all namespaces

### Step 4.3 - Create module-level repository implementations

```csharp
// ModernPaySystem.Module.Transaction.Infrastructure/Repos/RequestRepository.cs
public class RequestRepository(IBaseRepository<Request, Guid> innerRepo) : IRequestRepository
{
    // Delegates to the generic SharedKernel repository
    // Adds specialized methods like GetWithFullDetailsAsync
}
```

### Step 4.4 - Create Module Registration

```csharp
// ModernPaySystem.Module.Transaction.Infrastructure/TransactionModuleRegistration.cs
public static class TransactionModuleRegistration
{
    public static IServiceCollection AddTransactionSystemModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DbContext
        services.AddDbContext<TransactionDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Register repositories
        services.AddScoped<IRequestRepository, RequestRepository>();
        services.AddScoped<ITransactionUnitOfWork, TransactionUnitOfWork>();

        // Register services
        services.AddScoped<IRequestService, RequestService>();
        services.AddScoped<IRequestTransactionService, RequestTransactionService>();
        services.AddScoped<IResponseService, ResponseService>();
        services.AddScoped<ITemplateService, TemplateService>();
        // ... etc

        // Register controllers if needed
        services.AddControllers()
            .AddApplicationPart(typeof(TransactionModuleRegistration).Assembly);

        return services;
    }
}
```

---

## Phase 5: Transaction Module - API Layer (Controllers)

### Step 5.1 - Copy controllers

**Copy from** `ModernPaySystem/Controllers/TransactionsSystemControllers/`:
- `RequestTransactionsController.cs`
- `RequestsController.cs`
- `ResponsesController.cs`
- `TemplatesController.cs`
- `AttachmentsController.cs`
- `DepartmentsController.cs`
- `LookUpFieldsController.cs`
- `LookUpFiledValuesController.cs`
- `ReportsController.cs`
- `NumberSpellingController.cs`

**Adapt**:
- Update namespaces to `ModernPaySystem.Module.Transaction.Api.Controllers`
- Add `[Route("api/transaction/[controller]")]` to avoid route conflicts when multiple modules are active
- Remove any reference to monolithic `Program.cs`-level configurations

---

## Phase 6: Boot Host - The Publish Entry Point

### Step 6.1 - Create `Program.cs`

```csharp
// src/ModernPaySystem.Boot/Program.cs
var builder = WebApplication.CreateBuilder(args);

// --- SHARED SERVICES ---
builder.Services.AddSharedKernel(builder.Configuration);

// --- MODULE SELECTION ---
// ModulesConfiguration.cs reads config to decide which modules to load
var modulesConfig = builder.Configuration.GetSection("Modules").Get<ModulesConfiguration>();

if (modulesConfig?.TransactionSystem?.Enabled == true)
{
    builder.Services.AddTransactionSystemModule(builder.Configuration);
}

if (modulesConfig?.Archive?.Enabled == true)
{
    // builder.Services.AddArchiveModule(builder.Configuration);
}

// --- COMMON ---
builder.Services.AddControllers();
builder.Services.AddSerilog(...);
builder.Services.AddOpenApi(...);

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

### Step 6.2 - Configuration-based module selection

```json
// src/ModernPaySystem.Boot/appsettings.json
{
  "Modules": {
    "TransactionSystem": {
      "Enabled": true,
      "ConnectionString": "..."
    },
    "Archive": {
      "Enabled": false
    }
  }
}
```

---

## Phase 7: File-by-File Copy Checklist (Transaction System)

### Domain (17 entity files + DTOs + Attributes)

| # | File | Source (monolith) | Destination (module) |
|---|------|-------------------|---------------------|
| 1 | Request.cs | `Domain/Entities/TransactionSystemEntities/Request.cs` | `Modules/TransactionSystem/Domain/Entities/Request.cs` |
| 2 | RequestTransaction.cs | `Domain/Entities/TransactionSystemEntities/RequestTransaction.cs` | `Modules/TransactionSystem/Domain/Entities/RequestTransaction.cs` |
| 3 | RequestAttachment.cs | `Domain/Entities/TransactionSystemEntities/RequestAttachment.cs` | `Modules/TransactionSystem/Domain/Entities/RequestAttachment.cs` |
| 4 | RequestTransactionAttachment.cs | `Domain/Entities/TransactionSystemEntities/RequestTransactionAttachment.cs` | `Modules/TransactionSystem/Domain/Entities/RequestTransactionAttachment.cs` |
| 5 | Response.cs | `Domain/Entities/TransactionSystemEntities/Response.cs` | `Modules/TransactionSystem/Domain/Entities/Response.cs` |
| 6 | ResponseAttachment.cs | `Domain/Entities/TransactionSystemEntities/ResponseAttachment.cs` | `Modules/TransactionSystem/Domain/Entities/ResponseAttachment.cs` |
| 7 | Template.cs | `Domain/Entities/TransactionSystemEntities/Template.cs` | `Modules/TransactionSystem/Domain/Entities/Template.cs` |
| 8 | TemplateExpressions.cs | `Domain/Entities/TransactionSystemEntities/TemplateExpressions.cs` | `Modules/TransactionSystem/Domain/Entities/TemplateExpressions.cs` |
| 9 | TemplateOwnership.cs | `Domain/Entities/TransactionSystemEntities/TemplateOwnership.cs` | `Modules/TransactionSystem/Domain/Entities/TemplateOwnership.cs` |
| 10 | UserTemplateOwnership.cs | `Domain/Entities/TransactionSystemEntities/UserTemplateOwnership.cs` | `Modules/TransactionSystem/Domain/Entities/UserTemplateOwnership.cs` |
| 11 | RequestTemplateValues.cs | `Domain/Entities/TransactionSystemEntities/RequestTemplateValues.cs` | `Modules/TransactionSystem/Domain/Entities/RequestTemplateValues.cs` |
| 12 | InputValue.cs | `Domain/Entities/TransactionSystemEntities/InputValue.cs` | `Modules/TransactionSystem/Domain/Entities/InputValue.cs` |
| 13 | RequestRelation.cs | `Domain/Entities/TransactionSystemEntities/RequestRelation.cs` | `Modules/TransactionSystem/Domain/Entities/RequestRelation.cs` |
| 14 | RequestExpressions.cs | `Domain/Entities/TransactionSystemEntities/RequestExpressions.cs` | `Modules/TransactionSystem/Domain/Entities/RequestExpressions.cs` |
| 15 | RequestTransactionExpressions.cs | `Domain/Entities/TransactionSystemEntities/RequestTransactionExpressions.cs` | `Modules/TransactionSystem/Domain/Entities/RequestTransactionExpressions.cs` |
| 16 | ResponseExpressions.cs | `Domain/Entities/TransactionSystemEntities/ResponseExpressions.cs` | `Modules/TransactionSystem/Domain/Entities/ResponseExpressions.cs` |
| 17 | RequestAuditLog.cs | `Domain/Entities/TransactionSystemEntities/RequestAuditLog.cs` | `Modules/TransactionSystem/Domain/Entities/RequestAuditLog.cs` |
| 18 | TransactionReportDtos.cs | `Domain/DTOs/TransactionReportDtos.cs` | `Modules/TransactionSystem/Domain/DTOs/TransactionReportDtos.cs` |
| 19 | RequestPagedFilterDto.cs | `Domain/DTOs/RequestPagedFilterDto.cs` | `Modules/TransactionSystem/Domain/DTOs/RequestPagedFilterDto.cs` |
| 20 | DepartmentDto.cs | `Domain/DTOs/DepartmentDto.cs` | `Modules/TransactionSystem/Domain/DTOs/DepartmentDto.cs` |
| 21 | TransactionPermissionAttribute.cs | `Domain/Attrs/TransactionPermissionAttribute.cs` | `Modules/TransactionSystem/Domain/Attrs/TransactionPermissionAttribute.cs` |
| 22 | Constants.cs (transaction parts) | `Domain/Commons/Constants.cs` | `Modules/TransactionSystem/Domain/Constants.cs` |
| 23 | ApplicationErrors.cs (transaction parts) | `Domain/Commons/ApplicationErrors.cs` | `Modules/TransactionSystem/Domain/Errors.cs` |

### Application (11 interface files + Validators)

| # | File | Source | Destination |
|---|------|--------|-------------|
| 24 | IRequestTransactionService.cs | `Application/Interfaces/IRequestTransactionService.cs` | `Modules/TransactionSystem/Application/Interfaces/IRequestTransactionService.cs` |
| 25 | IRequestService.cs | `Application/Interfaces/IRequestService.cs` | `Modules/TransactionSystem/Application/Interfaces/IRequestService.cs` |
| 26 | IResponseService.cs | `Application/Interfaces/IResponseService.cs` | `Modules/TransactionSystem/Application/Interfaces/IResponseService.cs` |
| 27 | ITemplateService.cs | `Application/Interfaces/ITemplateService.cs` | `Modules/TransactionSystem/Application/Interfaces/ITemplateService.cs` |
| 28 | IAttachmentService.cs | `Application/Interfaces/IAttachmentService.cs` | `Modules/TransactionSystem/Application/Interfaces/IAttachmentService.cs` |
| 29 | IDepartmentService.cs | `Application/Interfaces/IDepartmentService.cs` | `Modules/TransactionSystem/Application/Interfaces/IDepartmentService.cs` |
| 30 | ILookUpFieldService.cs | `Application/Interfaces/ILookUpFieldService.cs` | `Modules/TransactionSystem/Application/Interfaces/ILookUpFieldService.cs` |
| 31 | ILookUpFiledValuesService.cs | `Application/Interfaces/ILookUpFiledValuesService.cs` | `Modules/TransactionSystem/Application/Interfaces/ILookUpFiledValuesService.cs` |
| 32 | INumberSpellingWrapperService.cs | `Application/Interfaces/INumberSpellingWrapperService.cs` | `Modules/TransactionSystem/Application/Interfaces/INumberSpellingWrapperService.cs` |
| 33 | IReportService.cs | `Application/Interfaces/TransactionSystemInterfaces/IReportService.cs` | `Modules/TransactionSystem/Application/Interfaces/IReportService.cs` |
| 34 | IRequestAuditService.cs | `Application/Interfaces/TransactionSystemInterfaces/IRequestAuditService.cs` | `Modules/TransactionSystem/Application/Interfaces/IRequestAuditService.cs` |
| 35 | IWebAttachmentService.cs | `Infrastructure/Services/IWebAttachmentService.cs` | `Modules/TransactionSystem/Application/Interfaces/IWebAttachmentService.cs` |
| 36 | Validators | `Application/Validators/*` | `Modules/TransactionSystem/Application/Validators/*` |

### Infrastructure (12 service implementation files)

| # | File | Source | Destination |
|---|------|--------|-------------|
| 37 | RequestTransactionService.cs | `Infrastructure/Services/RequestTransactionService.cs` | `Modules/TransactionSystem/Infrastructure/Services/RequestTransactionService.cs` |
| 38 | RequestService.cs | `Infrastructure/Services/RequestService.cs` | `Modules/TransactionSystem/Infrastructure/Services/RequestService.cs` |
| 39 | ResponseService.cs | `Infrastructure/Services/ResponseService.cs` | `Modules/TransactionSystem/Infrastructure/Services/ResponseService.cs` |
| 40 | TemplateService.cs | `Infrastructure/Services/TemplateService.cs` | `Modules/TransactionSystem/Infrastructure/Services/TemplateService.cs` |
| 41 | WebAttachmentService.cs | `Infrastructure/Services/WebAttachmentService.cs` | `Modules/TransactionSystem/Infrastructure/Services/WebAttachmentService.cs` |
| 42 | DepartmentService.cs | `Infrastructure/Services/DepartmentService.cs` | `Modules/TransactionSystem/Infrastructure/Services/DepartmentService.cs` |
| 43 | LookUpFieldService.cs | `Infrastructure/Services/LookUpFieldService.cs` | `Modules/TransactionSystem/Infrastructure/Services/LookUpFieldService.cs` |
| 44 | LookUpFiledValuesService.cs | `Infrastructure/Services/LookUpFiledValuesService.cs` | `Modules/TransactionSystem/Infrastructure/Services/LookUpFiledValuesService.cs` |
| 45 | ReportService.cs | `Infrastructure/Services/ReportService.cs` | `Modules/TransactionSystem/Infrastructure/Services/ReportService.cs` |
| 46 | RequestAuditService.cs | `Infrastructure/Services/RequestAuditService.cs` | `Modules/TransactionSystem/Infrastructure/Services/RequestAuditService.cs` |
| 47 | NumberSpellingWrapperService.cs | `Infrastructure/Services/NumberSpellingWrapperService.cs` | `Modules/TransactionSystem/Infrastructure/Services/NumberSpellingWrapperService.cs` |
| 48 | TransactionDbContext.cs | *(new)* | `Modules/TransactionSystem/Infrastructure/Persistence/TransactionDbContext.cs` |
| 49 | TransactionUnitOfWork.cs | *(new)* | `Modules/TransactionSystem/Infrastructure/Persistence/TransactionUnitOfWork.cs` |
| 50 | RequestRepository.cs | *(new)* | `Modules/TransactionSystem/Infrastructure/Repos/RequestRepository.cs` |

### API (10 controller files)

| # | File | Source | Destination |
|---|------|--------|-------------|
| 51 | RequestTransactionsController.cs | `Controllers/TransactionsSystemControllers/RequestTransactionsController.cs` | `Modules/TransactionSystem/Api/Controllers/RequestTransactionsController.cs` |
| 52 | RequestsController.cs | `Controllers/TransactionsSystemControllers/RequestsController.cs` | `Modules/TransactionSystem/Api/Controllers/RequestsController.cs` |
| 53 | ResponsesController.cs | `Controllers/TransactionsSystemControllers/ResponsesController.cs` | `Modules/TransactionSystem/Api/Controllers/ResponsesController.cs` |
| 54 | TemplatesController.cs | `Controllers/TransactionsSystemControllers/TemplatesController.cs` | `Modules/TransactionSystem/Api/Controllers/TemplatesController.cs` |
| 55 | AttachmentsController.cs | `Controllers/TransactionsSystemControllers/AttachmentsController.cs` | `Modules/TransactionSystem/Api/Controllers/AttachmentsController.cs` |
| 56 | DepartmentsController.cs | `Controllers/TransactionsSystemControllers/DepartmentsController.cs` | `Modules/TransactionSystem/Api/Controllers/DepartmentsController.cs` |
| 57 | LookUpFieldsController.cs | `Controllers/TransactionsSystemControllers/LookUpFieldsController.cs` | `Modules/TransactionSystem/Api/Controllers/LookUpFieldsController.cs` |
| 58 | LookUpFiledValuesController.cs | `Controllers/TransactionsSystemControllers/LookUpFiledValuesController.cs` | `Modules/TransactionSystem/Api/Controllers/LookUpFiledValuesController.cs` |
| 59 | ReportsController.cs | `Controllers/TransactionsSystemControllers/ReportsController.cs` | `Modules/TransactionSystem/Api/Controllers/ReportsController.cs` |
| 60 | NumberSpellingController.cs | `Controllers/TransactionsSystemControllers/NumberSpellingController.cs` | `Modules/TransactionSystem/Api/Controllers/NumberSpellingController.cs` |

---

## Phase 8: Adapting the Code - Key Changes

### 8.1 - Extract DTOs from entity files

In the monolith, DTOs are embedded inside entity files (e.g., `RequestDto` inside `Request.cs`). In the module:
- Keep entities in `Domain/Entities/*.cs`
- Extract DTOs to `Domain/DTOs/*.cs`
- Extract mapping methods (`ToDto()`) to extension methods in `Domain/Mappings/*.cs`

### 8.2 - Replace `IUnitOfWork` with `ITransactionUnitOfWork`

**Before** (monolith):
```csharp
public class RequestService(IUnitOfWork unitOfWork, ...) : IRequestService
{
    public async Task<Result<...>> Method()
    {
        var request = await unitOfWork.Requests.GetAsync(...);
        var user = await unitOfWork.Users.GetAsync(...);
    }
}
```

**After** (module):
```csharp
public class RequestService(ITransactionUnitOfWork unitOfWork, ISharedUserRepository users, ...) : IRequestService
{
    public async Task<Result<...>> Method()
    {
        var request = await unitOfWork.Requests.GetAsync(...);
        var user = await users.GetByIdAsync(...);
    }
}
```

### 8.3 - External dependencies (shared entities)

The Transaction module depends on these shared entities:
- `User` → needs `IGuidRepository<User>` (or a shared `IUserRepository` in SharedKernel)
- `Department` → needs `IGuidRepository<Department>`
- `Attachment` → needs `IGuidRepository<Attachment>`
- `Role`, `PermissionEntity` → for auth checks

**Solution**: The SharedKernel.Infrastructure project provides:
- Shared entity definitions (User, Department, Attachment - as POCOs or with minimal behavior)
- `ISharedReadRepository<T>` for read-only access
- The Boot host registers shared repositories that all modules can use

### 8.4 - Shared Entity Access Strategy

Since the transaction module needs to read User and Department data (but full CRUD is owned by auth/user management), create read-only abstractions:

```csharp
// SharedKernel.Application
public interface ISharedReadRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null);
    Task<bool> AnyAsync(Expression<Func<T, bool>> filter);
}
```

The Boot host registers the implementation, and each module consumes it.

### 8.5 - Auth/Authorization in Modules

Each module handles its own authorization:
- `TransactionPermissionAttribute` stays in the module
- Permission checks reference shared `IUserContext` from SharedKernel
- `IHttpContextServiceManager` (shared) provides current user info

---

## Phase 9: Verification & Testing

### Step 9.1 - Build verification

```bash
dotnet build src/ModernPaySystem.Boot/ModernPaySystem.Boot.csproj
```

### Step 9.2 - Empty module test (isolated)

Create a test configuration that only enables Transaction module, start the Boot project, and verify:
- Only transaction endpoints are available at `/api/transaction/*`
- Health check passes
- Database migrations apply only transaction tables

### Step 9.3 - Full integration test (all modules)

For future: enable all modules, verify no route conflicts, shared DB context works.

---

## Phase 10: Adding a New System (e.g., RentSystem)

### Step 10.1 - Create module structure

```
src/Modules/RentSystem/
├── ModernPaySystem.Module.Rent.Domain/
├── ModernPaySystem.Module.Rent.Application/
├── ModernPaySystem.Module.Rent.Infrastructure/
└── ModernPaySystem.Module.Rent.Api/
```

### Step 10.2 - Follow same pattern
1. **Domain**: Create entities that inherit from `Entity<Guid>` in SharedKernel
2. **Application**: Create interfaces, validators, repository interfaces
3. **Infrastructure**: Create `RentDbContext` (owns its own tables), services, repositories
4. **Api**: Create controllers with `[Route("api/rent/[controller]")]`
5. **Registration**: Create `AddRentSystemModule(this IServiceCollection, IConfiguration)`

### Step 10.3 - Register in Boot

```csharp
if (modulesConfig?.RentSystem?.Enabled == true)
{
    builder.Services.AddRentSystemModule(builder.Configuration);
}
```

That's it - the module pattern is fully reusable.

---

## Migration Strategy

### Parallel Operation
- The monolith (`ModernPaySystem/`) continues to work as-is
- The new modular system (`src/`) runs independently with its own copy of the code
- Both can run side-by-side during migration

### Data Strategy
- **Phase 1**: New module uses the same database (separate schema or same tables)
- **Phase 2**: Optionally migrate to isolated database per module
- Transaction module owns transaction tables, SharedKernel owns user/department tables

### Go-Live
1. Build and test the `src/ModernPaySystem.Boot` project
2. Route traffic to the new boot project
3. Keep old monolith as fallback
4. Decommission old monolith when all modules are migrated

---

## Summary: Key Architectural Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Module directory structure | `src/Modules/{ModuleName}/{Layer}` | Clear isolation, follows conventions |
| Shared entities | Read-only repositories in SharedKernel | Avoids duplication, maintains data consistency |
| DbContext per module | Yes, one per module | Each module owns its tables, independent migrations |
| Route prefix | `/api/{module}/[controller]` | Prevents route conflicts when combining modules |
| Module registration | Extension method pattern | Consistent, testable, composable |
| DTOs separate from entities | Yes, extract | Clean separation, SRP |
| Dependency direction | Modules → SharedKernel (never reverse) | No circular dependencies |
| Testing | New tests in module projects | Existing monolith tests remain untouched |
