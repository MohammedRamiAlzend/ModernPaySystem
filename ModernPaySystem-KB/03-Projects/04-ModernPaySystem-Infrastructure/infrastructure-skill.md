# Infrastructure Skill — ModernPaySystem.Infrastructure

## Purpose

The Infrastructure layer implements **side effects** and **external integrations**. It contains the real implementations of interfaces defined in the Application layer — authentication, email, SMS, file storage, OCR, and third-party API clients. It has **no business logic** and **no HTTP endpoints**.

---

## Responsibilities

| Responsibility | Implementation |
|---------------|---------------|
| **Authentication** | JWT token generation, password hashing, refresh tokens |
| **Email** | SendGrid / SMTP email sender |
| **SMS** | Twilio / SMS provider |
| **File Storage** | Local / Azure Blob / S3 file system |
| **OCR** | Tesseract / Azure OCR integration |
| **External APIs** | Payment gateway, tax calculator, address verification |
| **Caching** | Redis / in-memory cache implementation |
| **Logging** | Serilog structured logging |
| **Configuration** | Strongly-typed options pattern |
| **Service Implementation** | `IUserService`, `IRequestService`, etc. — all in `Services/` folder, inject `IUnitOfWork` as primary data access |
| **Data Access** | Via `IUnitOfWork` → `IRepositoryBase<T, TKey>` — services NEVER inject repositories directly |

---

## Folder Structure

```
ModernPaySystem.Infrastructure/
├── Auth/
│   ├── JwtTokenGenerator.cs
│   ├── PasswordHasher.cs
│   └── CurrentUserService.cs
├── Email/
│   ├── SendGridEmailService.cs
│   └── SmtpEmailService.cs
├── Sms/
│   └── TwilioSmsService.cs
├── FileStorage/
│   ├── LocalFileStorageService.cs
│   └── AzureBlobStorageService.cs
├── Cache/
│   └── RedisCacheService.cs
├── ExternalApis/
│   ├── PaymentGatewayClient.cs
│   └── TaxCalculatorClient.cs
├── Logging/
│   └── SerilogExtensions.cs
├── ModernPaySystem.Infrastructure.csproj
└── DependencyInjection.cs
```

---

## UnitOfWork Access Pattern

### IUnitOfWork is the ONLY gateway to data

Infrastructure services **MUST** inject `IUnitOfWork` and access all repositories through it:

```csharp
// ✅ CORRECT — UnitOfWork injection
public class RequestService(
    IUnitOfWork unitOfWork,
    ILogger<RequestService> logger,
    IWebAttachmentService webAttachmentService,
    IHttpContextServiceManager httpContextServiceManager) : IRequestService
{
    public async Task<Result<PagedList<RequestDto>>> GetPagedAsync(RequestPagedFilterDto? filterDto = null)
    {
        var result = await unitOfWork.Requests.GetPagedAsync(page, pageSize, ...);
        // ...
    }
}
```

### ❌ FORBIDDEN — Direct repository injection

```csharp
// ❌ FORBIDDEN
public class MyService(
    IUnitOfWork unitOfWork,
    IRepositoryBase<MyEntity, Guid> myRepo) : IMyService  // ← DIRECT REPO INJECTION FORBIDDEN

// ❌ FORBIDDEN
using ModernPaySystem.Infrastructure.Persistence.Repos;  // ← DIRECT REPO NAMESPACE FORBIDDEN
```

### IUnitOfWork provides all repositories

```csharp
// From ModernPaySystem.Infrastructure.Persistence.UnitOfWork.IUnitOfWork
public interface IUnitOfWork
{
    IRepositoryBase<User, Guid> Users { get; }
    IRepositoryBase<Request, Guid> Requests { get; }
    IRepositoryBase<ArchiveRecord, Guid> ArchiveRecords { get; }
    // ... one property per entity
    Task<int> SaveChangesAsync();
}
```

### AI Coding Rule

```
When writing an Infrastructure service:
1. Inject IUnitOfWork as the primary data access dependency
2. Access repositories via unitOfWork.{PluralEntityName}.{Method}()
3. NEVER inject IRepositoryBase<T, TKey> directly
4. NEVER import ModernPaySystem.Infrastructure.Persistence.Repos
```

---

## Service Implementation Patterns

### Pattern 1 — IUnitOfWork for All Data Access

Infrastructure services inject `IUnitOfWork` (from `ModernPaySystem.Infrastructure.Persistence.UnitOfWork`) and access all repositories through it:

