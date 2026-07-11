using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using ModernPaySystem.Module.Transaction.Application;
using ModernPaySystem.Module.Transaction.Domain.Entities;
using ModernPaySystem.Module.Transaction.Infrastructure.Persistence;
using ModernPaySystem.SharedKernel.Application.Repos;
using ModernPaySystem.SharedKernel.Application.Services;
using ModernPaySystem.SharedKernel.Domain.Entities;
using ModernPaySystem.SharedKernel.Infrastructure.Persistence;

namespace ModernPaySystem.Module.Transaction.Infrastructure;

public class TransactionUnitOfWork(
    TransactionDbContext dbContext,
    ILogger<TransactionUnitOfWork> logger,
    ILoggerFactory loggerFactory) : ITransactionUnitOfWork
{
    private readonly TransactionDbContext _dbContext = dbContext;
    private readonly ILogger<TransactionUnitOfWork> _logger = logger;
    private readonly ILoggerFactory _loggerFactory = loggerFactory;
    private IDbContextTransaction? _transaction;

    private IRepositoryBase<Request, Guid>? _requests;
    private IRepositoryBase<RequestTemplateValues, Guid>? _requestTemplateValues;
    private IRepositoryBase<RequestRelation, Guid>? _requestRelations;
    private IRepositoryBase<Response, Guid>? _responses;
    private IRepositoryBase<Template, Guid>? _templates;
    private IRepositoryBase<TemplateDepartmentOwnership, Guid>? _templateDepartmentOwnerships;
    private IRepositoryBase<UserTemplateOwnership, Guid>? _userTemplateOwnerships;
    private IRepositoryBase<RequestAttachment, Guid>? _requestAttachments;
    private IRepositoryBase<ResponseAttachment, Guid>? _responseAttachments;
    private IRepositoryBase<RequestTransaction, Guid>? _requestTransactions;
    private IRepositoryBase<RequestTransactionAttachment, Guid>? _requestTransactionAttachments;
    private IRepositoryBase<RequestAuditLog, Guid>? _requestAuditLogs;
    private IRepositoryBase<InputValue, Guid>? _inputValues;
    private IRepositoryBase<Attachment, Guid>? _attachments;
    private IRepositoryBase<LookUpField, Guid>? _lookUpFields;
    private IRepositoryBase<LookUpFiledValues, Guid>? _lookUpFiledValues;
    private IRepositoryBase<DepartmentTemplateNumber, Guid>? _departmentTemplateNumbers;

    public IRepositoryBase<Request, Guid> Requests =>
        _requests ??= new RepositoryBase<Request, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<Request, Guid>>());

    public IRepositoryBase<RequestTemplateValues, Guid> RequestTemplateValues =>
        _requestTemplateValues ??= new RepositoryBase<RequestTemplateValues, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<RequestTemplateValues, Guid>>());

    public IRepositoryBase<RequestRelation, Guid> RequestRelations =>
        _requestRelations ??= new RepositoryBase<RequestRelation, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<RequestRelation, Guid>>());

    public IRepositoryBase<Response, Guid> Responses =>
        _responses ??= new RepositoryBase<Response, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<Response, Guid>>());

    public IRepositoryBase<Template, Guid> Templates =>
        _templates ??= new RepositoryBase<Template, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<Template, Guid>>());

    public IRepositoryBase<TemplateDepartmentOwnership, Guid> TemplateDepartmentOwnerships =>
        _templateDepartmentOwnerships ??= new RepositoryBase<TemplateDepartmentOwnership, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<TemplateDepartmentOwnership, Guid>>());

    public IRepositoryBase<UserTemplateOwnership, Guid> UserTemplateOwnerships =>
        _userTemplateOwnerships ??= new RepositoryBase<UserTemplateOwnership, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<UserTemplateOwnership, Guid>>());

    public IRepositoryBase<RequestAttachment, Guid> RequestAttachments =>
        _requestAttachments ??= new RepositoryBase<RequestAttachment, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<RequestAttachment, Guid>>());

    public IRepositoryBase<ResponseAttachment, Guid> ResponseAttachments =>
        _responseAttachments ??= new RepositoryBase<ResponseAttachment, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<ResponseAttachment, Guid>>());

    public IRepositoryBase<RequestTransaction, Guid> RequestTransactions =>
        _requestTransactions ??= new RepositoryBase<RequestTransaction, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<RequestTransaction, Guid>>());

    public IRepositoryBase<RequestTransactionAttachment, Guid> RequestTransactionAttachments =>
        _requestTransactionAttachments ??= new RepositoryBase<RequestTransactionAttachment, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<RequestTransactionAttachment, Guid>>());

    public IRepositoryBase<RequestAuditLog, Guid> RequestAuditLogs =>
        _requestAuditLogs ??= new RepositoryBase<RequestAuditLog, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<RequestAuditLog, Guid>>());

    public IRepositoryBase<InputValue, Guid> InputValues =>
        _inputValues ??= new RepositoryBase<InputValue, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<InputValue, Guid>>());

    public IRepositoryBase<Attachment, Guid> Attachments =>
        _attachments ??= new RepositoryBase<Attachment, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<Attachment, Guid>>());

    public IRepositoryBase<LookUpField, Guid> LookUpFields =>
        _lookUpFields ??= new RepositoryBase<LookUpField, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<LookUpField, Guid>>());

    public IRepositoryBase<LookUpFiledValues, Guid> LookUpFiledValues =>
        _lookUpFiledValues ??= new RepositoryBase<LookUpFiledValues, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<LookUpFiledValues, Guid>>());

    public IRepositoryBase<DepartmentTemplateNumber, Guid> DepartmentTemplateNumbers =>
        _departmentTemplateNumbers ??= new RepositoryBase<DepartmentTemplateNumber, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<DepartmentTemplateNumber, Guid>>());

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
