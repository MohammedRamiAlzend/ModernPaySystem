# API Skill — ModernPaySystem

## Purpose

The API layer (`ModernPaySystem` project) is the **composition root** and **HTTP entry point**. It receives HTTP requests, delegates to Application services, and returns HTTP responses. It contains **zero business logic** and **zero data access**.

---

## Responsibilities

| Responsibility | Details |
|---------------|---------|
| **Controllers** | Map routes to actions, call one service method, return `IActionResult` |
| **Authentication** | JWT middleware — validate tokens, set `HttpContext.User` |
| **Authorization** | `[Authorize]` attributes, policy-based role checks |
| **Middleware** | Global error handling, request logging, correlation IDs |
| **Swagger** | OpenAPI spec generation via Swashbuckle |
| **Dependency Injection** | `Program.cs` — register all services, repositories, DbContext |
| **Model Binding** | Validate incoming DTOs via `[FromBody]`, `[FromQuery]` |
| **Route Configuration** | `[Route("api/[controller]")]`, attribute routing |

---

## Folder Structure

```
ModernPaySystem/
├── Controllers/
│   ├── AuthController.cs
│   ├── UsersController.cs
│   ├── PaymentsController.cs
│   └── ...
├── Middleware/
│   ├── ExceptionHandlingMiddleware.cs
│   └── RequestLoggingMiddleware.cs
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
└── ModernPaySystem.csproj
```

---

## Design Rules

### Rule 1 — Controllers Are Thin Wrappers

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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _userService.GetByIdAsync(id, ct);
        return result.ToActionResult();
    }
}
```

**One action = one service call.** No if/else, no loops, no business logic.

### Rule 2 — Always Inject, Never Instantiate

✅ Inject through constructor:
```csharp
public class PaymentsController(IPaymentService paymentService)
```

❌ Never create with `new`:
```csharp
var service = new PaymentService(); // ❌ Forbidden
```

### Rule 3 — Return `IActionResult` via `Result<T>.ToActionResult()`

```csharp
var result = await _service.DoSomethingAsync(dto);
return result.ToActionResult(); // ✅ Maps Result to 200/201/400/404/500 automatically
```

### Rule 4 — No DbContext or Repository in Controllers

```csharp
// ❌ Forbidden
private readonly AppDbContext _context;
private readonly IUserRepository _userRepository;
```

Controllers inject **service interfaces** only.

### Rule 5 — Controllers Must Not Access Repositories or DbContext

```csharp
// ❌ FORBIDDEN — controllers using DbContext or RepositoryBase
public class RequestsController(AppDbContext context, IRepositoryBase<Request, Guid> requestRepo) : ControllerBase

// ✅ CORRECT — controllers only inject I*Service interfaces
public class RequestsController(IRequestService requestService) : ControllerBase
```

---

## Controller Examples

### Standard CRUD pattern

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService) => _userService = userService;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _userService.GetAllAsync(ct);
        return result.ToActionResult();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _userService.GetByIdAsync(id, ct);
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto, CancellationToken ct)
    {
        var result = await _userService.CreateAsync(dto, ct);
        return result.ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto, CancellationToken ct)
    {
        var result = await _userService.UpdateAsync(id, dto, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _userService.DeleteAsync(id, ct);
        return result.ToActionResult();
    }
}
```

### Authentication controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;

    public AuthController(IAuthenticationService authService) => _authService = authService;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(dto, ct);
        return result.ToActionResult();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken ct)
    {
        var result = await _authService.RegisterAsync(dto, ct);
        return result.ToActionResult();
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto, CancellationToken ct)
    {
        var result = await _authService.RefreshTokenAsync(dto, ct);
        return result.ToActionResult();
    }
}
```

### Authorized controller

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService) => _paymentService = paymentService;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentDto dto, CancellationToken ct)
    {
        var userId = User.GetUserId(); // Extension method on ClaimsPrincipal
        var result = await _paymentService.CreateAsync(userId, dto, ct);
        return result.ToActionResult();
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyPayments(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _paymentService.GetByUserIdAsync(userId, ct);
        return result.ToActionResult();
    }
}
```

---

## Validation Strategy

### DTO validation via data annotations

```csharp
public class CreateUserDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [MinLength(8)]
    public string Password { get; set; }
}
```

### FluentValidation (alternative)

```csharp
public class CreateUserValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(2).MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}
```

### Program.cs registration

```csharp
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserValidator>();
```

### Never validate in controller body

```csharp
// ❌ BAD
if (string.IsNullOrEmpty(dto.Name))
    return BadRequest("Name required");

// ✅ GOOD — let [ApiController] handle it
// Or use FluentValidation
```

---

## Security Recommendations

