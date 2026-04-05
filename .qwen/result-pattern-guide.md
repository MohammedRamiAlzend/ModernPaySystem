# Result Pattern Guide - ModernPaySystem

## Overview

The Result pattern is a functional approach to error handling that makes error states explicit through the type system. Instead of throwing exceptions, methods return `Result<T>` objects that explicitly encode success or failure.

## Core Components

### Result<T> Type
Located in: `ModernPaySystem.Domain.Commons.ResultOfT.cs`

```csharp
public sealed class Result<TValue> : IResult<TValue>
{
    public bool IsSuccess { get; }
    public bool IsError => !IsSuccess;
    public List<Error> Errors { get; }
    public TValue? Value { get; }
    public Error TopError { get; }
}
```

### Success Markers
```csharp
public readonly record struct Success(object? Data = null);
public readonly record struct Created(object? Data = null);
public readonly record struct Deleted;
public readonly record struct Updated(object? Data = null);
public readonly record struct Assigned;

public static class Result
{
    public static Success Success => default;
    public static Created Created => default;
    public static Deleted Deleted => default;
    public static Updated Updated => default;
    public static Assigned Assigned => default;
}
```

### Error Type
Located in: `ModernPaySystem.Domain.Commons.Error.cs`

```csharp
public readonly record struct Error
{
    public string Code { get; }
    public string Description { get; }
    public ErrorKind Type { get; }
    public string? ArabicDescription { get; }
    public HttpStatusCode HttpStatus { get; }
    
    // Factory methods
    public static Error Failure(...);
    public static Error Unexpected(...);
    public static Error Validation(...);
    public static Error Conflict(...);
    public static Error NotFound(...);
    public static Error Unauthorized(...);
    public static Error Forbidden(...);
}

public enum ErrorKind
{
    Failure,      // General failures (500)
    Unexpected,   // Unexpected errors (500)
    Validation,   // Validation errors (400)
    Conflict,     // Conflict errors (409)
    NotFound,     // Not found (404)
    Unauthorized, // Unauthorized (401)
    Forbidden,    // Forbidden (403)
}
```

### Extension Methods
Located in: `ModernPaySystem.Infrastructure.Extensions.ResultExtensions.cs`

### ⚠️ Important: Creating Error Instances

**Rule**: All Error instances must be defined in `ModernPaySystem.Domain/Commons/ApplicationErrors.cs`.

**DO NOT** create new Error instances outside of `ApplicationErrors.cs`.

#### ✅ Correct Approach: Use ApplicationErrors
```csharp
// Use predefined errors from ApplicationErrors.cs
return ApplicationErrors.UserNotFound;
return ApplicationErrors.InvalidEmailFormat;
return ApplicationErrors.EmailAlreadyExists;
return ApplicationErrors.RequestNotFound;
```

#### ❌ Incorrect Approach: Creating New Errors
```csharp
// ❌ NEVER do this outside ApplicationErrors.cs
return Error.NotFound("MyCustomError", "Something went wrong.");
return new Error("999", "Custom error", ErrorKind.NotFound);
```

#### Why This Rule?
1. **Consistency**: All errors are centralized in one place
2. **Unique Codes**: ApplicationErrors.cs maintains unique numeric codes (1-99, 100-199, etc.)
3. **Bilingual Support**: Predefined errors include Arabic descriptions
4. **Maintainability**: Easy to track and manage all application errors
5. **Reusability**: Same errors can be reused across multiple services

#### Error Code Ranges in ApplicationErrors.cs
- **1-99**: Authentication Errors
- **100-199**: Role Errors
- **200-299**: Permission Errors
- **300-399**: Template Errors
- **400-499**: Request Errors
- **500-599**: Response Errors
- **600-699**: Attachment Errors
- **700-799**: Validation Errors
- **800-899**: General Errors
- **900-999**: Transaction System Errors
- **1000-1099**: File Operation Errors

#### Adding New Errors
If you need a new error type, add it to `ApplicationErrors.cs` following the naming convention and code range:
```csharp
// In ApplicationErrors.cs
public static readonly Error MyNewError = new("XXX", "Error description.", ErrorKind.Validation, "وصف الخطأ بالعربية.");
```

```csharp
// Basic conversion
result.ToActionResult()

// With location header for Created responses
result.ToActionResult(locationUri: "/api/resource/123")

// With action name for Created responses
result.ToCreatedAtActionResult(controller, "GetById", new { id = 123 })
```

## Usage Workflows

### Workflow 1: Creating a Service Method

#### Step 1: Define the method signature
```csharp
public async Task<Result<UserDto>> GetUserByIdAsync(Guid id)
```

#### Step 2: Validate input
```csharp
if (id == Guid.Empty)
{
    return ApplicationErrors.InvalidInput;
}
```

#### Step 3: Execute business logic with error handling
```csharp
var user = await _userRepository.GetByIdAsync(id);

if (user == null)
{
    return ApplicationErrors.UserNotFound;
}
```

