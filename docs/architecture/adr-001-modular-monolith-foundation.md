# ADR-001: Modular Monolith Foundation (Backend Only)

## Status
Accepted

## Context
The backend currently has cross-layer coupling and limited module boundaries. We need a migration-safe foundation toward a modular monolith while keeping current behavior stable.

## Decision
- Introduce backend-only modular folder layout under `src/`:
  - `src/BuildingBlocks/*`
  - `src/Modules/Transactions/*`
  - `src/Modules/Diwan/*`
- Keep existing runtime behavior untouched while adding architecture scaffolding.
- Add architecture tests that enforce boundaries for the new modular projects.
- Standardize module registration entrypoints via `Add{Module}Module` extension methods.
- Keep frontend (`ModernPaySystem.Front`) out of scope.

## Dependency Rules
### Allowed
- `Module.Application -> Module.Domain`
- `Module.Infrastructure -> Module.Application + Module.Domain`
- `Host -> Module.Infrastructure` only through composition registration
- `Module.* -> BuildingBlocks.*` as needed

### Forbidden
- `Module.Domain -> ASP.NET Core`
- `Module.Domain -> EF Core`
- `Module.Application -> OtherModule.Infrastructure`
- `Module.Contracts -> Infrastructure`

## Consequences
- Provides immediate guardrails for new code.
- Enables phased migration of existing features without large-bang rewrite.
- Existing legacy structure remains until later migration phases.
