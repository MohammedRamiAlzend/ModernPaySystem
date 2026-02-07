# JWT Permissions-Based Authentication Implementation

## Overview
Your ModernPaySystem project now has a complete JWT-based permissions authentication system with role-based access control (RBAC) using the Result pattern for error handling.

## Components Created

### 1. **Domain Entities**
- `Role.cs` - Represents user roles
- `PermissionEntity.cs` - Represents permissions
- `UserRole.cs` - Junction entity for User-Role relationship
- `RolePermission.cs` - Junction entity for Role-Permission relationship

**Updated Entities:**
- `User.cs` - Added `UserRoles` navigation property (RefreshToken removed)

### 2. **Infrastructure Services**

#### `JwtTokenService.cs`
- Generates JWT access tokens with permissions as claims
- Validates expired tokens

#### `AuthenticationService.cs` (Returns Result Pattern)
- Authenticates users by username/password - Returns `Result<string>` (AccessToken only)
- Hashes passwords using SHA256
- Retrieves user permissions from roles
- Methods:
  - `AuthenticateAsync()` - Login - Returns Result with access token
  - `GetUserPermissionsAsync()` - Get user permissions - Returns Result<List<string>>
  - `HashPassword()` - Secure password hashing
  - `VerifyPassword()` - Verify password against hash

### 3. **Authorization**

#### `PermissionAuthorizationHandler.cs`
- Custom authorization handler for permission-based policies
- Checks permission claims in JWT tokens
- Integrates with ASP.NET Core authorization

### 4. **API Endpoints**

#### `AuthController.cs` - `/api/auth`
Single endpoint for authentication:

- `POST /api/auth/login` - Login and get access token
  - Returns 200 with access token on success
  - Returns 401 with error details on invalid credentials

### 5. **DTOs**

#### `AuthDtos.cs`
- `LoginRequest` - Username and password
- `LoginResponse` - Access token only

### 6. **Database Configuration**

#### `AppDbContext.cs` Updates
- Added `DbSet<Role>` 
- Added `DbSet<PermissionEntity>`
- Added `DbSet<UserRole>` with composite key
- Added `DbSet<RolePermission>` with composite key
- Configured all relationships and delete behaviors

### 7. **Configuration**

#### `appsettings.json`
```json
"JwtSettings": {
  "SecretKey": "your-super-secret-key-that-is-at-least-32-characters-long-change-this-in-production",
  "Issuer": "ModernPaySystem",
  "Audience": "ModernPaySystemUsers",
  "ExpirationMinutes": 15
}
```

#### `Program.cs` Updates
- Configured JWT authentication with Bearer scheme
- Added DbContext with SQL Server
- Registered all services (scoped)
- Created authorization policies for Transaction System permissions:
  - `TransactionSystem.ViewTransactions`
  - `TransactionSystem.CreateTransaction`
  - `TransactionSystem.UpdateTransaction`
  - `TransactionSystem.DeleteTransaction`
- Added authentication and authorization middleware

## Result Pattern Integration

### Error Handling with Result<T>

The service layer returns `Result<T>` which encapsulates both success and failure states:

```csharp
// Success case - implicit conversion
return accessToken; // Returns Result with value

// Error cases
return ApplicationError.InvalidCredentials;
return ApplicationError.UserNotFound;
```

### Controller Response Handling

Controllers automatically map Result errors to appropriate HTTP status codes:

```csharp
var result = await _authService.AuthenticateAsync(username, password);

if (result.IsError)
    return result.ToActionResult();

var accessToken = result.Value;
return new OkObjectResult(new LoginResponse { AccessToken = accessToken });
```

### Error Types Available

- `Error.Failure()` - Generic failure (500)
- `Error.Unexpected()` - Unexpected error (500)
- `Error.Validation()` - Validation error (400)
- `Error.Conflict()` - Conflict error (409)
- `Error.NotFound()` - Not found error (404)
- `Error.Unauthorized()` - Unauthorized error (401)
- `Error.Forbidden()` - Forbidden error (403)

## How to Use

### 1. **Protect Endpoints**

Add the `[HasPermission]` attribute:

```csharp
[ApiController]
[Route("api/[controller]")]
public class TransactionController : ControllerBase
{
    [HasPermission(Permission.TransactionSystem.ViewTransactions)]
    [HttpGet]
    public async Task<IActionResult> GetTransactions()
    {
        // Your code here
    }

    [HasPermission(Permission.TransactionSystem.CreateTransaction)]
    [HttpPost]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
    {
        // Your code here
    }
}
```

### 2. **Login Flow**

```
POST /api/auth/login
{
  "username": "user@example.com",
  "password": "password123"
}

Success Response (200):
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs..."
}

Error Response (401):
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

### 3. **Use Access Token**

Include in request headers:
```
Authorization: Bearer {accessToken}
```

## Database Migration

You'll need to create and apply migrations:

```bash
dotnet ef migrations add AddAuthenticationEntities
dotnet ef database update
```

## Security Considerations

1. **Change the JWT Secret Key** in `appsettings.json` for production
2. **Use environment variables** for sensitive configuration
3. **Implement HTTPS** in production
4. **Set appropriate token expiration times**
5. **Use stronger hashing** - Consider bcrypt instead of SHA256 for password hashing

## Next Steps

1. Create database migration: `dotnet ef migrations add AddAuthenticationEntities`
2. Apply migration: `dotnet ef database update`
3. Seed initial roles and permissions
4. Seed test users with hashed passwords
5. Apply the `[HasPermission]` attribute to your endpoints
6. Adjust JWT secret and other settings per environment
