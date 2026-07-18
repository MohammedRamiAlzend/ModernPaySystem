# Persistence Skill — ModernPaySystem.Infrastructure.Persistence

## Purpose

The Persistence layer is the **data access implementation**. It contains the EF Core `DbContext`, repository implementations, entity configurations, and database migrations. It implements interfaces defined in the Application layer and maps Domain entities to the database.

---

## Responsibilities

| Responsibility | Details |
|---------------|---------|
| **DbContext** | `AppDbContext` — entity sets, relationship configuration, interceptors |
| **Repositories** | `UserRepository`, `PaymentRepository` — implement `IUserRepository`, `IPaymentRepository` |
| **Migrations** | EF Core migrations for schema changes |
| **Entity Configurations** | Fluent API in `IEntityTypeConfiguration<T>` classes |
| **Interceptors** | `SaveChangesInterceptor` for auditing, soft delete |
| **Unit of Work** | `IUnitOfWork` interface + `UnitOfWork` implementation — exposes one `IRepositoryBase<T, TKey>` property per entity, manages `SaveChangesAsync()` and transactions |
| **Query Optimization** | Compiled queries, projection, indexing hints |

---

## Folder Structure

```
ModernPaySystem.Infrastructure.Persistence/
├── AppDbContext.cs
├── AppDbContextFactory.cs              // For design-time migrations
├── DependencyInjection.cs             // DI registration extension
├── Interceptors/
│   ├── AuditableEntityInterceptor.cs
│   └── SoftDeleteInterceptor.cs
├── Configurations/
│   ├── UserConfiguration.cs
│   ├── PaymentConfiguration.cs
│   ├── InvoiceConfiguration.cs
│   └── TransactionConfiguration.cs
├── Repositories/
│   ├── UserRepository.cs
│   ├── PaymentRepository.cs
│   ├── InvoiceRepository.cs
│   └── BaseRepository.cs
├── Migrations/
│   ├── 20260101000000_Initial.cs
│   ├── 20260201000000_AddPayments.cs
│   └── AppDbContextModelSnapshot.cs
├── Extensions/
│   ├── QueryableExtensions.cs
│   └── ModelBuilderExtensions.cs
└── ModernPaySystem.Infrastructure.Persistence.csproj
```

---

## EF Core Best Practices

### 1. DbContext Configuration

```csharp
// AppDbContext.cs
public class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Payment> Payments => Set<Payment>();

    private readonly AuditableEntityInterceptor _auditInterceptor;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        AuditableEntityInterceptor auditInterceptor)
        : base(options)
    {
        _auditInterceptor = auditInterceptor;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditInterceptor);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }
        return await base.SaveChangesAsync(ct);
    }
}
```

### 2. Entity Configuration with Fluent API

```csharp
// Configurations/UserConfiguration.cs
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .IsRequired()
                .HasMaxLength(200);
            email.HasIndex(e => e.Value).IsUnique();
        });

        builder.Property(u => u.PasswordHash).IsRequired();

        builder.Property(u => u.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasQueryFilter(u => !u.IsDeleted);

        builder.Ignore(u => u.DomainEvents);
    }
}
```

### 3. Repository Implementation — GetAllByIdsAsync

For bulk retrieval by primary keys, use `GetAllByIdsAsync`:

```csharp
// RepositoryBase<T, TKey>
public async Task<Result<List<TEntity>>> GetAllByIdsAsync(
    List<TKey> ids,
    Func<IQueryable<TEntity>, IQueryable<TEntity>>? transform = null,
    bool bypassAuth = false)
{
    IQueryable<TEntity> query = dbcontext.Set<TEntity>();
    query = query.Where(e => ids.Contains(e.Id));

    if (transform != null)
        query = transform(query);

    query = ApplyDefaultOrdering(query);
    return await query.ToListAsync();
}
```

### 4. Legacy Repository Implementation

```csharp
// Repositories/BaseRepository.cs
public abstract class BaseRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext Context;
    protected readonly DbSet<T> Set;

    protected BaseRepository(AppDbContext context)
    {
        Context = context;
        Set = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await Set.FindAsync(new object[] { id }, ct);

    public async Task<List<T>> GetAllAsync(CancellationToken ct = default)
        => await Set.ToListAsync(ct);

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await Set.AddAsync(entity, ct);

    public void Update(T entity) => Set.Update(entity);
    public void Delete(T entity) => Set.Remove(entity);
}

// Repositories/UserRepository.cs
public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await Set.FirstOrDefaultAsync(u => u.Email.Value == email, ct);

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
        => await Set.AnyAsync(u => u.Email.Value == email, ct);

    public async Task<List<User>> GetActiveUsersAsync(CancellationToken ct = default)
        => await Set.Where(u => u.IsActive).ToListAsync(ct);
}
```

### 4. Dependency Injection Registration