| Recommendation | Implementation |
|---------------|---------------|
| **Use JWT with short expiration** | 15-30 min access token, 7-day refresh token |
| **Apply `[Authorize]` by default** | Set `FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()` in `Program.cs` |
| **Use HTTPS only** | `app.UseHttpsRedirection()` |
| **Validate all input** | Data annotations + FluentValidation |
| **Never expose stack traces** | Global exception middleware returns generic error |
| **Use CORS with specific origins** | `builder.Services.AddCors(o => o.AddPolicy("AllowFrontend", p => p.WithOrigins("https://localhost:5173")))` |
| **Rate limiting** | Use `builder.Services.AddRateLimiter()` for sensitive endpoints |
| **Anti-forgery for state-changing endpoints** | Token-based (JWT) handles this automatically |
| **Permission-based authorization** | `[EndpointPermission("requests.get-by-id", SubSystem.TransactionSystem, PermissionType.Read)]` + `[Authorize]` |

### Program.cs security setup

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
```

---

## Performance Recommendations

| Area | Recommendation |
|------|---------------|
| **Async all the way** | Every controller action is `async Task<IActionResult>` |
| **Cancellation tokens** | Accept `CancellationToken ct` and pass to all service calls |
| **Response caching** | `[ResponseCache(Duration = 60)]` on GET endpoints |
| **Output caching** | `builder.Services.AddOutputCache()` for frequently accessed data |
| **Compression** | `builder.Services.AddResponseCompression()` |
| **No sync-over-async** | Never `.Result` or `.Wait()` |
| **Pagination** | **Use `IUnitOfWork + RepositoryBase.GetPagedAsync()` — never load full tables** |
| **Minimal allocations** | Avoid large object allocations in hot paths |

---

## Testing Recommendations

### Unit tests
- Use `Mock<IUserService>` to test controller behavior
- Test all HTTP status code paths (200, 201, 400, 404, 500)
- Verify `Result.ToActionResult()` produces correct status

### Integration tests
- Use `WebApplicationFactory<T>` with test database
- Test full request/response pipeline including middleware
- Test authentication and authorization

### Example controller test

```csharp
[Fact]
public async Task GetById_WhenUserExists_ReturnsOk()
{
    var userId = Guid.NewGuid();
    var userDto = new UserDto { Id = userId, Name = "Test" };
    var service = new Mock<IUserService>();
    service.Setup(s => s.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
           .ReturnsAsync(Result<UserDto>.Success(userDto));

    var controller = new UsersController(service.Object);
    var result = await controller.GetById(userId, CancellationToken.None);

    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.Equal(userDto, okResult.Value);
}
```

---

## AI Generation Instructions

### When generating a new controller

```markdown
1. Place file in `ModernPaySystem/Controllers/{EntityName}Controller.cs`
2. Use file-scoped namespace: `namespace ModernPaySystem.Controllers;`
3. Decorate with `[ApiController]` and `[Route("api/[controller]")]`
4. Inject exactly one service interface via primary constructor (never DbContext, RepositoryBase, or IUnitOfWork)
5. Create methods matching: GetAll, GetById, Create, Update, Delete
6. Each method:
    - Accepts DTO from body/query/route
    - Accepts `CancellationToken ct`
    - Calls ONE service method
    - Returns `result.ToActionResult()`
7. Add `[Authorize]` if endpoint requires authentication
8. Controllers NEVER reference ModernPaySystem.Infrastructure.Persistence or ModernPaySystem.Infrastructure.Persistence.UnitOfWork
9. Never add business logic, DB access, or repository usage
```

### When modifying Program.cs

```markdown
1. Never remove existing registrations
2. Group registrations by layer:
   // Persistence
   builder.Services.AddDbContext<...>();
   builder.Services.AddScoped<I..., ...>();

   // Infrastructure
   builder.Services.AddScoped<I..., ...>();

   // Application
   builder.Services.AddScoped<I..., ...>();

   // Auth
   builder.Services.AddAuthentication(...);

   // Swagger
   builder.Services.AddSwaggerGen(...);

   // CORS, compression, rate limiting
   builder.Services.AddCors(...);
```

### Checklist before submitting

```markdown
- [ ] Controller contains no business logic
- [ ] Controller does NOT reference DbContext, RepositoryBase, or IUnitOfWork
- [ ] Controller injects ONLY I*Service interfaces
- [ ] All actions return IActionResult
- [ ] All async methods pass CancellationToken
- [ ] DTO validation attributes are present
- [ ] [Authorize] is applied where needed
- [ ] Route follows `/api/[controller]` convention
- [ ] Only service interfaces are injected (no concrete types)
- [ ] No `using` statements for Persistence or EF Core namespaces
```
