# Dependency Injection Setup Guide

## Overview
The project uses a clean dependency injection (DI) architecture with separate extension methods for each layer. This approach keeps Program.cs clean and maintainable.

## DI Extension Methods

### 1. **PersistenceServiceRegistration** (`Infrastructure.Persistence`)
Registers all database-related services.

```csharp
public static IServiceCollection AddPersistenceServices(
    this IServiceCollection services,
    IConfiguration configuration)
```

**Services Registered:**
- `AppDbContext` - Entity Framework DbContext with retry policy

**Configuration Options:**
- Connection string: `DefaultConnection`
- Retry policy: 3 attempts, 30-second delay

**Usage:**
```csharp
builder.Services.AddPersistenceServices(builder.Configuration);
```

---

### 2. **AuthenticationServiceRegistration** (`Infrastructure.Auth`)
Registers JWT authentication and token validation.

```csharp
public static IServiceCollection AddJwtAuthentication(
    this IServiceCollection services,
    IConfiguration configuration)
```

**Services Registered:**
- JWT Bearer Authentication scheme
- Token validation parameters

**Configuration Required (appsettings.json):**
```json
"JwtSettings": {
  "SecretKey": "your-secret-key-here",
  "Issuer": "ModernPaySystem",
  "Audience": "ModernPaySystemUsers",
  "ExpirationMinutes": 15
}
```

**Usage:**
```csharp
builder.Services.AddJwtAuthentication(builder.Configuration);
```

---

### 3. **InfrastructureServiceRegistration** (`Infrastructure`)
Registers all infrastructure services and authorization policies.

```csharp
public static IServiceCollection AddInfrastructureServices(
    this IServiceCollection services)

public static IServiceCollection AddAuthorizationPolicies(
    this IServiceCollection services)
```

**Services Registered:**
- `IUnitOfWork` → `UnitOfWork` (Scoped)
- `ITokenService` → `JwtTokenService` (Scoped)
- `IAuthenticationService` → `AuthenticationService` (Scoped)
- `IAuthorizationHandler` → `PermissionAuthorizationHandler` (Scoped)

**Authorization Policies Created:**
- `Permission.TransactionSystem.ViewTransactions`
- `Permission.TransactionSystem.CreateTransaction`
- `Permission.TransactionSystem.UpdateTransaction`
- `Permission.TransactionSystem.DeleteTransaction`

**Usage:**
```csharp
builder.Services.AddInfrastructureServices();
builder.Services.AddAuthorizationPolicies();
```

---

## Program.cs Structure

```csharp
var builder = WebApplication.CreateBuilder(args);

// 1. Database & Persistence
builder.Services.AddPersistenceServices(builder.Configuration);

// 2. Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// 3. Authorization & Infrastructure
builder.Services.AddAuthorizationPolicies();
builder.Services.AddInfrastructureServices();

// 4. API Services
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// ... middleware configuration ...
```

---

## Service Lifetimes

| Service | Lifetime | Reason |
|---------|----------|--------|
| `AppDbContext` | Scoped | One per HTTP request |
| `IUnitOfWork` | Scoped | Coordinates multiple repositories per request |
| `ITokenService` | Scoped | Stateless token generation |
| `IAuthenticationService` | Scoped | Per-request user authentication |
| `IAuthorizationHandler` | Scoped | Per-request authorization checks |

---

## Adding New DI Extensions

Follow this pattern when adding new services:

### Step 1: Create Extension File
```csharp
namespace ModernPaySystem.Infrastructure.MyFeature;

public static class MyFeatureServiceRegistration
{
    public static IServiceCollection AddMyFeatureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register services here
        services.AddScoped<IMyService, MyService>();
        return services;
    }
}
```

### Step 2: Update Program.cs
```csharp
builder.Services.AddMyFeatureServices(builder.Configuration);
```

---

## Configuration Pattern

All DI extensions follow this pattern:

1. **Take IServiceCollection as first parameter** (for extension method)
2. **Accept IConfiguration if needed** (for settings)
3. **Register services with appropriate lifetimes**
4. **Return IServiceCollection** (for method chaining)
5. **Add XML documentation** (for IntelliSense)

---

## Testing with DI

Services can be easily mocked for testing:

```csharp
[Fact]
public async Task Login_WithValidCredentials_ReturnsToken()
{
    // Arrange
    var mockUnitOfWork = new Mock<IUnitOfWork>();
    var mockTokenService = new Mock<ITokenService>();
    
    var authService = new AuthenticationService(mockUnitOfWork.Object, mockTokenService.Object);
    
    // Act & Assert
    // ... test implementation ...
}
```

---

## Troubleshooting

### Service Not Found
If you get "Unable to resolve service" error:
1. Check if service is registered in DI extension
2. Verify the extension method is called in Program.cs
3. Check the namespace is imported

### Configuration Not Loading
If JWT settings are not being read:
1. Verify `appsettings.json` has `JwtSettings` section
2. Check connection string name matches `DefaultConnection`
3. Ensure configuration is passed to extension method

---

## Best Practices

✅ **Do:**
- Use extension methods for clean Program.cs
- Group related services in same extension
- Pass IConfiguration to methods that need it
- Document extension methods with XML comments
- Use scoped lifetime for DbContext and repositories

❌ **Don't:**
- Register services directly in Program.cs
- Mix multiple concerns in one extension
- Use singleton for services with state
- Forget to call extension methods
- Register the same service twice

---

## Service Registration Order

The order of DI registration generally doesn't matter, but a logical order is:

1. **Configuration & Logging**
2. **Database & Persistence**
3. **Authentication & Security**
4. **Business Services**
5. **API Services (Controllers, etc.)**
