# ModernPaySystem — Modular Monolith Guide

## Project Structure

```
src/
├── ModernPaySystem.Boot/                         # Host — entry point, wires everything
│   ├── Program.cs                                # Startup: config toggle → registers active modules
│   └── appsettings.json                          # ConnectionStrings, module toggles
│
├── ModernPaySystem.SharedKernel.Domain/          # Shared domain primitives
│   └── Entities/   Entity base, Result&lt;T&gt;, PagedList,
│                    shared entity stubs (User, Department, Attachment, Role, etc.),
│                    SubSystem enum, EndpointPermissionAttribute
│   └── Identity/   CurrentUser record
│
├── ModernPaySystem.SharedKernel.Application/     # Shared application contracts
│   └── Repos/      IRepositoryBase&lt;T, TId&gt;, ISharedReadRepository&lt;T&gt;
│   └── Interfaces/ ICurrentUserService
│   └── Services/   IHttpContextServiceManager
│
├── ModernPaySystem.SharedKernel.Infrastructure/  # Shared infrastructure
│   └── Persistence/ RepositoryBaseT (EF Core impl)
│   └── Services/   HttpContextServiceManager, CurrentUserService
│   └── SharedKernelServiceRegistration.cs        # DI: AddSharedKernel()
│
├── ModernPaySystem.SharedKernel.Domain/Commons/  # Result&lt;T&gt;, Error, PagedList, ErrorKind
│
└── Modules/
    ├── TransactionSystem/                        # Transaction Module (own DbContext)
    │   ├── ModernPaySystem.Module.Transaction.Domain/
    │   │   └── Entities/     17 entity classes (Request, RequestTransaction, Response, etc.)
    │   │   └── DTOs/         Report DTOs, filter DTOs
    │   │   └── Attrs/        TransactionPermissionAttribute
    │   │
    │   ├── ModernPaySystem.Module.Transaction.Application/
    │   │   ├── ITransactionUnitOfWork.cs          # UoW contract (14 repos)
    │   │   └── Interfaces/   12 service interfaces
    │   │                       (IRequestService, IRequestTransactionService, IResponseService,
    │   │                        ITemplateService, IAttachmentService, IDepartmentService,
    │   │                        ILookUpFieldService, ILookUpFiledValuesService,
    │   │                        INumberSpellingWrapperService, IReportService,
    │   │                        IRequestAuditService, IWebAttachmentService)
    │   │
    │   ├── ModernPaySystem.Module.Transaction.Infrastructure/
    │   │   ├── Persistence/  TransactionDbContext  # EF Core context (module entities only)
    │   │   ├── Interceptors/ TransactionAuditInterceptor
    │   │   ├── TransactionUnitOfWork.cs           # ITransactionUnitOfWork impl
    │   │   └── TransactionModuleRegistration.cs   # DI: AddTransactionModule()
    │   │
    │   └── ModernPaySystem.Module.Transaction.Api/
    │       ├── Controllers/     10 controllers (api/transaction/... prefix)
    │       └── Extensions/      ResultExtensions (ToActionResult)
    │
    └── IdentitySystem/                           # Identity Module (shared tables — Option B)
        ├── ModernPaySystem.Module.Identity.Domain/
        │   └── Attrs/        IdentityPermissionAttribute
        │
        ├── ModernPaySystem.Module.Identity.Application/
        │   └── (empty placeholder — future migration target)
        │
        ├── ModernPaySystem.Module.Identity.Infrastructure/
        │   └── IdentityModuleRegistration.cs     # DI: AddIdentityModule()
        │
        └── ModernPaySystem.Module.Identity.Api/
            ├── Controllers/  AuthController, UsersController,
            │                 DepartmentsController, RolesController
            └── GlobalUsings.cs
```

## Module Contract

### Option A — Own DbContext (Transaction, Archive)

```
src/Modules/ArchiveSystem/
├── ModernPaySystem.Module.Archive.Domain/        # Entities, DTOs
├── ModernPaySystem.Module.Archive.Application/   # IArchiveUnitOfWork, service interfaces
├── ModernPaySystem.Module.Archive.Infrastructure/ # ArchiveDbContext, UnitOfWork, DI registration
└── ModernPaySystem.Module.Archive.Api/           # Controllers (api/archive/...)
```

Each module must provide:
1. A `*ModuleRegistration.cs` extension method on `IServiceCollection`
2. Its own `DbContext` (only its own entity `DbSet`s)
3. Its own controllers with an isolated route prefix
4. Its own `UnitOfWork` that wraps `RepositoryBase&lt;T, Guid&gt;` from SharedKernel

