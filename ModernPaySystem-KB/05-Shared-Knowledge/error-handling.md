# Error Handling — ModernPaySystem

## Result<T> Pattern

All service methods return `Result<T>` or `Result` from `ModernPaySystem.Domain.Commons`. **Never throw exceptions for expected failure cases.**

### Correct Pattern

```csharp
public async Task<Result<RequestDto>> GetByIdAsync(Guid id)
{
    var result = await unitOfWork.Requests.GetByIdAsync(id);
    if (result.IsError)
        return result.Errors;

    return result.Value!.ToDto();
}
```

### ApplicationErrors

Common errors are defined in `ModernPaySystem.Domain.Commons.ApplicationErrors`:

| Error | Usage |
|-------|-------|
| `ApplicationErrors.InvalidInput` | Validation failure |
| `ApplicationErrors.NotFound` | Entity not found |
| `ApplicationErrors.InternalServerError` | Unexpected server error |
| `ApplicationErrors.AttachmentNotFound` | Attachment-specific |
| `ApplicationErrors.ResponseNotFound` | Response-specific |

### Anti-Patterns

#### ❌ Raw Exception Throws

```csharp
// BAD — throws raw Exception for expected failures
throw new Exception("Error checking associations: " + string.Join(", ", errors));
```

✅ **Instead**, return the error via `Result<T>`:

```csharp
var result = await unitOfWork.Entities.GetAllAsync(filter);
if (result.IsError)
    return result.Errors;  // Propagate the error
```

#### ❌ Sync-over-Async (Deadlock Risk)

```csharp
// BAD — blocks on async call
var result = unitOfWork.Departments.GetByIdAsync(id).GetAwaiter().GetResult();
```

✅ **Instead**, make the method async:

```csharp
public async Task<Result<bool>> CheckAsync(Guid id)
{
    var result = await unitOfWork.Departments.GetByIdAsync(id);
    if (result.IsError)
        return result.Errors;

    return true;  // Implicit conversion: bool → Result<bool>
}
```

#### ❌ Swallowing Exceptions

```csharp
// BAD — hides errors
catch { return true; }
```

✅ **Instead**, log and return a safe default:

```csharp
catch (Exception ex)
{
    logger.LogError(ex, "Error checking parent assignment");
    return true;  // Implicit conversion: bool → Result<bool>
}
```
