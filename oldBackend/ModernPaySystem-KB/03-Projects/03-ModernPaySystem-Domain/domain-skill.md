# Domain Skill — ModernPaySystem.Domain

## Purpose

The Domain layer is the **root of the architecture**. It contains entities, value objects, DTOs, expression helpers, Result<T>, constants, and business rules. It has **zero external framework dependencies** except library projects (ExpressionBuilderLib) and `System.*` namespaces.

**Key difference from standard Clean Architecture:** DTOs live in Domain (not Application) because they are shared by all layers. The `Entity<TKey>` base uses `[Key]` for pragmatic reasons.

---

## Responsibility Matrix

| Concept | Responsibility | Location |
|---------|---------------|----------|
| **Entity** | Object with identity (`Id`), mutable state, behavior methods, expression helpers | `Domain/Entities/{Category}/{Entity}.cs` |
| **Base Entity** | `Entity<TKey>` with `[Key]` attribute, `Id` property | `Domain/Entities/Abstraction/Entity.cs` |
| **AuditableEntity** | Interface for Created/Updated audit fields | `Domain/Entities/Abstraction/IAuditableEntity.cs` |
| **Value Object** | Immutable object defined by attributes (e.g., `Email` could be a VO) | `Domain/Entities/{Category}/*.cs` |
| **Expression Helper** | Static `Expression<Func<T, bool>>` builders for dynamic queries | `Domain/Entities/{Category}/{Entity}Expressions.cs` |
| **DTO** | Data transfer object (request/response) | `Domain/DTOs/` |
| **Domain Exception** | Custom exception types | `Domain/Exceptions/` |
| **Result<T>** | Operation outcome wrapper | `Domain/Commons/ResultOfT.cs` |
| **Error** | Error code, message, kind | `Domain/Commons/Error.cs` |
| **PagedList<T>** | Pagination wrapper | `Domain/Commons/PagedList.cs` |
| **Guard** | Validation helpers | `Domain/Commons/` |
| **Constants** | App-wide constants | `Domain/Commons/Constants.cs` |
| **ApplicationErrors** | Centralized error messages | `Domain/Commons/ApplicationErrors.cs` |

---

## Folder Structure

```
ModernPaySystem.Domain/
├── Abstraction/
│   ├── Entity.cs                    # Base entity with [Key] attribute
│   ├── Entity.cs                    # Base entity with [Key] attribute
│   ├── IEntityDesc.cs
│   └── IAuditableEntity.cs          # Created/Updated audit interface
├── Commons/
│   ├── ResultOfT.cs                 # Result<T>, Success, Created, Deleted, Updated
│   ├── Error.cs                     # Error record with Code, Message, Kind
│   ├── PagedList.cs                 # Pagination wrapper
│   ├── ApplicationErrors.cs         # Centralized error instances
│   └── ...                          # Guard, Constants, etc.
├── DTOs/
│   ├── Auth/
│   │   └── AuthDtos.cs              # LoginRequest, etc.
│   └── {Category}/                  # e.g., TransactionSystem/, Archiving/
│       ├── {Entity}Dto.cs           # Response DTO
│       ├── Create{Entity}Dto.cs     # Create request
│       ├── Update{Entity}Dto.cs     # Update request
│       ├── {Entity}PagedFilterDto.cs # Pagination/filter DTO
│       └── ...                      # Related DTOs
├── Entities/
│   ├── Abstraction/                 # (already listed above)
│   ├── SharedEntities/
│   │   ├── User.cs
│   │   ├── UserExpressions.cs       # Dynamic expression helpers for User
│   │   ├── Role.cs
│   │   ├── RoleExpressions.cs
│   │   ├── Attachment.cs
│   │   ├── AttachmentExpressions.cs
│   │   ├── LookUpField.cs
│   │   ├── LookUpFiledValuesExpressions.cs
│   │   ├── Department.cs
│   │   ├── PermissionEntity.cs
│   │   └── SubSystemUser.cs
│   ├── TransactionSystemEntities/
│   │   ├── Request.cs
│   │   ├── RequestExpressions.cs
│   │   ├── RequestTemplateValues.cs
│   │   ├── RequestRelation.cs
│   │   ├── RequestAttachment.cs
│   │   ├── RequestTransaction.cs
│   │   ├── RequestTransactionExpressions.cs
│   │   ├── Response.cs
│   │   ├── ResponseExpressions.cs
│   │   ├── Template.cs
│   │   ├── TemplateExpressions.cs
│   │   ├── InputValue.cs
│   │   ├── UserTemplateOwnership.cs
│   │   └── TemplateOwnership.cs
│   ├── Archiving/
│   │   ├── ArchiveRecord.cs
│   │   ├── ArchiveRecordTemplateValues.cs
│   │   ├── ArchiveRecordFormInputValue.cs
│   │   ├── ArchiveFormTemplate.cs
│   │   ├── PhysicalFile.cs
│   │   ├── Folder.cs
│   │   ├── FolderPermission.cs
│   │   ├── ArchiveGovernance.cs      # Contains Delete/EditArchiveRequest, DepartmentArchiveLeader
│   │   └── AccessLevel.cs            # Enum
│   └── PaySystemEntities/
│       └── FastOperations/
│           ├── Operation.cs
│           ├── Client.cs
│           ├── Gov.cs                 # Enum
│           ├── Gender.cs              # Enum
│           ├── National.cs            # Enum
│           ├── KindShip.cs            # Enum
│           ├── OperationStatus.cs     # Enum
│           └── OperationServiceType.cs # Enum
├── Attributes/                      # Custom attributes (if any)
├── ModernPaySystem.Domain.csproj
└── (no Services/ — no domain service classes currently)
```

