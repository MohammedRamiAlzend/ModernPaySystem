# Unit of Work Pattern Implementation Guide

## Overview
The Unit of Work pattern coordinates the work of multiple repositories by representing it as a single unit. This ensures data consistency and provides a clean abstraction for database operations.

## Architecture

### Components

1. **IUnitOfWork** - Interface defining the contract for Unit of Work
2. **UnitOfWork** - Implementation managing all repositories and transactions
3. **RepositoryBase<TEntity, TKey>** - Generic repository for CRUD operations
4. **AppDbContext** - Entity Framework DbContext

## Key Features

✅ **Lazy Loading Repositories** - Repositories are created on first access  
✅ **Transaction Support** - Built-in transaction management  
✅ **Consistent Data Access** - All entities use the same DbContext  
✅ **Easy Testing** - IUnitOfWork interface for mocking  
✅ **Dependency Injection** - Registered as scoped service  

## Available Repositories

The IUnitOfWork provides access to all entities:

```csharp
// Authentication & Authorization
uow.Users              // IRepositoryBase<User, Guid>
uow.Roles              // IRepositoryBase<Role, Guid>
uow.Permissions        // IRepositoryBase<PermissionEntity, Guid>
uow.UserRoles          // IRepositoryBase<UserRole, Guid>
uow.RolePermissions    // IRepositoryBase<RolePermission, Guid>
uow.SubSystemUsers     // IRepositoryBase<SubSystemUser, Guid>

// Transaction System
uow.Templates          // IRepositoryBase<Template, Guid>
uow.Requests           // IRepositoryBase<Request, Guid>
uow.Responses          // IRepositoryBase<Response, Guid>
uow.TemplateOwnerships // IRepositoryBase<TemplateOwnership, Guid>

// Shared
uow.Attachments        // IRepositoryBase<Attachment, Guid>
uow.RequestAttachments // IRepositoryBase<RequestAttachment, Guid>
uow.ResponseAttachments// IRepositoryBase<ResponseAttachment, Guid>
```

## Usage Examples

### Basic CRUD Operations

```csharp
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public UserController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // GET all users
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _unitOfWork.Users.GetAllAsync();
        return result.ToActionResult();
    }

    // GET user by id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _unitOfWork.Users.GetAsync(u => u.Id == id);
        return result.ToActionResult();
    }

    // CREATE user
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = request.Username,
            HashedPassword = HashPassword(request.Password)
        };

        var result = await _unitOfWork.Users.AddAsync(user);
        
        if (result.IsError)
            return result.ToActionResult();

        return result.ToCreatedAtActionResult(this, nameof(GetById), new { id = user.Id });
    }

    // UPDATE user
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        var userResult = await _unitOfWork.Users.GetAsync(u => u.Id == id);
        
        if (userResult.IsError)
            return userResult.ToActionResult();

        var user = userResult.Value;
        user.UserName = request.Username;

        var result = await _unitOfWork.Users.UpdateAsync(user);
        return result.ToActionResult();
    }

    // DELETE user
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _unitOfWork.Users.RemoveAsync(u => u.Id == id);
        return result.ToActionResult();
    }
}
```

### Transaction Support

```csharp
[ApiController]
[Route("api/[controller]")]
public class RequestApprovalController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public RequestApprovalController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpPost("{requestId}/approve")]
    public async Task<IActionResult> ApproveRequest(Guid requestId)
    {
        try
        {
            // Begin transaction
            await _unitOfWork.BeginTransactionAsync();

            // Get the request
            var requestResult = await _unitOfWork.Requests
                .GetAsync(r => r.Id == requestId);

            if (requestResult.IsError)
                return requestResult.ToActionResult();

            var request = requestResult.Value;
            
            // Check if already approved
            if (request.Status == RequestStatus.Approved)
                return new ConflictObjectResult(
                    new { error = ApplicationError.RequestAlreadyApproved });

            // Update request status
            request.Status = RequestStatus.Approved;
            request.ApprovedAt = DateTime.UtcNow;

            var updateResult = await _unitOfWork.Requests.UpdateAsync(request);

            if (updateResult.IsError)
                return updateResult.ToActionResult();

            // Create response/audit log
            var response = new Response
            {
                Id = Guid.NewGuid(),
                RequestId = requestId,
                RespondedBy = /* current user */,
                Comment = "Request approved",
                CreatedAt = DateTime.UtcNow
            };

            var responseResult = await _unitOfWork.Responses.AddAsync(response);

            if (responseResult.IsError)
                return responseResult.ToActionResult();

            // Commit transaction
            await _unitOfWork.CommitTransactionAsync();

            return new OkObjectResult(new { message = "Request approved successfully" });
        }
        catch (Exception ex)
        {
            // Rollback on error
            await _unitOfWork.RollbackTransactionAsync();
            
            // Log and return error
            return new ObjectResult(
                new { error = ApplicationError.InternalServerError })
            { StatusCode = 500 };
        }
    }
}
```

### Service Layer Pattern

