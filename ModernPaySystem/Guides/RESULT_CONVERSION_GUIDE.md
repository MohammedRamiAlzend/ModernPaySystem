# Result<T> to IActionResult Conversion Guide

## Overview
The `Result<T>` type now has convenient extension methods to automatically convert to appropriate `IActionResult` responses. This eliminates boilerplate error handling in controllers.

## Files Added

### 1. `SuccessResponses.cs`
Marker types for different success scenarios:
- `Created` - Returns 201 Created
- `Deleted` - Returns 204 No Content
- `Updated` - Returns 200 OK
- `Success` - Returns 200 OK

### 2. `ResultExtensions.cs`
Extension methods for converting `Result<T>` to `IActionResult`:
- `ToActionResult()` - Basic conversion
- `ToActionResult(locationUri)` - With custom location header
- `ToCreatedAtActionResult()` - With action name and route values

## Automatic Error to HTTP Status Mapping

| ErrorKind | HTTP Status | Result Class |
|-----------|-------------|--------------|
| NotFound | 404 | NotFoundObjectResult |
| Unauthorized | 401 | UnauthorizedObjectResult |
| Forbidden | 403 | ForbiddenObjectResult |
| Conflict | 409 | ConflictObjectResult |
| Validation | 400 | BadRequestObjectResult |
| Failure/Other | 400 | BadRequestObjectResult |

## Success Response Types

### Created (201)
```csharp
public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
{
    var result = await _userService.CreateUserAsync(request);
    
    return result.ToActionResult() ?? new BadRequestObjectResult(result.Errors);
}
```
Returns:
```json
{
  "data": {
    "id": "123",
    "username": "john.doe",
    ...
  }
}
```

### Deleted (204)
```csharp
public async Task<IActionResult> DeleteUser(Guid id)
{
    var result = await _userService.DeleteUserAsync(id);
    
    return result.ToActionResult();
}
```
Returns: Empty response with 204 status

### Updated (200)
```csharp
public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
{
    var result = await _userService.UpdateUserAsync(id, request);
    
    return result.ToActionResult();
}
```

### Success (200)
```csharp
public async Task<IActionResult> GetUser(Guid id)
{
    var result = await _userService.GetUserAsync(id);
    
    return result.ToActionResult();
}
```

## Usage Examples

### Basic Usage
```csharp
[HttpPost]
public async Task<IActionResult> CreateTemplate([FromBody] CreateTemplateRequest request)
{
    var result = await _templateService.CreateTemplateAsync(request);
    return result.ToActionResult();
}
```

### With Custom Location Header
```csharp
[HttpPost]
public async Task<IActionResult> CreateRequest([FromBody] CreateRequestRequest request)
{
    var result = await _requestService.CreateRequestAsync(request);
    return result.ToActionResult($"/api/requests/{result.Value.Id}");
}
```

### With CreatedAtAction
```csharp
[HttpPost]
public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
{
    var result = await _roleService.CreateRoleAsync(request);
    
    if (result.IsError)
        return result.ToActionResult();
    
    return result.ToCreatedAtActionResult(this, nameof(GetRole), new { id = result.Value.Id });
}

[HttpGet("{id}")]
public async Task<IActionResult> GetRole(Guid id)
{
    var result = await _roleService.GetRoleAsync(id);
    return result.ToActionResult();
}
```

## Service Layer Pattern

### Returning Success Responses

```csharp
public class UserService
{
    public async Task<Result<Created>> CreateUserAsync(CreateUserRequest request)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(request.Username))
            return ApplicationError.MissingRequiredField;
        
        // Check for duplicates
        var exists = await _dbContext.Users
            .AnyAsync(u => u.UserName == request.Username);
        
        if (exists)
            return ApplicationError.UserAlreadyExists;
        
        // Create user
        var user = new User 
        { 
            UserName = request.Username,
            HashedPassword = _authService.HashPassword(request.Password)
        };
        
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        
        // Return success with data
        return new Created(new { user.Id, user.UserName });
    }

    public async Task<Result<Updated>> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        var user = await _dbContext.Users.FindAsync(id);
        
        if (user == null)
            return ApplicationError.UserNotFound;
        
        user.UserName = request.Username;
        await _dbContext.SaveChangesAsync();
        
        return new Updated(new { user.Id, user.UserName });
    }

    public async Task<Result<Deleted>> DeleteUserAsync(Guid id)
    {
        var user = await _dbContext.Users.FindAsync(id);
        
        if (user == null)
            return ApplicationError.UserNotFound;
        
        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();
        
        return new Deleted();
    }

    public async Task<Result<Success>> GetUserAsync(Guid id)
    {
        var user = await _dbContext.Users.FindAsync(id);
        
        if (user == null)
            return ApplicationError.UserNotFound;
        
        return new Success(user);
    }
}
```

## Controller Implementation

### Complete Example

```csharp
[ApiController]
[Route("api/[controller]")]
[HasPermission(Permission.TransactionSystem.ViewTransactions)]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var result = await _userService.GetUserAsync(id);
        return result.ToActionResult();
    }

    [HttpPost]
    [HasPermission(Permission.TransactionSystem.CreateTransaction)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var result = await _userService.CreateUserAsync(request);
        return result.ToCreatedAtActionResult(this, nameof(GetUser), new { id = result.Value.Id });
    }

    [HttpPut("{id}")]
    [HasPermission(Permission.TransactionSystem.UpdateTransaction)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        var result = await _userService.UpdateUserAsync(id, request);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    [HasPermission(Permission.TransactionSystem.DeleteTransaction)]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var result = await _userService.DeleteUserAsync(id);
        return result.ToActionResult();
    }
}
```

## Response Examples

### Successful Creation (201)
```json
{
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "username": "john.doe"
  }
}
```

### Successful Deletion (204)
```
[Empty body with 204 status]
```

### Successful Update (200)
```json
{
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "username": "john.updated"
  }
}
```

### Validation Error (400)
```json
{
  "errors": [
    {
      "code": "701",
      "description": "A required field is missing.",
      "type": "Validation",
      "httpStatus": 400
    }
  ]
}
```

### Not Found Error (404)
```json
{
  "errors": [
    {
      "code": "6",
      "description": "User with the specified ID was not found.",
      "type": "NotFound",
      "httpStatus": 404
    }
  ]
}
```

### Unauthorized Error (401)
```json
{
  "errors": [
    {
      "code": "1",
      "description": "Username or password is incorrect.",
      "type": "Unauthorized",
      "httpStatus": 401
    }
  ]
}
```

## Benefits

✅ **Eliminates boilerplate** - No more manual error handling in controllers  
✅ **Consistent responses** - All responses follow the same format  
✅ **Type-safe** - Compiler catches issues at compile-time  
✅ **Automatic status codes** - Based on ErrorKind  
✅ **Cleaner code** - Controllers are now focused on logic, not error mapping  
✅ **Easy to extend** - Add new success types or error handlers as needed  

## Best Practices

1. ✅ Always return a `Result<T>` from service methods
2. ✅ Use appropriate `Success*` types (Created, Deleted, Updated, Success)
3. ✅ Use `ApplicationError` constants for all errors
4. ✅ Call `ToActionResult()` in controllers
5. ✅ Keep business logic in services, HTTP concerns in controllers
6. ❌ Don't throw exceptions for business logic
7. ❌ Don't mix Result pattern with exception handling