---

## Design Rules

### Rule 1 — Entity<T> Base Class with [Key]

```csharp
// ModernPaySystem.Domain/Entities/Abstraction/Entity.cs
using System.ComponentModel.DataAnnotations;

namespace ModernPaySystem.Domain.Entities.Abstraction;

public class Entity<TKey>
{
    [Key]  // EF Core attribute on Domain base — pragmatic exception
    public virtual TKey Id { get; set; }
}
```

**Key points:**
- All entities inherit from `Entity<Guid>` (or `Entity<int>`, etc.)
- The `[Key]` attribute is on the base class so every entity gets it automatically
- Properties use `public` setters (not private) because EF Core needs them for materialization
- Private parameterless constructor is NOT used (different from standard DDD)

### Rule 2 — IAuditableEntity Interface

```csharp
// ModernPaySystem.Domain/Entities/Abstraction/IAuditableEntity.cs
namespace ModernPaySystem.Domain.Entities.Abstraction;

public interface IAuditableEntity
{
    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

Entities that need audit tracking implement this interface. The `AuditInterceptor` in Persistence automatically populates these fields.

### Rule 3 — Expression Helpers for Dynamic Queries

Each entity may have a corresponding `*Expressions.cs` file with static expression builders:

```csharp
// ModernPaySystem.Domain/Entities/TransactionSystemEntities/RequestExpressions.cs
namespace ModernPaySystem.Domain.Entities.TransactionSystemEntities;

public static class RequestExpressions
{
    public static Expression<Func<Request, bool>> ByRequesterId(Guid requesterId) =>
        r => r.RequesterId == requesterId;

    public static Expression<Func<Request, bool>> ByStatus(RequestStatus status) =>
        r => r.Status == status;

    public static Expression<Func<Request, bool>> ByDateRange(DateTime? from, DateTime? to) =>
        r => (!from.HasValue || r.CreatedAt >= from.Value) &&
             (!to.HasValue || r.CreatedAt <= to.Value);
}
```

These expressions are consumed by `RepositoryBase<T>` via the `ExpressionCombiner` from ExpressionBuilderLib.

### Rule 4 — DTOs with ToDto() Methods

DTOs are records in `Domain/DTOs/`. Entities expose `ToDto()` methods:

```csharp
// Domain/Entities/TransactionSystemEntities/Request.cs (partial)
public class Request : Entity<Guid>, IAuditableEntity
{
    // ... entity properties ...

    public RequestDto ToDto()
    {
        return new RequestDto
        {
            Id = this.Id,
            RequestNumber = this.RequestNumber,
            RequesterId = this.RequesterId,
            // ... map properties ...
        };
    }
}
```

### Rule 5 — Business Logic on Entities

Entities encapsulate domain rules as methods:

```csharp
// Request.cs
public class Request : Entity<Guid>, IAuditableEntity
{
    public bool CanEdit(Guid userId)
    {
        if (userId == this.RequesterId) return true;
        if (userId == this.ApproverId) return false;
        if (ReadOnlyUsers?.Any(u => u.Id == userId) == true) return false;
        return false;
    }

    public bool CanView(Guid userId)
    {
        if (userId == this.RequesterId) return true;
        if (userId == this.ApproverId) return true;
        if (ReadOnlyUsers?.Any(u => u.Id == userId) == true) return true;
        return false;
    }
}
```

### Rule 6 — Result<T> for All Operations

```csharp
// Domain/Commons/ResultOfT.cs
public sealed class Result<TValue> : IResult<TValue>
{
    public bool IsSuccess { get; }
    public bool IsError => !IsSuccess;
    public List<Error> Errors => IsError ? _errors! : [];
    public TValue? Value => IsSuccess ? _value! : default;
    // ...
}

