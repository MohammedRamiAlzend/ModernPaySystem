# Smart Filter Centralization Pattern

## Overview
This document describes the pattern used to centralize filter expressions across the ModernPaySystem codebase. The pattern replaces inline `ExpressionBuilder` usage with static expression classes, enabling reusable, testable, and maintainable filter definitions.

## Architecture

### 1. Expression Classes
Static classes containing pre-built `Expression<Func<TEntity, bool>>` methods for common filters.

**Location:** `ModernPaySystem.Domain/Entities/[EntityFolder]/[EntityName]Expressions.cs`

**Example:**
```csharp
public static class RequestExpressions
{
    // Single expression filters
    public static Expression<Func<Request, bool>> ByRequesterId(Guid requesterId) =>
        r => r.RequesterId == requesterId;

    // Permission filters (database-level)
    public static Expression<Func<Request, bool>> CanReadByUserId(Guid userId) =>
        r => r.RequesterId == userId
             || r.ApproverId == userId
             || r.ReadOnlyUsers.Any(u => u.Id == userId);

    public static Expression<Func<Request, bool>> CanMakeUpdateByUserId(Guid userId) =>
        r => r.RequesterId == userId
             && r.ApproverId != userId
             && (r.ReadOnlyUsers == null || !r.ReadOnlyUsers.Any(u => u.Id == userId));

    // Combined filter lists
    public static List<Expression<Func<Request, bool>>> ByApproverIdAndResponse(Guid approverId, bool hasResponse) =>
    [
        ByApproverId(approverId),
        HasResponse(hasResponse)
    ];
}
```

### 2. Repository Interface
The `IRepositoryBase<TEntity, TKey>` interface supports an optional `additionalFilters` parameter:

```csharp
Task<Result<PagedList<TEntity>>> GetPagedAsync(
    int page,
    int pageSize,
    Expression<Func<TEntity, bool>>? filter = null,
    Func<IQueryable<TEntity>, IQueryable<TEntity>>? transform = null,
    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
    bool bypassAuth = false,
    List<Expression<Func<TEntity, bool>>>? additionalFilters = null);
```

### 3. Repository Implementation
The implementation combines all filters using `ExpressionCombiner.AndAll()`:

```csharp
var allFilters = new List<Expression<Func<TEntity, bool>>>();
if (filter != null) allFilters.Add(filter);
if (additionalFilters != null && additionalFilters.Count > 0)
    allFilters.AddRange(additionalFilters);

if (allFilters.Count > 0)
{
    var combinedFilter = ExpressionCombiner.AndAll(allFilters.ToArray());
    query = query.Where(combinedFilter);
}
```

## Usage Patterns

### Pattern 1: Simple Filter in Service
```csharp
// Before (inline ExpressionBuilder)
var filter = new ExpressionBuilder<Request>();
filter.And(r => r.RequesterId == requesterId);
var result = await unitOfWork.Requests.GetPagedAsync(page, pageSize, filter.Build());

// After (static expression)
var result = await unitOfWork.Requests.GetPagedAsync(
    page,
    pageSize,
    additionalFilters: RequestExpressions.ByRequesterId(requesterId).AsList());
```

### Pattern 2: Combined Filters
```csharp
var pagedRequests = await unitOfWork.Requests.GetPagedAsync(
    page,
    pageSize,
    transform: i => i.Include(x => x.RequestAttachments),
    additionalFilters: RequestExpressions.ByApproverIdAndResponse(currentUserId, hasResponse));
```

### Pattern 3: Permission-Based Filtering
```csharp
// GetAsync with permission filter
var request = await unitOfWork.Requests.GetAsync(
    filter: r => r.Id == id,
    transform: x => x.Include(x => x.Template)
                     .Include(x => x.Approver)
                     .Include(x => x.RequestAttachments),
    additionalFilters: new List<Expression<Func<Request, bool>>> 
    { 
        RequestExpressions.CanReadByUserId(currentUserId) 
    });

if (request.IsError) return request.Errors;
if (request.Value == null) return ApplicationErrors.UnauthorizedRequestAccess;
```

### Pattern 4: Edit Permission Check
```csharp
public async Task<Result<bool>> DeleteAsync(Guid id)
{
    var currentUserId = httpContextServiceManager.GetCurrentUserId();
    
    var entity = await unitOfWork.Requests.GetAsync(
        filter: r => r.Id == id,
        additionalFilters: new List<Expression<Func<Request, bool>>> 
        { 
            RequestExpressions.CanMakeUpdateByUserId(currentUserId) 
        });

    if (entity.Value == null) 
        return ApplicationErrors.UnauthorizedRequestAccess;
    
    // Proceed with deletion...
}
```

## Benefits

1. **Centralized Logic** - All filter expressions defined in one place, easy to find and modify
2. **Reusable** - Same expressions used across multiple service methods
3. **Testable** - Static methods can be unit tested independently
4. **Type-Safe** - Compile-time checking of expressions
5. **Database-Level Security** - Permission filters applied at query level, not in memory
6. **Composable** - Multiple filters can be combined using `ExpressionCombiner`
7. **Clean Services** - Service methods focus on business logic, not filter construction

## File Structure

```
ModernPaySystem.Domain/
└── Entities/
    └── TransactionSystemEntities/
        ├── Request.cs
        ├── RequestExpressions.cs      ← Static expression class
        ├── Response.cs
        ├── ResponseExpressions.cs     ← Static expression class
        └── ...

ModernPaySystem.Infrastructure/
└── Services/
    ├── RequestService.cs              ← Uses RequestExpressions
    └── ResponseService.cs             ← Uses ResponseExpressions
```

## ExpressionBuilderLib Reference

The pattern leverages the `ExpressionBuilderLib` library:

- `ExpressionCombiner.AndAll(expressions[])` - Combines all expressions with AND
- `ExpressionCombiner.OrAll(expressions[])` - Combines all expressions with OR
- `ExpressionBuilder<T>` - Fluent builder for complex expressions
- `ExpressionExtensions` - Extension methods like `.And()`, `.Or()`, `.Not()`

## Migration Checklist

When migrating an entity to this pattern:

- [ ] Create `[EntityName]Expressions.cs` in the entity's folder
- [ ] Add common filter expressions (ById, by foreign keys, etc.)
- [ ] Add permission filter expressions (`CanReadByUserId`, `CanMakeUpdateByUserId`)
- [ ] Add combined filter lists for frequent combinations
- [ ] Update service methods to use static expressions
- [ ] Replace `GetByIdAsync` with `GetAsync` + permission filters where needed
- [ ] Remove inline `ExpressionBuilder` usage in services
- [ ] Remove `CanView`/`CanEdit` methods from entity classes (move to expressions)
- [ ] Build and verify no errors
