# Agent Skill — ModernPaySystem

> **IMPORTANT:** Before finishing ANY implementation task, verify the KB is updated. See `04-Agents/00-Self-Improvement-Skill/self-improvement.md` for the self-update protocol.

## Architecture

```
API → Application → Domain ← Persistence ← Infrastructure
```

| Layer | Project | Role |
|-------|---------|------|
| API | `ModernPaySystem` | Controllers, middleware, DI wiring |
| Application | `ModernPaySystem.Application` | Services, DTOs, interfaces, validators |
| Domain | `ModernPaySystem.Domain` | Entities, value objects, business rules |
| Persistence | `ModernPaySystem.Infrastructure.Persistence` | EF Core, repositories, migrations |
| Infrastructure | `ModernPaySystem.Infrastructure` | Auth, email, file I/O, integrations |

## Dependency Rules

```
Domain → (none)
Application → Domain
Persistence → Application, Domain
Infrastructure → Application
API → Application, Persistence, Infrastructure
```

### Forbidden

```
Application → Persistence / Infrastructure
Persistence → Infrastructure
Domain → anything
```

## Naming Conventions

| Element | Style | Example |
|---------|-------|---------|
| Class/Interface | `PascalCase` | `UserService`, `IUserRepository` |
| Methods | `PascalCase` | `GetByIdAsync` |
| Private fields | `_camelCase` | `_userService` |
| Parameters | `camelCase` | `CreateUserDto dto` |
| Interfaces | Prefix `I` | `IUserService` |
| Files | Match class name | `UserService.cs` |

## Folder Conventions

### Backend

```
Controllers/     → {Name}Controller.cs
Services/        → {Name}Service.cs
Interfaces/      → I{Name}Service.cs
DTOs/{Entity}/   → {Name}Dto.cs, Create{Name}Dto.cs
Entities/        → {Name}.cs, {Name}Expressions.cs
Repos/           → RepositoryBaseT.cs (generic base)
UnitOfWork/      → IUnitOfWork.cs, UnitOfWork.cs
ValueObjects/    → {Name}.cs
Repositories/    → {Name}Repository.cs
Configurations/  → {Name}Configuration.cs
Middleware/      → {Name}Middleware.cs
```

### Frontend

```
src/components/      → Reusable UI
src/features/        → Feature modules
src/hooks/           → use{Name}.ts
src/services/        → {name}Service.ts
src/types/           → {Name}.ts
src/store/           → Redux slices
src/utils/           → Utilities
```

## Coding Standards

### C#

- File-scoped namespaces: `namespace ModernPaySystem.Controllers;`
- Primary constructors for DI: `public class UserService(IUserService svc)`
- `Result<T>` for all service returns, never throw for expected failures
- Always `async Task`, never `.Result` or `.Wait()`
- Accept `CancellationToken ct` on all async methods
- No business logic in controllers or repositories
- Private setters on entities, no public `{ get; set; }` for domain state
- `IReadOnlyList<T>` for collections, backed by `private readonly List<T>`

### UnitOfWork Pattern

- Services inject `IUnitOfWork` — the ONLY gateway to data access
- Repositories are NEVER injected directly in services
- Every new entity requires a property in `IUnitOfWork`
- `RepositoryBase<TEntity, TKey>` (Persistence) implements `IRepositoryBase<TEntity, TKey>` (Application)
- Services access data via `unitOfWork.{PluralEntityName}.{Method}()`

### Database

- Uses PostgreSQL with Npgsql.EntityFrameworkCore.PostgreSQL (NOT SQL Server)

### TypeScript/React

- Functional components with hooks
- TanStack Query for server state
- Redux Toolkit for global client state
- Imports: external → `@/` absolute → relative → type imports
- `cn()` from `tailwind-merge` for conditional classes
- Props typed explicitly (never `any`)

## Refactoring Rules

| Smell | Fix |
|-------|-----|
| Business logic in controller | Move to Application service |
| EF Core in Application | Extract to Persistence, use interface |
| `IQueryable` from repository | Close repository — return `Task<List<T>>` |
| `new Service()` in controller | Inject via constructor |
| Anemic entity (get/set bag) | Encapsulate behavior on entity |
| Large service (>7 methods) | Split by concern |
| DTO in Domain | Move to Application/DTOs |
| Logic in DTO | Remove; DTOs are data-only records |
| Direct IRepositoryBase in service | Inject IUnitOfWork |

## Testing Requirements

| Layer | Framework | Focus |
|-------|-----------|-------|
| Domain | xUnit | Entity behavior, invariants, value equality |
| Application | xUnit + Moq | Service orchestration, Result paths |
| API | xUnit + WebApplicationFactory | HTTP status codes, auth, validation |
| Frontend | Vitest + Testing Library | Component render, user interaction, API mocking |

- Every service method tested: happy path + error path
- Entities tested: all state transitions, guard clauses throw on invalid
- Controllers tested: all HTTP responses (200/201/400/404/500)
- Never test EF Core internals — test repository against test DB or mock

## Quick Reference

```bash
# Build & run
dotnet build ModernPaySystem.slnx
dotnet run --project ModernPaySystem
cd ModernPaySystem.Front && npm run dev

# Test
dotnet test --filter "FullyQualifiedName~Unit"
dotnet test --filter "FullyQualifiedName~Integration"

# Migrations
dotnet ef migrations add <Name> --project ModernPaySystem.Infrastructure.Persistence
dotnet ef database update --project ModernPaySystem.Infrastructure.Persistence

# Frontend
npm run lint
npm run build
```

# Self-Improvement: KB must stay in sync
# After adding any new entity/service/controller/library:
# 1. Update corresponding KB skill files
# 2. Update vault-structure.md if folders changed
# 3. Update dependency-rules.md if packages changed
# 4. Commit KB changes alongside code changes

# UnitOfWork: data access gateway
# ✅ unitOfWork.Requests.GetAsync(filter)
# ✅ unitOfWork.Users.GetByIdAsync(id)
# ❌ FORBIDDEN: injecting IRepositoryBase<T> directly in services
