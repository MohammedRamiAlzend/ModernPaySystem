using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using ModernPaySystem.Module.Archive.Application;
using ModernPaySystem.Module.Archive.Domain.Entities;
using ModernPaySystem.Module.Archive.Infrastructure.Persistence;
using ModernPaySystem.SharedKernel.Application.Repos;
using ModernPaySystem.SharedKernel.Infrastructure.Persistence;

namespace ModernPaySystem.Module.Archive.Infrastructure;

public class ArchiveUnitOfWork(
    ArchiveDbContext dbContext,
    ILogger<ArchiveUnitOfWork> logger,
    ILoggerFactory loggerFactory) : IArchiveUnitOfWork
{
    private readonly ArchiveDbContext _dbContext = dbContext;
    private readonly ILogger<ArchiveUnitOfWork> _logger = logger;
    private readonly ILoggerFactory _loggerFactory = loggerFactory;
    private IDbContextTransaction? _transaction;

    private IRepositoryBase<Folder, Guid>? _folders;
    private IRepositoryBase<DepartmentArchiveLeader, Guid>? _departmentArchiveLeaders;
    private IRepositoryBase<DeleteArchiveRequest, Guid>? _deleteArchiveRequests;
    private IRepositoryBase<EditArchiveRequest, Guid>? _editArchiveRequests;
    private IRepositoryBase<ArchiveFormTemplate, Guid>? _dynamicForms;
    private IRepositoryBase<ArchiveRecord, Guid>? _archiveRecords;
    private IRepositoryBase<ArchiveRecordTemplateValues, Guid>? _archiveRecordTemplateValues;
    private IRepositoryBase<ArchiveRecordFormInputValue, Guid>? _archiveRecordFormInputValues;
    private IRepositoryBase<PhysicalFile, Guid>? _physicalFiles;
    private IRepositoryBase<FolderPermission, Guid>? _folderPermissions;
    private IRepositoryBase<Document, Guid>? _documents;
    private IRepositoryBase<DocumentChunk, Guid>? _documentChunks;
    private IRepositoryBase<ArchiveAuditLog, Guid>? _archiveAuditLogs;
    private IRepositoryBase<ArchiveConfig, Guid>? _archiveConfigs;
    private IRepositoryBase<FolderIcon, Guid>? _folderIcons;

    public IRepositoryBase<Folder, Guid> Folders =>
        _folders ??= new RepositoryBase<Folder, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<Folder, Guid>>());

    public IRepositoryBase<DepartmentArchiveLeader, Guid> DepartmentArchiveLeaders =>
        _departmentArchiveLeaders ??= new RepositoryBase<DepartmentArchiveLeader, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<DepartmentArchiveLeader, Guid>>());

    public IRepositoryBase<DeleteArchiveRequest, Guid> DeleteArchiveRequests =>
        _deleteArchiveRequests ??= new RepositoryBase<DeleteArchiveRequest, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<DeleteArchiveRequest, Guid>>());

    public IRepositoryBase<EditArchiveRequest, Guid> EditArchiveRequests =>
        _editArchiveRequests ??= new RepositoryBase<EditArchiveRequest, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<EditArchiveRequest, Guid>>());

    public IRepositoryBase<ArchiveFormTemplate, Guid> DynamicForms =>
        _dynamicForms ??= new RepositoryBase<ArchiveFormTemplate, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<ArchiveFormTemplate, Guid>>());

    public IRepositoryBase<ArchiveRecord, Guid> ArchiveRecords =>
        _archiveRecords ??= new RepositoryBase<ArchiveRecord, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<ArchiveRecord, Guid>>());

    public IRepositoryBase<ArchiveRecordTemplateValues, Guid> ArchiveRecordTemplateValues =>
        _archiveRecordTemplateValues ??= new RepositoryBase<ArchiveRecordTemplateValues, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<ArchiveRecordTemplateValues, Guid>>());

    public IRepositoryBase<ArchiveRecordFormInputValue, Guid> ArchiveRecordFormInputValues =>
        _archiveRecordFormInputValues ??= new RepositoryBase<ArchiveRecordFormInputValue, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<ArchiveRecordFormInputValue, Guid>>());

    public IRepositoryBase<PhysicalFile, Guid> PhysicalFiles =>
        _physicalFiles ??= new RepositoryBase<PhysicalFile, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<PhysicalFile, Guid>>());

    public IRepositoryBase<FolderPermission, Guid> FolderPermissions =>
        _folderPermissions ??= new RepositoryBase<FolderPermission, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<FolderPermission, Guid>>());

    public IRepositoryBase<Document, Guid> Documents =>
        _documents ??= new RepositoryBase<Document, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<Document, Guid>>());

    public IRepositoryBase<DocumentChunk, Guid> DocumentChunks =>
        _documentChunks ??= new RepositoryBase<DocumentChunk, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<DocumentChunk, Guid>>());

    public IRepositoryBase<ArchiveAuditLog, Guid> ArchiveAuditLogs =>
        _archiveAuditLogs ??= new RepositoryBase<ArchiveAuditLog, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<ArchiveAuditLog, Guid>>());

    public IRepositoryBase<ArchiveConfig, Guid> ArchiveConfigs =>
        _archiveConfigs ??= new RepositoryBase<ArchiveConfig, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<ArchiveConfig, Guid>>());

    public IRepositoryBase<FolderIcon, Guid> FolderIcons =>
        _folderIcons ??= new RepositoryBase<FolderIcon, Guid>(_dbContext, _loggerFactory.CreateLogger<RepositoryBase<FolderIcon, Guid>>());

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