```csharp
// Application defines the contract
// Application/Interfaces/IRequestService.cs
public interface IRequestService
{
    Task<Result<PagedList<RequestDto>>> GetPagedAsync(RequestPagedFilterDto? filterDto);
}

// Infrastructure implements it — injects IUnitOfWork
// Infrastructure/Services/RequestService.cs
public class RequestService(
    IUnitOfWork unitOfWork,
    ILogger<RequestService> logger) : IRequestService
{
    public async Task<Result<PagedList<RequestDto>>> GetPagedAsync(...)
    {
        var result = await unitOfWork.Requests.GetPagedAsync(page, pageSize, ...);
        // ...
    }
}
```

### Pattern 2 — Options Pattern for Configuration

```csharp
// Infrastructure/Email/EmailOptions.cs
public class EmailOptions
{
    public const string SectionName = "Email";
    public string ApiKey { get; init; }
    public string FromAddress { get; init; }
    public string FromName { get; init; }
}

// Infrastructure/DependencyInjection.cs
public static IServiceCollection AddInfrastructure(
    this IServiceCollection services, IConfiguration configuration)
{
    services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
    services.AddScoped<IEmailService, SendGridEmailService>();
    return services;
}
```

### Pattern 3 — HttpClient Factory for External APIs

```csharp
// Infrastructure/ExternalApis/PaymentGatewayClient.cs
public class PaymentGatewayClient : IPaymentGatewayClient
{
    private readonly HttpClient _httpClient;

    public PaymentGatewayClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.paymentgateway.com/");
    }

    public async Task<Result<PaymentResponse>> ChargeAsync(PaymentRequest request, CancellationToken ct)
    {
        var response = await _httpClient.PostAsJsonAsync("charges", request, ct);
        if (!response.IsSuccessStatusCode)
            return Result<PaymentResponse>.Failure("Payment gateway declined");

        var data = await response.Content.ReadFromJsonAsync<PaymentResponse>(ct);
        return Result<PaymentResponse>.Success(data);
    }
}

// Program.cs
builder.Services.AddHttpClient<IPaymentGatewayClient, PaymentGatewayClient>(client =>
{
    client.BaseAddress = new Uri("https://api.paymentgateway.com/");
});
```

---

## Retry Policies

### Use Polly for resilience

```csharp
// Infrastructure/Resilience/RetryPolicy.cs
public static class RetryPolicy
{
    public static IAsyncPolicy<T> GetDefaultRetryPolicy<T>(int retryCount = 3)
    {
        return Policy<T>
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount,
                attempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    // Log retry attempt
                });
    }
}

// Usage in service
public async Task<Result<PaymentResponse>> ChargeAsync(PaymentRequest request, CancellationToken ct)
{
    var policy = RetryPolicy.GetDefaultRetryPolicy<HttpResponseMessage>();
    var response = await policy.ExecuteAsync(() => _httpClient.PostAsJsonAsync("charges", request, ct));
    // ...
}
```

### Polly Policies by Use Case

| Use Case | Policy | Retries | Circuit Breaker |
|----------|--------|---------|-----------------|
| External API call | Wait and Retry | 3 (exponential backoff) | Yes — 5 failures in 30s |
| Email send | Wait and Retry | 2 (immediate) | No |
| OCR processing | Wait and Retry | 1 | No |
| File upload | No retry | 0 | No |

---

## Resilience Patterns

### Circuit Breaker

```csharp
var circuitBreaker = Policy
    .Handle<HttpRequestException>()
    .CircuitBreakerAsync(
        exceptionsAllowedBeforeBreaking: 5,
        durationOfBreak: TimeSpan.FromSeconds(30),
        onBreak: (ex, duration) => { /* Log */ },
        onReset: () => { /* Log */ });

var retry = Policy
    .Handle<HttpRequestException>()
    .WaitAndRetryAsync(3, attempt => TimeSpan.FromMilliseconds(200));

var wrapped = Policy.WrapAsync(retry, circuitBreaker);
```

### Timeout

```csharp
var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));
```

### Bulkhead (Isolation)

```csharp
var bulkhead = Policy.BulkheadAsync<HttpResponseMessage>(
    maxParallelization: 10,
    maxQueuingActions: 20);
```

---

## Logging Strategy

### Structured logging with Serilog

```csharp
// Infrastructure/Logging/SerilogExtensions.cs
public static class SerilogExtensions
{
    public static IHostBuilder UseSerilogLogging(this IHostBuilder host)
    {
        return host.UseSerilog((context, config) =>
        {
            config.ReadFrom.Configuration(context.Configuration)
                  .Enrich.FromLogContext()
                  .Enrich.WithMachineName()
                  .Enrich.WithEnvironmentName()
                  .WriteTo.Console()
                  .WriteTo.Seq("http://localhost:5341");
        });
    }
}

// Program.cs
builder.Host.UseSerilogLogging();
```

