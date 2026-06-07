# ModernPaySystem Knowledge Base

```
ModernPaySystem-KB/
├── 00-Overview/
│   └── README.md                          # Quick orientation for new developers
├── 01-Architecture/
│   ├── clean-architecture.md              # Layer responsibilities, service location rules
│   └── dependency-rules.md                # Allowed/forbidden dependencies, import audit
├── 02-Code-Conventions/
│   ├── csharp-conventions.md              # C# patterns: file-scoped namespaces, primary constructors, naming
│   ├── entity-patterns.md                 # Entity<T>, private setters, expression helpers, ToDto()
│   ├── repository-pattern.md              # RepositoryBase<T, TKey>, IUnitOfWork, expression filtering
│   ├── service-patterns.md                # Infrastructure service patterns, DI, error handling
│   └── controller-patterns.md             # Thin controllers, EndpointPermission attribute, Result.ToActionResult()
├── 03-Projects/
│   ├── 01-ModernPaySystem/                # API layer
│   │   └── api-skill.md                   # Controllers, Program.cs, Scalar, auth, CORS
│   ├── 02-ModernPaySystem-Application/    # Contract-only layer
│   │   └── application-skill.md           # Interfaces, Repos, DTOs, specifications
│   ├── 03-ModernPaySystem-Domain/         # Root layer
│   │   └── domain-skill.md                # Entities, DTOs, Result<T>, Commons, Expression helpers
│   ├── 04-ModernPaySystem-Infrastructure/ # Service implementations
│   │   └── infrastructure-skill.md        # Services, Auth, OCR, FileManager, NumberSpelling, WebAttachment
│   ├── 05-ModernPaySystem-Infrastructure-Persistence/  # Data access
│   │   └── persistence-skill.md           # AppDbContext, RepositoryBase, UnitOfWork, Npgsql, Migrations, Seeding
│   ├── 06-ExpressionBuilderLib/
│   │   └── library-skill.md               # Dynamic expression tree building, ExpressionCombiner
│   ├── 07-FileManager/
│   │   └── library-skill.md               # File read/write, EnhancedFileManager, path utilities
│   ├── 08-NumberSpelling/
│   │   └── library-skill.md               # Number to Arabic words, INumberSpellingProvider
│   └── 09-OcrReader/
│       └── library-skill.md               # Tesseract OCR, image/text extraction, OcrService
├── 04-Agents/
│   ├── 00-Self-Improvement-Skill/
│   │   └── self-improvement.md            # KB auto-update rules, trigger conditions, per-project update rules
│   ├── 01-Backend-Agent/
│   │   └── agent-skill.md                 # Agent guidelines for backend work
│   └── 02-Frontend-Agent/
│       └── agent-skill.md                 # (if needed)
├── 05-Shared-Knowledge/
│   ├── authentication.md                  # JWT flow, PermissionAuthorizationHandler, EndpointPermission
│   ├── error-handling.md                  # Result<T> pattern, ApplicationErrors, ErrorKind
│   ├── pagination.md                      # PagedList<T>, GetPagedAsync, RequestPagedFilterDto
│   ├── soft-delete.md                     # Query filters, ISoftDeletable, interceptors
│   └── query-building.md                  # ExpressionCombiner, dynamic filters, AndAll/OrAll
├── 06-Roadmap/
│   └── roadmap.md
├── 07-Dependencies/
│   └── third-party-libs.md                # Npgsql, Tesseract, BCrypt, Serilog, Scalar, Bogus
├── 08-Templates/
│   ├── new-controller.md                  # Template for new API controller
│   ├── new-service.md                     # Template for new Infrastructure service
│   ├── new-repository.md                  # Template guidance for custom RepositoryBase usage
│   └── new-entity.md                      # Template for new Domain entity
└── 09-Decision-Log/
    └── decisions.md
```

## Key Facts

- **Self-Improvement**: Every code change triggers corresponding KB updates; new entities require IUnitOfWork registration updates across multiple KB files

| Aspect | Value |
|--------|-------|
| **Language** | C# 13 (.NET 10), TypeScript (React 19 frontend, ignored) |
| **Database** | PostgreSQL + Npgsql.EntityFrameworkCore.PostgreSQL 10.0 |
| **ORM** | EF Core 10 |
| **API Docs** | Scalar.AspNetCore (not Swashbuckle) |
| **Auth** | JWT + Permission-based authorization (custom attributes) |
| **Frontend** | ModernPaySystem.Front (React + Vite — out of scope for backend KB) |
| **Pattern** | Application = contracts only; Services in Infrastructure; Repositories via generic RepositoryBase |
| **DTOs Location** | `ModernPaySystem.Domain.DTOs` (shared across all layers) |
| **Result Pattern** | `ModernPaySystem.Domain.Commons.Result<T>` |
| **Libraries** | ExpressionBuilderLib (dynamic filtering), FileManager, NumberSpelling, OcrReader |
| **Test Projects** | None currently exist |
| **Self-Improvement** | Trigger-based KB updates when code changes occur |