public static class Result
{
    public static Success Success => default;
    public static Created Created => default;
    public static Deleted Deleted => default;
    public static Updated Updated => default;
    // ...
}
```

Usage in services:
```csharp
var result = await _userRepository.GetByIdAsync(id);
if (result.IsError) return result.Errors;  // pass Error list through
return Result<UserDto>.Success(user.ToDto());
```

### Rule 7 — PagedList<T> for Paginated Results

```csharp
// Domain/Commons/PagedList.cs
public class PagedList<T>
{
    public List<T> Items { get; }
    public int TotalItems { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

    public static PagedList<T> Create(List<T> items, int totalItems, int page, int pageSize);
}
```

### Rule 8 — No Private Setters on Entity Properties

Unlike standard DDD, this solution uses public setters for EF Core materialization:

```csharp
public class Request : Entity<Guid>, IAuditableEntity
{
    public Guid RequesterId { get; set; }      // public set — EF Core needs this
    public User? Requester { get; set; }       // public set — navigation property
    public RequestStatus Status { get; set; }  // public set — enum property
    public ICollection<RequestAttachment> RequestAttachments { get; set; } = []
}
```

---

### Rule 9 — Every New Entity Must Be Registered in IUnitOfWork

After creating a new entity class in Domain, the developer MUST:

1. Add `DbSet<NewEntity>` to `AppDbContext` in Persistence
2. Add a new property to `IUnitOfWork`:
   ```csharp
   IRepositoryBase<NewEntity, Guid> NewEntities { get; }
   ```
3. Implement the property in `UnitOfWork.cs`

Only then can Infrastructure services access the entity via:
```csharp
await unitOfWork.NewEntities.GetByIdAsync(id);
```

This ensures a consistent data access path through `IUnitOfWork` for all entities.

---

## Entity Naming Conventions

| Convention | Example |
|-----------|---------|
| Entity class | `Request`, `User`, `ArchiveRecord` |
| Base class | `Entity<Guid>` |
| Expression file | `RequestExpressions.cs` |
| DTO (response) | `RequestDto` |
| DTO (create) | `CreateRequestDto` |
| DTO (update) | `UpdateRequestDto` |
| DTO (filter) | `RequestPagedFilterDto` |
| Modifier DTOs | `CreateRequestRelationDto`, `UpdateRequestRelationDto` |
| Enum | `RequestStatus` |
| UnitOfWork property | `unitOfWork.Requests`, `unitOfWork.ArchiveRecords` |

---

## Common Patterns

### Entity with Navigation Properties

```csharp
public class Request : Entity<Guid>, IAuditableEntity
{
    public required Guid RequesterId { get; set; }       // Foreign key
    public User? Requester { get; set; }                  // Navigation property

    public Guid? ResponseId { get; set; }                 // Optional FK
    public Response? Response { get; set; }               // Optional navigation

    public ICollection<RequestAttachment> RequestAttachments { get; set; } = [];  // Collection init
}
```

### Enum in Domain

```csharp
// Request.cs — in same file or separate
public enum RequestStatus
{
    Pending = 0,
    Delivered = 1,
    InProcess = 2,
    Managed = 3,
}
```

### File-Scoped Namespaces

All files use file-scoped namespaces:

```csharp
namespace ModernPaySystem.Domain.Entities.TransactionSystemEntities;

public class Request : Entity<Guid>, IAuditableEntity
{
    // ...
}
```

---

## AI Generation Rules

### When creating a new entity

```markdown
1. Place in appropriate subfolder under Domain/Entities/{Category}/
2. Inherit from `Entity<Guid>` (or appropriate key type)
3. Implement `IAuditableEntity` if the entity needs Created/Updated tracking
4. Use `public` setters for EF Core materialization
5. Use `required` keyword for non-nullable reference properties
6. Use `ICollection<T> = []` for collection initialization
7. Add static expression helpers in `{Entity}Expressions.cs` if dynamic queries are needed
8. Add corresponding DTOs in Domain/DTOs/{Category}/
9. Add `ToDto()` method on entity or create mapping extension
10. Use file-scoped namespace: `namespace ModernPaySystem.Domain.Entities.{Category};`
```

### When creating a new DTO

```markdown
1. Place in `Domain/DTOs/{Category}/{Entity}Dto.cs`
2. Use `record` or `record struct` type
3. All properties `{ get; init; }`
4. Separate DTOs: Create*, Update*, response {Entity}Dto, filter DTOs
5. No validation logic in DTOs
6. No methods beyond simple computed properties
7. Use file-scoped namespace: `namespace ModernPaySystem.Domain.DTOs.{Category};`
```

### When creating expression helpers

```markdown
1. Place in `Domain/Entities/{Category}/{Entity}Expressions.cs`
2. Static class with static Expression<Func<T, bool>> methods
3. Use for common filter patterns (ByStatus, ByUserId, ByDateRange)
4. Combine with ExpressionCombiner from ExpressionBuilderLib
5. Use file-scoped namespace matching the entity
```

### Domain checklist

```markdown
- [ ] Entity inherits from Entity<TKey>
- [ ] IAuditableEntity implemented if auditable
- [ ] All properties have public setters (for EF Core)
- [ ] required keyword used for non-null FKs
- [ ] Collection properties initialized with []
- [ ] Expression helpers in *Expressions.cs if dynamic queries needed
- [ ] DTOs in Domain/DTOs/ with correct namespace
- [ ] ToDto() method on entity or mapping extension
- [ ] File-scoped namespace used
- [ ] Entity registered in IUnitOfWork (IRepositoryBase<NewEntity, Guid> property)
- [ ] UnitOfWork.cs property implementation added
- [ ] No using for Infrastructure, Persistence, API, or ASP.NET (except System.ComponentModel.DataAnnotations for [Key] on base)
```