### Logging in services

```csharp
public class SendGridEmailService : IEmailService
{
    private readonly ILogger<SendGridEmailService> _logger;

    public SendGridEmailService(ILogger<SendGridEmailService> logger) => _logger = logger;

    public async Task<Result> SendAsync(string to, string subject, string body, CancellationToken ct)
    {
        _logger.LogInformation("Sending email to {To} with subject {Subject}", to, subject);
        // ...
        _logger.LogInformation("Email sent successfully to {To}", to);
    }
}
```

### Logging levels

| Level | When |
|-------|------|
| `Trace` | Method entry/exit |
| `Debug` | External API request/response bodies |
| `Information` | Successful operation, email sent, file uploaded |
| `Warning` | Retry attempt, rate limit approaching |
| `Error` | External API failure, email send failure |
| `Fatal` | Unrecoverable error, service unavailable |

---

## Caching Strategy

### Cache Interface

```csharp
// Application/Interfaces/ICacheService.cs
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);
}
```

### Redis Implementation

```csharp
// Infrastructure/Cache/RedisCacheService.cs
public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var value = await db.StringGetAsync(key);
        return value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(value);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var serialized = JsonSerializer.Serialize(value);
        await db.StringSetAsync(key, serialized, expiration);
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(key);
    }
}
```

### Caching Strategy by Data

| Data Type | Cache Duration | Invalidation |
|-----------|---------------|--------------|
| User profile | 5 minutes | On profile update |
| Product catalog | 1 hour | On product change (event-driven) |
| Lookup data (countries) | 24 hours | Manual |
| Authentication tokens | Until expiry | On logout / refresh |

---

## Security Recommendations

| Area | Recommendation |
|------|---------------|
| **API Keys** | Store in `appsettings.*.json` (dev) or Key Vault / env vars (prod) |
| **JWT Secrets** | Minimum 256-bit key, rotate regularly |
| **Password Hashing** | Use `BCrypt.Net` or `PBKDF2` — never plain text or MD5/SHA1 |
| **HTTPS** | Always validate certificates; never disable SSL in production |
| **External API Calls** | Validate all responses; never trust external data implicitly |
| **File Upload** | Scan for malware; limit file size; restrict extensions |
| **Email** | Sanitize all inputs to prevent injection; never expose API keys in logs |

---

## AI Generation Rules

### When creating a new infrastructure service

```markdown
1. Implement interface defined in Application/Interfaces/
2. Place in `Infrastructure/{Category}/{ServiceName}.cs`
3. Class name matches implementation: `SendGridEmailService : IEmailService`
4. Inject `IUnitOfWork` as the ONLY data access dependency
5. Access repositories via `unitOfWork.{PluralEntityName}` properties
6. NEVER inject `IRepositoryBase<T, TKey>` directly
7. NEVER add `using ModernPaySystem.Infrastructure.Persistence.Repos` in services
8. Use constructor injection for all dependencies
9. Use `ILogger<T>` for logging
10. Use `IConfiguration` or Options pattern for settings
11. Return `Result<T>` or `Result` (not throw for expected failures)
12. Wrap external calls in try/catch, log and return Result.Failure
13. Apply Polly retry policies for network-dependent operations
14. Register in `DependencyInjection.cs` extension method
```

### When configuring an external API client

```markdown
1. Create typed HttpClient in `ExternalApis/{ApiName}Client.cs`
2. Register via `services.AddHttpClient<Interface, Implementation>()`
3. Use named or typed client pattern (typed preferred)
4. Configure base address and default headers
5. Apply Polly policies
6. Never store API keys in code — use configuration/options
```

### Infrastructure checklist

```markdown
- [ ] Implements interface from Application layer
- [ ] Uses Options pattern for configuration
- [ ] Logs all operations via ILogger<T>
- [ ] Returns Result<T> for expected failures
- [ ] HttpClient uses IHttpClientFactory (typed client)
- [ ] Polly retry policies applied for external calls
- [ ] No business logic (no if/else on domain concepts)
- [ ] Registered in DependencyInjection.cs
- [ ] Async all the way (no .Result, no .Wait())
- [ ] CancellationToken passed to all async calls
- [ ] Service injects IUnitOfWork (not IRepositoryBase directly)
- [ ] All repository access via unitOfWork.{EntityName}.{Method}()
- [ ] No using for ModernPaySystem.Infrastructure.Persistence.Repos
```