```csharp
// DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<SoftDeleteInterceptor>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));

            if (environment.IsDevelopment())
                options.EnableSensitiveDataLogging();
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        return services;
    }
}

## UnitOfWork

### IUnitOfWork Interface

The `IUnitOfWork` interface is the **exclusive gateway** for all data access. It lives in:
```
ModernPaySystem.Infrastructure.Persistence.UnitOfWork.IUnitOfWork
```

```csharp
public interface IUnitOfWork : IDisposable
{
    AppDbContext Context { get; }
    IRepositoryBase<User, Guid> Users { get; }
    IRepositoryBase<Role, Guid> Roles { get; }
    IRepositoryBase<Request, Guid> Requests { get; }
    // ... one property per entity
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

### UnitOfWork Implementation

```csharp
public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public AppDbContext Context => context;
    public IRepositoryBase<User, Guid> Users { get; } = new RepositoryBase<User, Guid>(...);
    public IRepositoryBase<Request, Guid> Requests { get; } = new RepositoryBase<Request, Guid>(...);
    // ... one property per entity
}
```

### Mandatory Registration Rule

**Every new entity MUST be registered in IUnitOfWork.** When adding a new entity:

1. Add `DbSet<NewEntity>` to `AppDbContext`
2. Add `IRepositoryBase<NewEntity, Guid> NewEntities { get; }` to `IUnitOfWork`
3. Implement the property in `UnitOfWork.cs`
4. The entity is now accessible via `unitOfWork.NewEntities.GetAsync(...)`

### DI Registration

```csharp
// Persistence/DependencyInjection.cs
services.AddScoped<IUnitOfWork, UnitOfWork>();
```

### Usage in Infrastructure Services

```csharp
// Infrastructure/Services/RequestService.cs
public class RequestService(IUnitOfWork unitOfWork) : IRequestService
{
    public async Task<Result<PagedList<RequestDto>>> GetPagedAsync(...)
    {
        var result = await unitOfWork.Requests.GetPagedAsync(page, pageSize, ...);
        // ...
    }
}
```

### ❌ Forbidden: Direct RepositoryBase Injection

```csharp
// ❌ FORBIDDEN in any service
public class MyService(IRepositoryBase<MyEntity, Guid> myRepo) : IMyService
```

---

## Query Optimization

### Always Use Projection

```csharp
// ❌ Slow — loads entire entity
var users = await _context.Users.ToListAsync();
var dtos = users.Select(u => new UserDto { Id = u.Id, Name = u.Name });

// ✅ Fast — SQL projects only needed columns
var dtos = await _context.Users
    .Where(u => u.IsActive)
    .Select(u => new UserDto { Id = u.Id, Name = u.Name })
    .ToListAsync(ct);
```

### Use Split Queries for Related Data

```csharp
// Single query with JOIN (cartesian explosion)
var invoices = await _context.Invoices
    .Include(i => i.LineItems)
    .ToListAsync(ct);

// Split query (separate SQL queries)
var invoices = await _context.Invoices
    .Include(i => i.LineItems)
    .AsSplitQuery()
    .ToListAsync(ct);
```

### Use Compiled Queries for Hot Paths

```csharp
private static readonly Func<AppDbContext, Guid, Task<User?>> GetUserById =
    EF.CompileAsyncQuery((AppDbContext context, Guid id) =>
        context.Users.FirstOrDefault(u => u.Id == id));

public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct)
    => await GetUserById(Context, id);
```

### Pagination

```csharp
public async Task<PagedResult<UserDto>> GetPagedAsync(int page, int pageSize, CancellationToken ct)
{
    var query = _context.Users.Where(u => u.IsActive);

    var total = await query.CountAsync(ct);
    var items = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(u => new UserDto { Id = u.Id, Name = u.Name })
        .ToListAsync(ct);

    return new PagedResult<UserDto>(items, total, page, pageSize);
}
```

---

## Bulk Operations

### Use ExecuteUpdate / ExecuteDelete (EF Core 7+)

```csharp
// ❌ Slow — loads and updates each entity
var users = await _context.Users.Where(u => u.IsExpired).ToListAsync(ct);
foreach (var user in users) user.IsActive = false;
await _context.SaveChangesAsync(ct);

// ✅ Fast — single SQL UPDATE
await _context.Users
    .Where(u => u.IsExpired)
    .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.IsActive, false), ct);

// ✅ Fast — single SQL DELETE
await _context.Users
    .Where(u => u.IsExpired)
    .ExecuteDeleteAsync(ct);
```

### Bulk Insert (EF Core 7+)

```csharp
// For large imports, use SqlBulkCopy directly
// Or use EF Core's AddRange for moderate sizes
await _context.Users.AddRangeAsync(newUsers, ct);
await _context.SaveChangesAsync(ct);
```

---

## Indexing Recommendations

### In Entity Configuration

```csharp
// Single column index
builder.HasIndex(u => u.Email).IsUnique();

// Composite index for common query pattern
builder.HasIndex(p => new { p.UserId, p.Status });

// Include columns for covering index
builder.HasIndex(p => p.CreatedAt)
    .IncludeProperties(p => new { p.Amount, p.Status });
```

### Recommended Indexes

| Table | Index | Type | Reason |
|-------|-------|------|--------|
| `Users` | `Email` | Unique | Login lookup |
| `Users` | `IsActive, CreatedAt` | Filtered | Active user queries |
| `Payments` | `UserId, Status` | Composite | User payment history |
| `Payments` | `CreatedAt` | Range | Date-range reports |
| `Invoices` | `DueAt` | Range | Overdue determination |
| `Transactions` | `ReferenceId` | Unique | Deduplication |

---

## Auditing Recommendations

### Auditable Entity Pattern

```csharp
// Domain/Entities/Abstraction/BaseAuditableEntity.cs
public abstract class BaseAuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

// Interceptors/AuditableEntityInterceptor.cs
public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;

    public AuditableEntityInterceptor(ICurrentUserService currentUser) => _currentUser = currentUser;

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateAuditableEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken ct = default)
    {
        UpdateAuditableEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, ct);
    }

    private void UpdateAuditableEntities(DbContext? context)
    {
        if (context is null) return;

        foreach (var entry in context.ChangeTracker.Entries<BaseAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.CreatedBy = _currentUser.UserId ?? "system";
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedBy = _currentUser.UserId ?? "system";
            }
        }
    }
}
```

---

## Soft Delete Recommendations

### Query Filter Approach

```csharp
// Interceptors/SoftDeleteInterceptor.cs
public class SoftDeleteInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        SoftDeleteEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    private static void SoftDeleteEntities(DbContext? context)
    {
        if (context is null) return;

        foreach (var entry in context.ChangeTracker.Entries<ISoftDeletable>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = DateTime.UtcNow;
            }
        }
    }
}

// In DbContext
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    modelBuilder.ApplyGlobalFilters<ISoftDeletable>(e => !e.IsDeleted);
}
```

---

## Performance Recommendations

| Recommendation | Why | Impact |
|---------------|-----|--------|
| **Use `AsNoTracking()` for read-only queries** | Skips change tracking overhead | High |
| **Use projection (`Select`) instead of `Include`** | Reduces data transferred | High |
| **Prefer `CountAsync` over `ToList().Count`** | SQL `COUNT` not full table scan | High |
| **Add query filters for soft delete** | Automatic filtering, no missed filters | Medium |
| **Use compiled queries for hot paths** | Skips query compilation | Medium |
| **Disable `EnableSensitiveDataLogging` in prod** | Performance + security | Medium |
| **Batch `SaveChangesAsync` calls** | Reduce round trips | Medium |
| **Use `AsSplitQuery()` for multiple includes** | Avoid cartesian explosion | Medium |
| **Profile with `ToQueryString()` during dev** | See actual SQL generated | Low |
| **Set `PoolSize` for DbContext pool** | Reuse context instances | Low |

---

## AI Generation Rules

### When creating a new repository

```markdown
1. Define interface in Application: `I{Entity}Repository`
2. Implement in Persistence: `{Entity}Repository : BaseRepository<{Entity}>, I{Entity}Repository`
3. Constructor takes `AppDbContext context`, passes to base
4. Methods:
   - Return `Task<T?>` for single items
   - Return `Task<List<T>>` for collections
   - Return `Task<bool>` for existence checks
   - Never return `IQueryable<T>`
5. Use `FindAsync` for ID lookups
6. Use `FirstOrDefaultAsync` / `ToListAsync` for queries
7. Use `AddAsync` for inserts
8. Register in `DependencyInjection.cs`
```

### When creating a new entity configuration

```markdown
1. Place in `Configurations/{Entity}Configuration.cs`
2. Implement `IEntityTypeConfiguration<{Entity}>`
3. Every property configured: column name, type, max length, required
4. Value objects use `OwnsOne` or `OwnsMany`
5. Indexes added for query patterns
6. Query filters applied for soft delete
7. Table name explicit: `builder.ToTable("{PluralName}")`
```

### When creating a migration

```markdown
1. `dotnet ef migrations add {Name} --project ModernPaySystem.Infrastructure.Persistence`
2. Review generated migration before applying
3. Ensure no data loss on existing columns
4. Add SQL comments for complex migrations
5. Test both `Up()` and `Down()` methods
6. Add the new entity's repository property to IUnitOfWork
7. Add the property implementation to UnitOfWork.cs
8. If the entity needs soft-delete or audit, ensure query filters are configured
```

### Persistence checklist

```markdown
- [ ] Repository implements Application interface only
- [ ] No business logic in repositories
- [ ] No DTO references in repositories (return entities)
- [ ] Entity configurations use Fluent API, not data annotations
- [ ] Indexes exist for all common query patterns
- [ ] Soft delete filter applied globally
- [ ] Auditing interceptor is registered
- [ ] Migrations are reviewed and tested
- [ ] `AsNoTracking()` used for read-only queries
- [ ] `Projection` used instead of loading full entities
- [ ] New entity registered in IUnitOfWork with IRepositoryBase<NewEntity, Guid> property
- [ ] UnitOfWork.cs property implementation added
- [ ] AppDbContext has DbSet<NewEntity>
```