#### Step 4: Return success result
```csharp
var userDto = new UserDto(user.Id, user.Name, user.Email);
return userDto;

// OR for operations that don't return data:
return Result.Success;

// OR for create operations:
var createdDto = new UserDto(newUser.Id, newUser.Name, newUser.Email);
return Result.Created(createdDto);

// OR for update operations:
return Result.Updated(updatedDto);

// OR for delete operations:
return Result.Deleted;
```

#### Complete Example
```csharp
public async Task<Result<UserDto>> CreateUserAsync(CreateUserDto dto)
{
    // Validate input
    if (string.IsNullOrWhiteSpace(dto.Email))
    {
        return ApplicationErrors.InvalidEmailFormat;
    }

    // Check for conflicts
    var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
    if (existingUser != null)
    {
        return ApplicationErrors.UserAlreadyExists;
    }

    // Create user
    var user = new User(dto.Email, dto.Name);
    await _userRepository.AddAsync(user);
    await _unitOfWork.SaveChangesAsync();

    // Return success
    var userDto = new UserDto(user.Id, user.Name, user.Email);
    return Result.Created(userDto);
}
```

### Workflow 2: Creating a Controller Action

#### Step 1: Define the action
```csharp
[HttpPost]
public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
```

#### Step 2: Call service and handle Result
```csharp
var result = await _userService.CreateUserAsync(dto);
return result.ToActionResult();
```

#### Step 3: For Created responses, use location header
```csharp
[HttpPost]
public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
{
    var result = await _userService.CreateUserAsync(dto);
    return result.ToActionResult(locationUri: $"/api/users");
}
```

#### Step 4: Or use CreatedAtAction
```csharp
[HttpPost]
public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
{
    var result = await _userService.CreateUserAsync(dto);
    return result.ToCreatedAtActionResult(
        this, 
        nameof(GetUserById), 
        new { id = result.Value?.Id }
    );
}
```

#### Complete Controller Example
```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var result = await _userService.GetUserByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        var result = await _userService.CreateUserAsync(dto);
        return result.ToActionResult();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
    {
        var result = await _userService.UpdateUserAsync(id, dto);
        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var result = await _userService.DeleteUserAsync(id);
        return result.ToActionResult();
    }
}
```

### Workflow 3: Handling Multiple Validation Errors

```csharp
public async Task<Result<UserDto>> CreateUserAsync(CreateUserDto dto)
{
    var errors = new List<Error>();

    // Collect all validation errors
    if (string.IsNullOrWhiteSpace(dto.Name))
    {
        errors.Add(ApplicationErrors.MissingRequiredField);
    }

    if (string.IsNullOrWhiteSpace(dto.Email))
    {
        errors.Add(ApplicationErrors.InvalidEmailFormat);
    }

    // Return all errors at once
    if (errors.Count > 0)
    {
        return errors; // Implicit conversion
    }

    // Continue with creation...
    var user = new User(dto.Email, dto.Name);
    await _userRepository.AddAsync(user);

    return Result.Created(new UserDto(user.Id, user.Name, user.Email));
}
```

### Workflow 4: Service Layer with Repository Pattern

```csharp
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserDto>> UpdateUserAsync(Guid id, UpdateUserDto dto)
    {
        // 1. Validate input
        if (id == Guid.Empty)
        {
            return ApplicationErrors.InvalidInput;
        }

        // 2. Fetch entity
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return ApplicationErrors.UserNotFound;
        }

        // 3. Apply business rules
        if (user.Email != dto.Email)
        {
            var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return ApplicationErrors.UserAlreadyExists;
            }
        }

        // 4. Update entity
        user.Update(dto.Name, dto.Email);
        await _unitOfWork.SaveChangesAsync();

        // 5. Return success
        return Result.Updated(new UserDto(user.Id, user.Name, user.Email));
    }
}
```

## Error Type Guidelines

### When to Use Each ErrorKind

| ErrorKind | HTTP Status | Use Case | Example |
|-----------|-------------|----------|---------|
| `Validation` | 400 Bad Request | Invalid input, failed validation rules | Empty required field, invalid email format |
| `NotFound` | 404 Not Found | Entity doesn't exist | User not found, order not found |
| `Conflict` | 409 Conflict | Duplicate data, state conflicts | Email already exists, version conflict |
| `Unauthorized` | 401 Unauthorized | Authentication required/failed | Invalid token, expired token |
| `Forbidden` | 403 Forbidden | Insufficient permissions | User lacks required permission |
| `Failure` | 500 Internal Server Error | General failures | Unexpected business logic failure |
| `Unexpected` | 500 Internal Server Error | Unknown/unhandled errors | Infrastructure errors, external service failure |

## Common Patterns & Anti-Patterns

### ✅ Good Patterns

**1. Early returns for validation using ApplicationErrors**
```csharp
if (string.IsNullOrWhiteSpace(input))
{
    return ApplicationErrors.MissingRequiredField;
}
```

**2. Check IsError before proceeding**
```csharp
var result = await _service.DoSomethingAsync();
if (result.IsError)
{
    return result.ToActionResult();
}
// Continue with result.Value
```