```csharp
public interface IRequestService
{
    Task<Result<Created>> CreateRequestAsync(CreateRequestRequest request);
    Task<Result<Updated>> ApproveRequestAsync(Guid requestId, Guid approverId);
    Task<Result<Success>> GetRequestAsync(Guid requestId);
}

public class RequestService : IRequestService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RequestService> _logger;

    public RequestService(IUnitOfWork unitOfWork, ILogger<RequestService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Created>> CreateRequestAsync(CreateRequestRequest request)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // Validate template exists
            var templateResult = await _unitOfWork.Templates
                .GetAsync(t => t.Id == request.TemplateId);

            if (templateResult.IsError)
                return templateResult.Errors;

            // Create request
            var newRequest = new Request
            {
                Id = Guid.NewGuid(),
                TemplateId = request.TemplateId,
                RequesterId = request.RequesterId,
                ApproverId = request.ApproverId,
                CreatedAt = DateTime.UtcNow
            };

            var addResult = await _unitOfWork.Requests.AddAsync(newRequest);

            if (addResult.IsError)
                return addResult.Errors;

            // Add attachments if any
            foreach (var attachmentId in request.AttachmentIds ?? new List<Guid>())
            {
                var attachment = new RequestAttachment
                {
                    RequestId = newRequest.Id,
                    AttachmentId = attachmentId
                };

                await _unitOfWork.RequestAttachments.AddAsync(attachment);
            }

            await _unitOfWork.CommitTransactionAsync();

            return new Created(new { newRequest.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating request");
            await _unitOfWork.RollbackTransactionAsync();
            return ApplicationError.InternalServerError;
        }
    }

    public async Task<Result<Updated>> ApproveRequestAsync(Guid requestId, Guid approverId)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var requestResult = await _unitOfWork.Requests
                .GetAsync(r => r.Id == requestId);

            if (requestResult.IsError)
                return requestResult.Errors;

            var request = requestResult.Value;

            // Can't approve own request
            if (request.RequesterId == approverId)
                return ApplicationError.CannotApproveOwnRequest;

            request.Status = RequestStatus.Approved;
            request.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _unitOfWork.Requests.UpdateAsync(request);

            if (updateResult.IsError)
                return updateResult.Errors;

            await _unitOfWork.CommitTransactionAsync();

            return new Updated(new { request.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving request");
            await _unitOfWork.RollbackTransactionAsync();
            return ApplicationError.InternalServerError;
        }
    }

    public async Task<Result<Success>> GetRequestAsync(Guid requestId)
    {
        var result = await _unitOfWork.Requests
            .GetAsync(r => r.Id == requestId);

        if (result.IsError)
            return result.Errors;

        return new Success(result.Value);
    }
}
```

### Controller Using Service with Unit of Work

```csharp
[ApiController]
[Route("api/[controller]")]
public class RequestController : ControllerBase
{
    private readonly IRequestService _requestService;

    public RequestController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    [HttpPost]
    [HasPermission(Permission.TransactionSystem.CreateTransaction)]
    public async Task<IActionResult> Create([FromBody] CreateRequestRequest request)
    {
        var result = await _requestService.CreateRequestAsync(request);
        return result.ToCreatedAtActionResult(this, nameof(GetById), new { id = result.Value?.Id });
    }

    [HttpPost("{id}/approve")]
    [HasPermission(Permission.TransactionSystem.UpdateTransaction)]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveRequestRequest request)
    {
        var result = await _requestService.ApproveRequestAsync(id, request.ApproverId);
        return result.ToActionResult();
    }

    [HttpGet("{id}")]
    [HasPermission(Permission.TransactionSystem.ViewTransactions)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _requestService.GetRequestAsync(id);
        return result.ToActionResult();
    }
}
```

## Transaction Management

### Transaction Methods

```csharp
// Begin a new transaction
await _unitOfWork.BeginTransactionAsync();

// Commit changes and transaction
await _unitOfWork.CommitTransactionAsync();

// Rollback transaction
await _unitOfWork.RollbackTransactionAsync();

// Save changes without explicit transaction
await _unitOfWork.SaveChangesAsync();
```

## Best Practices

1. ✅ **Use transactions for multi-step operations**
   ```csharp
   await _unitOfWork.BeginTransactionAsync();
   try
   {
       // Multiple repository operations
       await _unitOfWork.CommitTransactionAsync();
   }
   catch
   {
       await _unitOfWork.RollbackTransactionAsync();
       throw;
   }
   ```

2. ✅ **Inject IUnitOfWork in services, not repositories**
   ```csharp
   public class RequestService
   {
       private readonly IUnitOfWork _unitOfWork; // ✅
   }
   ```

3. ✅ **Use Result pattern for consistent error handling**
   ```csharp
   var result = await _unitOfWork.Users.AddAsync(user);
   if (result.IsError)
       return result.ToActionResult();
   ```

4. ✅ **Dispose properly**
   ```csharp
   using (var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>())
   {
       // Use unit of work
   } // Disposed automatically
   ```

5. ❌ **Don't share Unit of Work across requests**
6. ❌ **Don't use DbContext directly in controllers**
7. ❌ **Don't forget to await SaveChangesAsync or CommitTransactionAsync**

## Testing with Unit of Work

```csharp
public class RequestServiceTests
{
    [Fact]
    public async Task CreateRequest_WithValidData_ReturnsCreated()
    {
        // Arrange
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var service = new RequestService(mockUnitOfWork.Object, new Mock<ILogger<RequestService>>().Object);

        var request = new CreateRequestRequest
        {
            TemplateId = Guid.NewGuid(),
            RequesterId = Guid.NewGuid(),
            ApproverId = Guid.NewGuid()
        };

        mockUnitOfWork.Setup(x => x.Templates.GetAsync(It.IsAny<Expression<Func<Template, bool>>>()))
            .ReturnsAsync(new Template { Id = request.TemplateId });

        mockUnitOfWork.Setup(x => x.Requests.AddAsync(It.IsAny<Request>()))
            .ReturnsAsync(new Success());

        // Act
        var result = await service.CreateRequestAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        mockUnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Once);
    }
}
```

## Performance Considerations

- **Lazy Loading**: Repositories are created on first access
- **DbContext Reuse**: All repositories share the same DbContext instance
- **Pooling**: Consider DbContext pooling for high-throughput scenarios
- **Transactions**: Keep transactions short to minimize lock time

## Dependency Injection Registration

Already configured in Program.cs:

```csharp
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
```

The `Scoped` lifetime ensures one Unit of Work per HTTP request.
