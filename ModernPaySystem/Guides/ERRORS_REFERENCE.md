# ApplicationError Reference Guide

## Overview
The `ApplicationError` static class contains all predefined errors used throughout the ModernPaySystem application. Each error has:
- **Unique numeric code** (1-999+) for identification
- **ErrorKind** - Determines HTTP status code
- **English description**
- **Arabic description** (for bilingual support)

## Error Code Ranges

| Range | Category | HTTP Status |
|-------|----------|------------|
| 1-99 | Authentication | 401/403 |
| 100-199 | Roles | 400-404 |
| 200-299 | Permissions | 400-404 |
| 300-399 | Templates | 400-409 |
| 400-499 | Requests | 400-409 |
| 500-599 | Responses | 400-409 |
| 600-699 | Attachments | 400-603 |
| 700-799 | Validation | 400 |
| 800-899 | General | 500/409 |
| 900-999 | Transactions | 400-409 |

## Error Categories & Codes

### Authentication Errors (1-99)

| Code | Error | ErrorKind | HTTP Status |
|------|-------|-----------|------------|
| 1 | `InvalidCredentials` | Unauthorized | 401 |
| 3 | `TokenExpired` | Unauthorized | 401 |
| 4 | `InvalidToken` | Unauthorized | 401 |
| 5 | `InsufficientPermissions` | Forbidden | 403 |
| 6 | `UserNotFound` | NotFound | 404 |
| 7 | `UserAlreadyExists` | Conflict | 409 |
| 8 | `UserNotActive` | Forbidden | 403 |

### Role Errors (100-199)

| Code | Error | ErrorKind | HTTP Status |
|------|-------|-----------|------------|
| 100 | `RoleNotFound` | NotFound | 404 |
| 101 | `RoleAlreadyExists` | Conflict | 409 |
| 102 | `CannotDeleteDefaultRole` | Forbidden | 403 |
| 103 | `RoleNotAssignedToUser` | NotFound | 404 |

### Permission Errors (200-299)

| Code | Error | ErrorKind | HTTP Status |
|------|-------|-----------|------------|
| 200 | `PermissionNotFound` | NotFound | 404 |
| 201 | `PermissionAlreadyExists` | Conflict | 409 |
| 202 | `PermissionNotAssignedToRole` | NotFound | 404 |

### Template Errors (300-399)

| Code | Error | ErrorKind | HTTP Status |
|------|-------|-----------|------------|
| 300 | `TemplateNotFound` | NotFound | 404 |
| 301 | `TemplateAlreadyExists` | Conflict | 409 |
| 302 | `InvalidTemplateContent` | Validation | 400 |
| 303 | `TemplateInUse` | Conflict | 409 |
| 304 | `UnauthorizedTemplateAccess` | Forbidden | 403 |

### Request Errors (400-499)

| Code | Error | ErrorKind | HTTP Status |
|------|-------|-----------|------------|
| 400 | `RequestNotFound` | NotFound | 404 |
| 401 | `RequestAlreadyApproved` | Conflict | 409 |
| 402 | `RequestAlreadyRejected` | Conflict | 409 |
| 403 | `InvalidRequestStatus` | Validation | 400 |
| 404 | `CannotApproveOwnRequest` | Forbidden | 403 |
| 405 | `RequesterNotFound` | NotFound | 404 |
| 406 | `ApproverNotFound` | NotFound | 404 |
| 407 | `UnauthorizedRequestAccess` | Forbidden | 403 |

### Response Errors (500-599)

| Code | Error | ErrorKind | HTTP Status |
|------|-------|-----------|------------|
| 500 | `ResponseNotFound` | NotFound | 404 |
| 501 | `RequestAlreadyHasResponse` | Conflict | 409 |
| 502 | `CannotRespondToOwnRequest` | Forbidden | 403 |
| 503 | `InvalidResponseContent` | Validation | 400 |

### Attachment Errors (600-699)

| Code | Error | ErrorKind | HTTP Status |
|------|-------|-----------|------------|
| 600 | `AttachmentNotFound` | NotFound | 404 |
| 601 | `InvalidAttachmentType` | Validation | 400 |
| 602 | `AttachmentTooLarge` | Validation | 400 |
| 603 | `FailedToUploadAttachment` | Failure | 500 |
| 604 | `FailedToDeleteAttachment` | Failure | 500 |