**3. Use predefined errors from ApplicationErrors**
```csharp
return ApplicationErrors.UserNotFound; // Returns Result<T>
return Result.Success; // Returns Result<Success>
```

**4. Collect multiple validation errors**
```csharp
var errors = new List<Error>();
// ... add errors from ApplicationErrors
return errors; // Returns Result<T> with all errors
```

### ❌ Anti-Patterns

**1. Don't throw exceptions for expected failures**
```csharp
// ❌ BAD
if (user == null)
{
    throw new NotFoundException("User not found");
}

// ✅ GOOD
if (user == null)
{
    return ApplicationErrors.UserNotFound;
}
```

**2. Don't mix Result pattern with exceptions**
```csharp
// ❌ BAD
try
{
    var user = await _repository.GetByIdAsync(id);
    return user;
}
catch (Exception ex)
{
    return Error.Failure("Error", ex.Message);
}

// ✅ GOOD
var user = await _repository.GetByIdAsync(id);
if (user == null)
{
    return ApplicationErrors.UserNotFound;
}
return user;
```

**3. Don't forget to check IsError**
```csharp
// ❌ BAD - accessing Value without checking
var result = await _service.DoSomethingAsync();
var value = result.Value; // Could be null if error!

// ✅ GOOD - always check or convert
var result = await _service.DoSomethingAsync();
if (result.IsError)
{
    return result.ToActionResult();
}
var value = result.Value; // Safe to access
```

**4. Don't return generic errors when specific ones exist**
```csharp
// ❌ BAD
return Error.Failure("Error", "Something went wrong");

// ✅ GOOD
return ApplicationErrors.UserNotFound;
return ApplicationErrors.InvalidEmailFormat;
return ApplicationErrors.UserAlreadyExists;
```

**5. NEVER create Error instances outside ApplicationErrors.cs**
```csharp
// ❌ BAD - Creating new errors outside ApplicationErrors.cs
return Error.NotFound("MyCustomError", "Custom error message.");
return new Error("999", "Another custom error", ErrorKind.NotFound);
return Error.Validation("CustomValidation", "Custom validation message");

// ✅ GOOD - Use predefined errors from ApplicationErrors.cs
return ApplicationErrors.UserNotFound;
return ApplicationErrors.InvalidInput;
return ApplicationErrors.RequestAlreadyApproved;
```

## Testing Result Pattern

### Unit Test Example
```csharp
[Fact]
public async Task GetUserByIdAsync_UserNotFound_ReturnsError()
{
    // Arrange
    var userId = Guid.NewGuid();
    _userRepository.Setup(x => x.GetByIdAsync(userId))
        .ReturnsAsync((User?)null);

    // Act
    var result = await _userService.GetUserByIdAsync(userId);

    // Assert
    Assert.True(result.IsError);
    Assert.Equal(ApplicationErrors.UserNotFound.Type, result.TopError.Type);
    Assert.Equal(ApplicationErrors.UserNotFound.Code, result.TopError.Code);
}

[Fact]
public async Task CreateUserAsync_ValidUser_ReturnsCreated()
{
    // Arrange
    var dto = new CreateUserDto("test@test.com", "Test User");

    // Act
    var result = await _userService.CreateUserAsync(dto);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.IsType<Created>(result.Value);
}
```

## Quick Reference

### Service Method Template
```csharp
public async Task<Result<TDto>> MethodAsync(parameters)
{
    // 1. Validate input
    if (invalid)
    {
        return ApplicationErrors.InvalidInput; // or specific validation error
    }

    // 2. Check existence
    if (entity == null)
    {
        return ApplicationErrors.UserNotFound; // or appropriate NotFound error
    }

    // 3. Check conflicts
    if (conflict)
    {
        return ApplicationErrors.UserAlreadyExists; // or appropriate Conflict error
    }

    // 4. Execute operation
    await _repository.DoSomethingAsync();
    await _unitOfWork.SaveChangesAsync();

    // 5. Return appropriate success marker
    return Result.Success;        // For general operations
    return Result.Created(dto);   // For create operations
    return Result.Updated(dto);   // For update operations
    return Result.Deleted;        // For delete operations
}
```

### Controller Action Template
```csharp
[HttpMethod]
public async Task<IActionResult> ActionName(parameters)
{
    var result = await _service.MethodAsync(parameters);
    return result.ToActionResult();
}
```

## Files Reference

- **Result<T>**: `ModernPaySystem.Domain/Commons/ResultOfT.cs`
- **Error**: `ModernPaySystem.Domain/Commons/Error.cs`
- **ApplicationErrors**: `ModernPaySystem.Domain/Commons/ApplicationErrors.cs` ⚠️ **All errors must be defined here**
- **IResult**: `ModernPaySystem.Domain/Commons/IResult.cs`
- **IResult<T>**: `ModernPaySystem.Domain/Commons/IResultOfT.cs`
- **Extensions**: `ModernPaySystem.Infrastructure/Extensions/ResultExtensions.cs`
- **Example Usage**: `ModernPaySystem/Controllers/*.cs`
