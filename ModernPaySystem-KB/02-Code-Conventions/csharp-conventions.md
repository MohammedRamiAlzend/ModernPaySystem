# C# Conventions — ModernPaySystem

## File-Scoped Namespaces

```csharp
namespace ModernPaySystem.Infrastructure.Services;
```

## Primary Constructors for DI

```csharp
public class RequestService(
    IUnitOfWork unitOfWork,
    ILogger<RequestService> logger) : IRequestService
```

## Private Fields

Prefix with `_` (only used when not using primary constructors):

```csharp
private readonly IUnitOfWork _unitOfWork;
```

## Async Method Naming

Suffix with `Async`:

```csharp
Task<Result<RequestDto>> GetByIdAsync(Guid id);
```

## .editorconfig

An `.editorconfig` file should be placed at the solution root to enforce:

```ini
[*.cs]
indent_style = space
indent_size = 4
csharp_style_namespace_declarations = file_scoped:silent
csharp_prefer_simple_using_statement = true:suggestion
csharp_style_prefer_primary_constructors = true:suggestion
```

### Running dotnet format

```bash
dotnet format ModernPaySystem.slnx
dotnet format ModernPaySystem.slnx --verify-no-changes  # CI check
```

## Naming

| Element | Convention | Example |
|---------|-----------|---------|
| Classes | PascalCase | `RequestService` |
| Interfaces | IPascalCase | `IRequestService` |
| Methods | PascalCase | `GetByIdAsync` |
| Properties | PascalCase | `PageSize` |
| Parameters | camelCase | `filterDto` |
| Private fields | _camelCase | `_unitOfWork` |
