using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using ModernPaySystem.Application.Repos;
using ModernPaySystem.Domain.Entities.SharedEntities;
using ModernPaySystem.Domain.Entities.TransactionSystemEntities;
using ModernPaySystem.Infrastructure.Persistence.Repos;

namespace ModernPaySystem.Infrastructure.Persistence.UnitOfWork;

/// <summary>
/// Unit of Work implementation for managing multiple repositories and transactions.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<UnitOfWork> _logger;
    private IDbContextTransaction? _transaction;

    // Repositories
    private IRepositoryBase<User, Guid>? _users;
    private IRepositoryBase<Role, Guid>? _roles;
    private IRepositoryBase<PermissionEntity, Guid>? _permissions;
    private IRepositoryBase<SubSystemUser, Guid>? _subSystemUsers;
    private IRepositoryBase<Attachment, Guid>? _attachments;
    private IRepositoryBase<Template, Guid>? _templates;
    private IRepositoryBase<Request, Guid>? _requests;
    private IRepositoryBase<Response, Guid>? _responses;
    private IRepositoryBase<TemplateOwnership, Guid>? _templateOwnerships;
    private IRepositoryBase<ResponseAttachment, Guid>? _responseAttachments;
    private IRepositoryBase<RequestAttachment, Guid>? _requestAttachments;

    public UnitOfWork(AppDbContext dbContext, ILogger<UnitOfWork> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public IRepositoryBase<User, Guid> Users =>
        _users ??= new RepositoryBase<User, Guid>(_dbContext, new LoggerFactory().CreateLogger<RepositoryBase<User, Guid>>());

    public IRepositoryBase<Role, Guid> Roles =>
        _roles ??= new RepositoryBase<Role, Guid>(_dbContext, new LoggerFactory().CreateLogger<RepositoryBase<Role, Guid>>());

    public IRepositoryBase<PermissionEntity, Guid> Permissions =>
        _permissions ??= new RepositoryBase<PermissionEntity, Guid>(_dbContext, new LoggerFactory().CreateLogger<RepositoryBase<PermissionEntity, Guid>>());

    public IRepositoryBase<SubSystemUser, Guid> SubSystemUsers =>
        _subSystemUsers ??= new RepositoryBase<SubSystemUser, Guid>(_dbContext, new LoggerFactory().CreateLogger<RepositoryBase<SubSystemUser, Guid>>());

    public IRepositoryBase<Attachment, Guid> Attachments =>
        _attachments ??= new RepositoryBase<Attachment, Guid>(_dbContext, new LoggerFactory().CreateLogger<RepositoryBase<Attachment, Guid>>());

    public IRepositoryBase<Template, Guid> Templates =>
        _templates ??= new RepositoryBase<Template, Guid>(_dbContext, new LoggerFactory().CreateLogger<RepositoryBase<Template, Guid>>());

    public IRepositoryBase<Request, Guid> Requests =>
        _requests ??= new RepositoryBase<Request, Guid>(_dbContext, new LoggerFactory().CreateLogger<RepositoryBase<Request, Guid>>());

    public IRepositoryBase<Response, Guid> Responses =>
        _responses ??= new RepositoryBase<Response, Guid>(_dbContext, new LoggerFactory().CreateLogger<RepositoryBase<Response, Guid>>());

    public IRepositoryBase<TemplateOwnership, Guid> TemplateOwnerships =>
        _templateOwnerships ??= new RepositoryBase<TemplateOwnership, Guid>(_dbContext, new LoggerFactory().CreateLogger<RepositoryBase<TemplateOwnership, Guid>>());

    public IRepositoryBase<ResponseAttachment, Guid> ResponseAttachments =>
        _responseAttachments ??= new RepositoryBase<ResponseAttachment, Guid>(_dbContext, new LoggerFactory().CreateLogger<RepositoryBase<ResponseAttachment, Guid>>());
    public IRepositoryBase<RequestAttachment, Guid> RequestAttachments =>
        _requestAttachments ??= new RepositoryBase<RequestAttachment, Guid>(_dbContext, new LoggerFactory().CreateLogger<RepositoryBase<RequestAttachment, Guid>>());

    public async Task<int> SaveChangesAsync()
    {
        try
        {
            return await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving changes to the database");
            throw;
        }
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _dbContext.Database.BeginTransactionAsync();
        _logger.LogInformation("Database transaction started");
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await SaveChangesAsync();
            await _transaction?.CommitAsync()!;
            _logger.LogInformation("Database transaction committed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error committing transaction");
            await RollbackTransactionAsync();
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

    public async Task RollbackTransactionAsync()
    {
        try
        {
            await _transaction?.RollbackAsync()!;
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

    public async ValueTask DisposeAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
        }

        await _dbContext.DisposeAsync();
    }
}
