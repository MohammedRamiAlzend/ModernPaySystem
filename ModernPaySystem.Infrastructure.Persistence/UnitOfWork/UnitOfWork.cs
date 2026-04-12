using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using ModernPaySystem.Application.Repos;
using ModernPaySystem.Application.Services;
using ModernPaySystem.Domain.Entities.SharedEntities;
using ModernPaySystem.Domain.Entities.TransactionSystemEntities;
using ModernPaySystem.Infrastructure.Persistence.Repos;

namespace ModernPaySystem.Infrastructure.Persistence.UnitOfWork;

/// <summary>
/// Unit of Work implementation for managing multiple repositories and transactions.
/// </summary>
public class UnitOfWork(
    AppDbContext dbContext,
    ILogger<UnitOfWork> logger,
    ILoggerFactory loggerFactory,
    IHttpContextServiceManager httpContextServiceManager) : IUnitOfWork
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly ILogger<UnitOfWork> _logger = logger;
    private readonly ILoggerFactory _loggerFactory = loggerFactory;
    private readonly IHttpContextServiceManager _httpContextServiceManager = httpContextServiceManager;
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
    private IRepositoryBase<LookUpField, Guid>? _lookUpFields;
    private IRepositoryBase<LookUpFiledValues, Guid>? _lookUpFiledValues;
    private IRepositoryBase<RequestTransaction, Guid>? _requestTransactions;
    private IRepositoryBase<RequestTransactionAttachment, Guid>? _requestTransactionAttachments;

    public IRepositoryBase<User, Guid> Users =>
        _users ??= new RepositoryBase<User, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<User, Guid>>(), _httpContextServiceManager);

    public IRepositoryBase<Role, Guid> Roles =>
        _roles ??= new RepositoryBase<Role, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<Role, Guid>>(), _httpContextServiceManager);

    public IRepositoryBase<PermissionEntity, Guid> Permissions =>
        _permissions ??= new RepositoryBase<PermissionEntity, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<PermissionEntity, Guid>>(), _httpContextServiceManager);

    public IRepositoryBase<SubSystemUser, Guid> SubSystemUsers =>
        _subSystemUsers ??= new RepositoryBase<SubSystemUser, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<SubSystemUser, Guid>>(), _httpContextServiceManager);

    public IRepositoryBase<Attachment, Guid> Attachments =>
        _attachments ??= new RepositoryBase<Attachment, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<Attachment, Guid>>(), _httpContextServiceManager);

    public IRepositoryBase<Template, Guid> Templates =>
        _templates ??= new RepositoryBase<Template, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<Template, Guid>>(), _httpContextServiceManager);

    public IRepositoryBase<Request, Guid> Requests =>
        _requests ??= new RepositoryBase<Request, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<Request, Guid>>(), _httpContextServiceManager);

    public IRepositoryBase<Response, Guid> Responses =>
        _responses ??= new RepositoryBase<Response, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<Response, Guid>>(), _httpContextServiceManager);

    public IRepositoryBase<TemplateOwnership, Guid> TemplateOwnerships =>
        _templateOwnerships ??= new RepositoryBase<TemplateOwnership, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<TemplateOwnership, Guid>>(), _httpContextServiceManager);

    public IRepositoryBase<ResponseAttachment, Guid> ResponseAttachments =>
        _responseAttachments ??= new RepositoryBase<ResponseAttachment, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<ResponseAttachment, Guid>>(), _httpContextServiceManager);

    public IRepositoryBase<RequestAttachment, Guid> RequestAttachments =>
        _requestAttachments ??= new RepositoryBase<RequestAttachment, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<RequestAttachment, Guid>>(), _httpContextServiceManager);

    public IRepositoryBase<LookUpField, Guid> LookUpFields =>
        _lookUpFields ??= new RepositoryBase<LookUpField, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<LookUpField, Guid>>(), _httpContextServiceManager);

    public IRepositoryBase<LookUpFiledValues, Guid> LookUpFiledValues =>
        _lookUpFiledValues ??= new RepositoryBase<LookUpFiledValues, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<LookUpFiledValues, Guid>>(), _httpContextServiceManager);

    public IRepositoryBase<RequestTransaction, Guid> RequestTransactions =>
        _requestTransactions ??= new RepositoryBase<RequestTransaction, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<RequestTransaction, Guid>>(), _httpContextServiceManager);

    public IRepositoryBase<RequestTransactionAttachment, Guid> RequestTransactionAttachments =>
        _requestTransactionAttachments ??= new RepositoryBase<RequestTransactionAttachment, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<RequestTransactionAttachment, Guid>>(), _httpContextServiceManager);

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
