# AGENTS.md - Agentic Coding Guidelines

ModernPaySystem is a payment processing system with ASP.NET Core 10 Web API (Clean Architecture), React 19 + Vite + TypeScript + Tailwind CSS, SQL Server with EF Core 10, and JWT authentication.

## Architecture

```
ModernPaySystem/                     # API Controllers
ModernPaySystem.Application/         # Use Cases, Interfaces, DTOs
ModernPaySystem.Domain/              # Entities, Domain Logic
ModernPaySystem.Infrastructure/      # Auth, Cross-cutting
ModernPaySystem.Infrastructure.Persistence/  # EF Core, Repositories
ModernPaySystem.Front/              # React Frontend
```

## Build/Lint/Test Commands

### Frontend
```bash
cd ModernPaySystem.Front
npm install
npm run dev        # Development server
npm run build      # Production build
npm run lint       # ESLint
npm run preview    # Preview build
```

### Backend
```bash
dotnet restore ModernPaySystem.slnx
dotnet build ModernPaySystem.slnx
dotnet run --project ModernPaySystem/ModernPaySystem.csproj
```

### Database Migrations
```bash
dotnet ef migrations add <Name> --project ModernPaySystem.Infrastructure.Persistence
dotnet ef database update --project ModernPaySystem.Infrastructure.Persistence
```

### Running Tests
```bash
dotnet test                                    # All tests
dotnet test <path>                             # Specific project
dotnet test --filter "FullyQualifiedName~Namespace.ClassName.MethodName"  # Single test
dotnet test --verbosity normal                # Verbose output
```

## C# Backend Conventions

### File Structure
- File-scoped namespaces: `namespace ModernPaySystem.Controllers;`
- Order: using → namespace → class → fields → constructor → public methods → private methods

### Naming Conventions
- Classes/Interfaces: `PascalCase` (e.g., `IUserService`, `UserController`)
- Methods/Properties: `PascalCase` (e.g., `GetByIdAsync`)
- Private fields: `_camelCase` (e.g., `_userService`)
- Parameters: `camelCase`
- Interfaces: Prefix with `I`

### Type Usage
- Enable nullable: `<Nullable>enable</Nullable>`, use `string?`
- Use `Task<T>` for async (always async, never blocking)
- Use `Result<T>` wrapper pattern from `ModernPaySystem.Domain.Commons`

### Error Handling
- Use `Result<T>` for operation outcomes
- Pattern: `var x = await GetByIdAsync(id); if (x.IsError) return x.ToActionResult();`
- Use typed results: `Result.Success`, `Result.Created`, `Result.Updated`, `Result.Deleted`
- Avoid exposing internal exceptions; log and return generic errors
- Use `IActionResult` with proper HTTP status codes

### Async/Await
- Always `async Task` for I/O
- Never `.Result` or `.Wait()` - always await
- Use `ConfigureAwait(false)` for library code

### Dependency Injection
- Constructor injection: `public AuthController(IAuthenticationService authService)`
- Register services in `Program.cs` or extension methods

## TypeScript/React Conventions

### File Structure
```
src/
├── components/    # Reusable UI
├── features/     # Feature modules
├── hooks/        # Custom hooks
├── services/     # API services
├── store/        # Redux
├── types/        # TypeScript types
└── utils/        # Utilities
```

### Naming Conventions
- Components: `PascalCase` (e.g., `UserProfile.tsx`)
- Hooks: `camelCase` starting with `use` (e.g., `useAuth.ts`)
- Services: `camelCase` (e.g., `authService.ts`)
- Types: `PascalCase` (e.g., `UserDto.ts`)

### Imports (Order)
1. External libraries (React, Redux, TanStack Query)
2. Internal absolute imports (`@/`)
3. Relative imports
4. Type imports (`import type`)

```typescript
import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Button } from '@/components/ui/button';
import type { UserDto } from '@/types/user';
```

### TypeScript
- Explicit types for props, parameters, return values
- Avoid `any` when possible
- Use strict mode: `"strict": true` in tsconfig

### React Patterns
- Functional components with hooks
- TanStack Query for server state
- Redux Toolkit for global client state
- Zod + react-hook-form for validation

### Tailwind CSS
- Use shadcn/ui components from `@/components/ui/`
- Use `cn()` utility from `tailwind-merge` for conditional classes

## API Conventions

- RESTful: `/api/[controller]`
- Use `[ApiController]`, `[Route("api/[controller]")]`
- HTTP methods: `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]`
- DTOs for request bodies: `[FromBody] CreateUserDto dto`
- Return `IActionResult`: `Ok()`, `NotFound()`, `BadRequest()`

## Database & EF Core

- Use `Guid` for primary keys
- Inherit from base entities in `Domain/Entities/Abstraction`
- Configure relationships in `AppDbContext`

## Security Guidelines

- Never commit secrets - use environment variables
- Use parameterized queries - never concatenate SQL
- Validate all input on both client and server
- Hash passwords with `IPasswordHasher` service
- Use JWT with short expiration times

## Useful Paths

- API: `ModernPaySystem/`
- Frontend: `ModernPaySystem.Front/`
- DbContext: `ModernPaySystem.Infrastructure.Persistence/AppDbContext.cs`
- Entry: `ModernPaySystem/Program.cs`