### Option B — Shared DbContext (Identity)

Identity module reuses the monolith's `AppDbContext` (shared schema with identity tables:
Users, Departments, Roles, Permissions). It does NOT own its own DbContext or UnitOfWork.

Instead, Identity.Infrastructure registers the monolith's existing service implementations
via DI:
- `IAuthenticationService` → `AuthenticationService`
- `IUserService` → `UserService`
- `IRoleService` → `RoleService`
- `IDepartmentService` → `DepartmentService`
- `ITokenService` → `JwtTokenService`

Controllers live in Identity.Api but reference monolith DTOs and service interfaces
(currently the monolith's `ModernPaySystem.Domain/Application` assemblies). This is a
transitional arrangement — when the monolith is fully decomposed, Identity.Application
and Identity.Infrastructure will own their own contracts and implementations.

## Running the App

```bash
dotnet run --project src/ModernPaySystem.Boot
```

Toggle modules in `src/ModernPaySystem.Boot/appsettings.json`:
```json
"Modules": {
  "TransactionSystem": { "Enabled": true },
  "Archive": { "Enabled": false },
  "Identity": { "Enabled": true }
}
```

API docs: http://localhost:64513/scalar/v1 (development only)

## EF Core Migrations with Multiple DbContexts

Each module's DbContext manages its own migration history independently.

### Install EF CLI (if not already)
```bash
dotnet tool install --global dotnet-ef
```

### Transaction Module

```bash
# Create migration
dotnet ef migrations add InitialCreate \
    --context TransactionDbContext \
    --project src/Modules/TransactionSystem/ModernPaySystem.Module.Transaction.Infrastructure \
    --startup-project src/ModernPaySystem.Boot \
    --output-dir Persistence/Migrations

# Apply
dotnet ef database update \
    --context TransactionDbContext \
    --project src/Modules/TransactionSystem/ModernPaySystem.Module.Transaction.Infrastructure \
    --startup-project src/ModernPaySystem.Boot
```

### Future Module with Own DbContext (e.g. Archive)

```bash
# Create migration
dotnet ef migrations add InitialCreate \
    --context ArchiveDbContext \
    --project src/Modules/ArchiveSystem/ModernPaySystem.Module.Archive.Infrastructure \
    --startup-project src/ModernPaySystem.Boot \
    --output-dir Persistence/Migrations

# Apply
dotnet ef database update \
    --context ArchiveDbContext \
    --project src/Modules/ArchiveSystem/ModernPaySystem.Module.Archive.Infrastructure \
    --startup-project src/ModernPaySystem.Boot
```

### Identity Module (Shared DbContext)

Identity module reuses the monolith's `AppDbContext` — no separate DbContext or migrations.
Identity tables (`Users`, `Departments`, `Roles`, `RolePermissions`) live in the monolith's
schema managed by `ModernPaySystem.Infrastructure.Persistence`.

### Key Points
- **Always specify `--context`** — EF can't auto-discover when multiple DbContexts exist
- **`--output-dir`** keeps migrations inside the module's Infrastructure project
- **`--startup-project`** points to the Boot host (it has the connection string + DI config)
- Each module's migrations table is independent (default `__EFMigrationsHistory` per context)
- The Boot project references all module Api projects, so all contexts are resolvable
- Modules using Option B (shared DbContext) have no migrations of their own — they share the monolith's migration history

## Architecture Decisions

| Decision | Rationale |
|----------|-----------|
| Shared entity stubs (User, Dept, etc.) | Avoid circular dependencies; module entities reference shared types by FK only |
| RepositoryBaseT uses `DbContext` (not `AppDbContext`) | Works with any module's DbContext |
| Route prefix `api/transaction/` | Isolates module endpoints, avoids collisions |
| `ISharedReadRepository&lt;T&gt;` | Modules read shared data without owning its schema or write access |
| Module-level UoW + DbContext | Each module is independently deployable and testable |
| **Option B** (shared tables for Identity) | Avoids cross-DbContext FK issues during migration; identity tables stay in monolith's AppDbContext with Transaction + Archive tables |
| Identity module delegates to monolith's services | Transitional — avoids duplicate implementation while moving controllers to module; Identity.Application/Infrastructure will own contracts when monolith is gone |
| Identity controllers use monolith's `Result&lt;T&gt;.ToActionResult()` | Avoids porting the extension method; same pattern used by original monolith controllers |