### Validation Errors (700-799)

| Code | Error | ErrorKind | HTTP Status |
|------|-------|-----------|------------|
| 700 | `InvalidInput` | Validation | 400 |
| 701 | `MissingRequiredField` | Validation | 400 |
| 702 | `InvalidEmailFormat` | Validation | 400 |
| 703 | `InvalidPasswordLength` | Validation | 400 |
| 704 | `PasswordsDoNotMatch` | Validation | 400 |

### General Errors (800-899)

| Code | Error | ErrorKind | HTTP Status |
|------|-------|-----------|------------|
| 800 | `InternalServerError` | Failure | 500 |
| 801 | `DatabaseError` | Failure | 500 |
| 802 | `OperationFailed` | Failure | 500 |
| 803 | `NotImplemented` | Failure | 500 |
| 804 | `ConcurrencyError` | Conflict | 409 |

### Transaction System Errors (900-999)

| Code | Error | ErrorKind | HTTP Status |
|------|-------|-----------|------------|
| 900 | `TransactionNotFound` | NotFound | 404 |
| 901 | `InvalidTransactionAmount` | Validation | 400 |
| 902 | `InsufficientFunds` | Validation | 400 |
| 903 | `TransactionAlreadyProcessed` | Conflict | 409 |
| 904 | `DuplicateTransaction` | Conflict | 409 |

## Usage Examples

### In Services

```csharp
public async Task<Result<User>> GetUserAsync(Guid userId)
{
    var user = await _dbContext.Users.FindAsync(userId);
    
    if (user == null)
        return ApplicationError.UserNotFound;
    
    return user;
}

public async Task<Result<string>> AuthenticateAsync(string username, string password)
{
    var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == username);
    
    if (user == null)
        return ApplicationError.InvalidCredentials;
    
    if (!VerifyPassword(password, user.HashedPassword))
        return ApplicationError.InvalidCredentials;
    
    var accessToken = _tokenService.GenerateAccessToken(user, permissions);
    return accessToken;
}
```

### In Controllers

```csharp
[HttpPost]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    var result = await _authService.AuthenticateAsync(request.Username, request.Password);
    
    if (result.IsError)
        return result.ToActionResult();
    
    var accessToken = result.Value;
    return new OkObjectResult(new LoginResponse { AccessToken = accessToken });
}
```

## Error Response Format

All errors are returned in a consistent format:

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

Or for multiple errors:

```json
{
  "errors": [
    {
      "code": "701",
      "description": "A required field is missing.",
      "type": "Validation",
      "httpStatus": 400
    },
    {
      "code": "702",
      "description": "The email format is invalid.",
      "type": "Validation",
      "httpStatus": 400
    }
  ]
}
```

## Adding New Errors

When adding new errors:

1. **Choose appropriate code range** based on category
2. **Pick ErrorKind** that matches the error type
3. **Add both English and Arabic descriptions**
4. **Add to ApplicationError static class**

Example:
```csharp
// Payment Errors (1000-1099)
public static readonly Error PaymentFailed = 
    new("1000", "Payment processing failed.", ErrorKind.Failure, "فشل معالجة الدفع.");

public static readonly Error PaymentMethodNotSupported = 
    new("1001", "The payment method is not supported.", ErrorKind.Validation, "طريقة الدفع غير مدعومة.");
```

## ErrorKind to HTTP Status Mapping

| ErrorKind | HTTP Status |
|-----------|------------|
| Unauthorized | 401 |
| Forbidden | 403 |
| NotFound | 404 |
| Conflict | 409 |
| Validation | 400 |
| Failure | 500 |
| Unexpected | 500 |

## Best Practices

1. ✅ Always use `ApplicationError` constants for consistency
2. ✅ Use appropriate error codes for categorization
3. ✅ Include both English and Arabic descriptions
4. ✅ Return errors from services/repositories
5. ✅ Let controllers map to HTTP responses automatically
6. ✅ Document error codes in error documentation
7. ❌ Don't create new Error instances inline
8. ❌ Don't mix different error handling approaches
