using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using ModernPaySystem.Module.Identity.Application;
using ModernPaySystem.Module.Identity.Infrastructure.Persistence;
using ModernPaySystem.SharedKernel.Application.Repos;
using ModernPaySystem.SharedKernel.Domain.Entities;
using ModernPaySystem.SharedKernel.Infrastructure.Persistence;

namespace ModernPaySystem.Module.Identity.Infrastructure;

public class IdentityUnitOfWork(
    IdentityDbContext dbContext,
    ILogger<IdentityUnitOfWork> logger,
    ILoggerFactory loggerFactory) : IIdentityUnitOfWork
{
    private readonly IdentityDbContext _dbContext = dbContext;
    private readonly ILogger<IdentityUnitOfWork> _logger = logger;
    private readonly ILoggerFactory _loggerFactory = loggerFactory;
    private IDbContextTransaction? _transaction;

    private IRepositoryBase<User, Guid>? _users;
    private IRepositoryBase<Role, Guid>? _roles;
    private IRepositoryBase<PermissionEntity, Guid>? _permissions;
    private IRepositoryBase<Department, Guid>? _departments;
    private IRepositoryBase<SubSystemUser, Guid>? _subSystemUsers;

    public IRepositoryBase<User, Guid> Users =>
        _users ??= new RepositoryBase<User, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<User, Guid>>());

    public IRepositoryBase<Role, Guid> Roles =>
        _roles ??= new RepositoryBase<Role, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<Role, Guid>>());

    public IRepositoryBase<PermissionEntity, Guid> Permissions =>
        _permissions ??= new RepositoryBase<PermissionEntity, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<PermissionEntity, Guid>>());

    public IRepositoryBase<Department, Guid> Departments =>
        _departments ??= new RepositoryBase<Department, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<Department, Guid>>());

    public IRepositoryBase<SubSystemUser, Guid> SubSystemUsers =>
        _subSystemUsers ??= new RepositoryBase<SubSystemUser, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<SubSystemUser, Guid>>());

    public bool HasActiveTransaction => _transaction != null;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving changes to the database");
            throw;
        }
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        _logger.LogInformation("Database transaction started");
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            await _transaction?.CommitAsync(cancellationToken)!;
            _logger.LogInformation("Database transaction committed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error committing transaction");
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _transaction?.RollbackAsync(cancellationToken)!;
            _logger.LogInformation("Database transaction rolled back");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rolling back transaction");
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }
}
