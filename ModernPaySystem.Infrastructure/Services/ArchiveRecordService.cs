using FileManager.Abstractions;
using FileManager.Services.Abstraction;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ModernPaySystem.Application.Interfaces;
using ModernPaySystem.Domain.Commons;
using ModernPaySystem.Domain.DTOs;
using ModernPaySystem.Domain.Entities.Archiving;
using ModernPaySystem.Infrastructure.Options;
using System.Linq.Expressions;
using ICSharpCode.SharpZipLib.Zip;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ModernPaySystem.Infrastructure.Services;

public class ArchiveRecordService(
    IUnitOfWork unitOfWork,
    IFilesManagerService filesManagerService,
    IFileManager fileManager,
    IMemoryCache memoryCache,
    IArchiveAuthorizationService archiveAuthorizationService,
    IArchiveDeletionWorkflowService archiveDeletionWorkflowService,
    ISemanticSearchService semanticSearchService,
    IOptions<ArchiveRecordFileUploadOptions> uploadOptions,
    IOptions<ArchiveRecordZipOptions> zipOptions,
    ILogger<ArchiveRecordService> logger,
    IOptions<ServerSettings> serverSettings,
    SystemHealthService healthService,
    IHttpContextServiceManager httpContextServiceManager,
    IArchiveResourceAuthorizationService resourceAuth,
    IAuditLogService auditLogService) : IArchiveRecordService
{
    private const string UploadRootDirectory = "Diwan";
    private const string DefaultUploadsDirectory = "Uploads";
    private const string ZipCachePrefix = "archive-record-zip";
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> QueryLocks = new();

    private ArchiveRecordFileUploadOptions UploadSettings => uploadOptions.Value;
    private ArchiveRecordZipOptions ZipSettings => zipOptions.Value;
    private ServerSettings ServerSettingsValue => serverSettings.Value;
    private SystemHealthService HealthService => healthService;

    private bool CanAutoIndex => ServerSettingsValue.ActivateSemanticSearch
                                 && HealthService.IsOllamaHealthy
                                 && HealthService.IsQdrantHealthy;

    private readonly IAuditLogService _auditLogService = auditLogService;

    private async Task<string> GetDefaultStoragePathAsync()
    {
        var config = await unitOfWork.Context.ArchiveConfigs
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();

        return config?.DefaultPath ?? DefaultUploadsDirectory;
    }

    public async Task<Result<IEnumerable<ArchiveRecordDto>>> GetAllAsync()
    {
        try
        {
            var result = await unitOfWork.ArchiveRecords.GetAllAsync(
                transform: query => query.Include(x => x.Folder)
                                         .Include(x => x.Form)
                                         .Include(x => x.ArchiveRecordTemplateValuesId)!.ThenInclude(x => x!.ArchiveRecordFormInputValues)
                                         .Include(x => x.PhysicalFiles));

            if (result.IsError)
            {
                return result.Errors;
            }

            return result.Value!.Select(x => x.ToDto()).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching archive records");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<ArchiveRecordDto>>> GetPagedAsync(int page, int pageSize)
    {
        try
        {
            if (page <= 0 || pageSize <= 0 || pageSize > 100)
            {
                return ApplicationErrors.InvalidInput;
            }

            var result = await unitOfWork.ArchiveRecords.GetPagedAsync(
                page,
                pageSize,
                transform: query => query.Include(x => x.Folder)
                                         .Include(x => x.Form)
                                         .Include(x => x.ArchiveRecordTemplateValuesId)!.ThenInclude(x => x!.ArchiveRecordFormInputValues)
                                         .Include(x => x.PhysicalFiles));

            if (result.IsError)
            {
                return result.Errors;
            }

            var items = result.Value!.Items.Select(x => x.ToDto()).ToList();
            return new PagedList<ArchiveRecordDto>(items, result.Value.TotalItems, page, pageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching paged archive records, page: {Page}, size: {PageSize}", page, pageSize);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<ArchiveRecordDto>> GetByIdAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return ApplicationErrors.InvalidInput;
            }

            var userId = httpContextServiceManager.GetCurrentUserId();
            var access = await resourceAuth.CanAccessArchiveRecordAsync(userId, id, AccessLevel.View);
            if (access.IsError)
                return access.Errors;
            if (!access.Value)
                return ApplicationErrors.ArchiveRecordAccessDenied;

            var result = await unitOfWork.ArchiveRecords.GetAsync(
                x => x.Id == id,
                query => query.Include(x => x.Folder)
                              .Include(x => x.Form)
                              .Include(x => x.ArchiveRecordTemplateValuesId)!.ThenInclude(x => x!.ArchiveRecordFormInputValues)
                              .Include(x => x.PhysicalFiles));

            if (result.IsError)
            {
                return result.Errors;
            }

            if (result.Value == null)
            {
                return ApplicationErrors.ArchiveRecordNotFound;
            }

            var ipAddress = httpContextServiceManager.GetClientIpAddress();
            var userAgent = httpContextServiceManager.GetUserAgent();
            await _auditLogService.LogAsync(id, userId.ToString(), AuditAction.View, "Viewed archive record", ipAddress, userAgent);

            return result.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching archive record {RecordId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<ArchiveRecordDto>>> GetByFolderIdAsync(Guid folderId, int page, int pageSize)
    {
        try
        {
            if (folderId == Guid.Empty || page <= 0 || pageSize <= 0 || pageSize > 100)
            {
                return ApplicationErrors.InvalidInput;
            }

            var userId = httpContextServiceManager.GetCurrentUserId();
            var access = await resourceAuth.CanAccessFolderAsync(userId, folderId, AccessLevel.View);
            if (access.IsError)
                return access.Errors;
            if (!access.Value)
                return ApplicationErrors.FolderAccessDenied;

            var result = await unitOfWork.ArchiveRecords.GetPagedAsync(
                page,
                pageSize,
                filter: x => x.FolderId == folderId,
                transform: query => query.Include(x => x.Folder)
                                         .Include(x => x.Form)
                                         .Include(x => x.ArchiveRecordTemplateValuesId)!.ThenInclude(x => x!.ArchiveRecordFormInputValues)
                                         .Include(x => x.PhysicalFiles));

            if (result.IsError)
            {
                return result.Errors;
            }

            var items = result.Value!.Items.Select(x => x.ToDto()).ToList();
            return new PagedList<ArchiveRecordDto>(items, result.Value.TotalItems, page, pageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching archive records for folder {FolderId}", folderId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<ArchiveRecordDto>>> GetByFormIdAsync(Guid formId, int page, int pageSize)
    {
        try
        {
            if (formId == Guid.Empty || page <= 0 || pageSize <= 0 || pageSize > 100)
            {
                return ApplicationErrors.InvalidInput;
            }

            var userId = httpContextServiceManager.GetCurrentUserId();
            var accessibleFolderIds = await resourceAuth.GetAccessibleFolderIdsAsync(userId);
            if (accessibleFolderIds.IsError)
                return accessibleFolderIds.Errors;

            var result = await unitOfWork.ArchiveRecords.GetPagedAsync(
                page,
                pageSize,
                filter: x => x.FormId == formId && accessibleFolderIds.Value!.Contains(x.FolderId),
                transform: query => query.Include(x => x.Folder)
                                         .Include(x => x.Form)
                                         .Include(x => x.ArchiveRecordTemplateValuesId)!.ThenInclude(x => x!.ArchiveRecordFormInputValues)
                                         .Include(x => x.PhysicalFiles));

            if (result.IsError)
            {
                return result.Errors;
            }

            var items = result.Value!.Items.Select(x => x.ToDto()).ToList();
            return new PagedList<ArchiveRecordDto>(items, result.Value.TotalItems, page, pageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching archive records for form {FormId}", formId);
            return ApplicationErrors.InternalServerError;
        }
    }


    public async Task<Result<PagedList<ArchiveRecordDto>>> GetPagedAsync(ArchiveRecordPagedFilterDto? filterDto = null)
    {
        try
        {
            var page = filterDto?.Page ?? 1;
            var pageSize = filterDto?.PageSize ?? 10;

            logger.LogInformation("Fetching paged archive records, page: {Page}, size: {PageSize}", page, pageSize);

            if (page <= 0)
                return ApplicationErrors.InvalidInput;
            if (pageSize <= 0 || pageSize > 100)
                return ApplicationErrors.InvalidInput;

            var userId = httpContextServiceManager.GetCurrentUserId();
            var accessibleFolderIdsResult = await resourceAuth.GetAccessibleFolderIdsAsync(userId);
            if (accessibleFolderIdsResult.IsError)
                return accessibleFolderIdsResult.Errors;
            var accessibleFolderIds = accessibleFolderIdsResult.Value!;

            List<Expression<Func<ArchiveRecord, bool>>> filters =
            [
                r => accessibleFolderIds.Contains(r.FolderId)
            ];
            if (filterDto != null)
            {
                if (!string.IsNullOrWhiteSpace(filterDto.SearchText))
                {
                    if (Guid.TryParse(filterDto.SearchText, out var searchId))
                        filters.Add(r => r.Id == searchId);
                }

                if (!string.IsNullOrWhiteSpace(filterDto.RecordId) && Guid.TryParse(filterDto.RecordId, out var recordId))
                    filters.Add(r => r.Id == recordId);

                if (filterDto.InputValueFilters is { Count: > 0 })
                {
                    foreach (var ivf in filterDto.InputValueFilters)
                    {
                        if (!string.IsNullOrWhiteSpace(ivf.Value))
                        {
                            filters.Add(r => r.ArchiveRecordTemplateValuesId != null
                                && r.ArchiveRecordTemplateValuesId.ArchiveRecordFormInputValues
                                    .Any(iv => iv.Key.Contains(ivf.Key) && iv.Value.Contains(ivf.Value)));
                        }
                        else
                        {
                            filters.Add(r => r.ArchiveRecordTemplateValuesId != null
                                && r.ArchiveRecordTemplateValuesId.ArchiveRecordFormInputValues
                                    .Any(iv => iv.Key.Contains(ivf.Key)));
                        }
                    }
                }
            }

            var pagedRecords = await unitOfWork.ArchiveRecords.GetPagedAsync(
                page,
                pageSize,
                transform: query => query.Include(x => x.Folder)
                                         .Include(x => x.Form)
                                         .Include(x => x.ArchiveRecordTemplateValuesId)!.ThenInclude(x => x!.ArchiveRecordFormInputValues)
                                         .Include(x => x.PhysicalFiles),
                additionalFilters: filters,
                logicalOperator: filterDto?.LogicalOperator == FilterLogicalOperator.Or
                    ? ExpressionBuilderLib.src.Core.Enums.LogicalOperator.Or
                    : ExpressionBuilderLib.src.Core.Enums.LogicalOperator.And);

            if (pagedRecords.IsError)
                return pagedRecords.Errors;

            var items = pagedRecords.Value!.Items.Select(x => x.ToDto()).ToList();
            return new PagedList<ArchiveRecordDto>(items, pagedRecords.Value.TotalItems, page, pageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching paged archive records, page: {Page}, size: {PageSize}", filterDto?.Page, filterDto?.PageSize);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<ArchiveRecordDto>> CreateAsync(CreateArchiveRecordDto dto)
    {
        var uploadedPaths = new List<string>();

        try
        {
            var requestValidationResult = ValidateCreateRequest(dto);
            if (requestValidationResult.IsError) return requestValidationResult.Errors;

            var validationResult = ValidateFiles(dto.Files);
            if (validationResult.IsError) return validationResult.Errors;

            var userId = httpContextServiceManager.GetCurrentUserId();
            var folderAccess = await resourceAuth.CanAccessFolderAsync(userId, dto.FolderId, AccessLevel.View);
            if (folderAccess.IsError)
                return folderAccess.Errors;
            if (!folderAccess.Value)
                return ApplicationErrors.FolderAccessDenied;

            var folderResult = await EnsureFolderExistsAsync(dto.FolderId);
            if (folderResult.IsError) return folderResult.Errors;

            var folder = folderResult.Value!;
            var storageSubDir = folder.DefaultStoragePath ?? await GetDefaultStoragePathAsync();

            var folderDepartmentResult = await archiveAuthorizationService.ResolveFolderDepartmentIdAsync(dto.FolderId);
            if (folderDepartmentResult.IsError) return folderDepartmentResult.Errors;

            if (!folderDepartmentResult.Value.HasValue)
            {
                return ApplicationErrors.ArchiveRecordDepartmentNotConfigured;
            }

            var formResolutionResult = await ResolveFormAsync(dto.FormId);
            if (formResolutionResult is not null && formResolutionResult.IsError)
                return formResolutionResult.Errors;

            var record = new ArchiveRecord
            {
                Id = dto.Id ?? Guid.NewGuid(),
                FolderId = dto.FolderId,
                DepartmentId = folderDepartmentResult.Value,
                FormId = formResolutionResult?.Value?.Id
            };

            if (dto.FormId is not null)
            {
                var buildTemplateValuesResult = BuildTemplateValues(record, dto);
                if (buildTemplateValuesResult.IsError) return buildTemplateValuesResult.Errors;
                record.ArchiveRecordTemplateValuesId = buildTemplateValuesResult.Value!;
            }

            var physicalFiles = await StoreFilesAsync(record, storageSubDir, dto.Files, uploadedPaths);
            if (physicalFiles.IsError)
            {
                await CleanupStoredFilesAsync(uploadedPaths);
                return physicalFiles.Errors;
            }
            record.PhysicalFiles = [.. physicalFiles.Value!];

            var dbContext = unitOfWork.Context;
            var executionStrategy = dbContext.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                await unitOfWork.BeginTransactionAsync();
                try
                {
                    var addTemplateValuesResult = await AddTemplateValuesIfPresentAsync(record);
                    if (addTemplateValuesResult.IsError)
                    {
                        await unitOfWork.RollbackTransactionAsync();
                        return addTemplateValuesResult.Errors;
                    }

                    var addRecordResult = await unitOfWork.ArchiveRecords.AddAsync(record);
                    if (addRecordResult.IsError)
                    {
                        await unitOfWork.RollbackTransactionAsync();
                        return addRecordResult.Errors;
                    }

                    foreach (var physicalFile in record.PhysicalFiles)
                    {
                        var addFileResult = await unitOfWork.PhysicalFiles.AddAsync(physicalFile);
                        if (addFileResult.IsError)
                        {
                            await unitOfWork.RollbackTransactionAsync();
                            return addFileResult.Errors;
                        }
                    }

                    var saveResult = await unitOfWork.SaveChangesAsync();
                    if (saveResult <= 0)
                    {
                        await unitOfWork.RollbackTransactionAsync();
                        return ApplicationErrors.DatabaseError;
                    }

                    await unitOfWork.CommitTransactionAsync();

                    if (CanAutoIndex)
                        _ = TryAutoIndexPhysicalFilesAsync(record.PhysicalFiles);

                    var ipAddress = httpContextServiceManager.GetClientIpAddress();
                    var userAgent = httpContextServiceManager.GetUserAgent();
                    await _auditLogService.LogAsync(record.Id, userId.ToString(), AuditAction.Create, "Created archive record", ipAddress, userAgent);

                    return await GetByIdAsync(record.Id);
                }
                catch
                {
                    if (unitOfWork.HasActiveTransaction)
                        await unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            });
        }
        catch (Exception ex)
        {
            if (unitOfWork.HasActiveTransaction)
                await unitOfWork.RollbackTransactionAsync();

            await CleanupStoredFilesAsync(uploadedPaths);
            logger.LogError(ex, "Error creating archive record");
            return ApplicationErrors.InternalServerError;
        }
    }


    private async Task<Result<Folder>> EnsureFolderExistsAsync(Guid folderId)
    {
        var folderResult = await unitOfWork.Folders.GetByIdAsync(folderId);
        if (folderResult.IsError)
        {
            return folderResult.Errors;
        }

        if (folderResult.Value == null)
        {
            return ApplicationErrors.FolderNotFound;
        }

        return folderResult.Value;
    }
    private async Task<Result<HashSet<Guid>>> GetFolderTreeIdsAsync(Guid folderId)
    {
        var foldersResult = await unitOfWork.Context.Folders
            .AsNoTracking()
            .Select(x => new { x.Id, x.ParentId })
            .ToListAsync();

        var folderMap = foldersResult
            .Where(x => x.ParentId.HasValue)
            .GroupBy(x => x.ParentId!.Value)
            .ToDictionary(x => x.Key, x => x.Select(folder => folder.Id).ToList());

        var folderIds = new HashSet<Guid>();
        var stack = new Stack<Guid>();
        stack.Push(folderId);

        while (stack.Count > 0)
        {
            var currentFolderId = stack.Pop();
            if (!folderIds.Add(currentFolderId))
            {
                continue;
            }

            if (!folderMap.TryGetValue(currentFolderId, out var childFolderIds))
            {
                continue;
            }

            foreach (var childFolderId in childFolderIds)
            {
                stack.Push(childFolderId);
            }
        }

        return folderIds;
    }
    private async Task<Result<ArchiveFormTemplate?>> ResolveFormAsync(Guid? formId)
    {
        if (!formId.HasValue)
        {
            return null!;
        }

        var formResult = await unitOfWork.DynamicForms.GetByIdAsync(formId.Value);
        if (formResult.IsError)
        {
            return formResult.Errors;
        }

        if (formResult.Value == null)
        {
            return ApplicationErrors.DynamicFormNotFound;
        }

        return formResult.Value;
    }

    private async Task<Result<Success>> AddTemplateValuesIfPresentAsync(ArchiveRecord record)
    {
        if (record.ArchiveRecordTemplateValuesId == null)
        {
            return Result.Success;
        }

        var addTemplateValuesResult = await unitOfWork.ArchiveRecordTemplateValues.AddAsync(record.ArchiveRecordTemplateValuesId);
        if (addTemplateValuesResult.IsError)
        {
            return addTemplateValuesResult.Errors;
        }

        return Result.Success;
    }

    public async Task<Result<ArchiveRecordDto>> UpdateAsync(Guid id, UpdateArchiveRecordDto dto)
    {
        var uploadedPaths = new List<string>();

        try
        {
            if (id == Guid.Empty || dto == null || dto.FolderId == Guid.Empty || (dto.FormId.HasValue && dto.FormId.Value == Guid.Empty))
            {
                logger.LogWarning("Update request validation failed: Invalid id, folder, or form.");
                return ApplicationErrors.InvalidInput;
            }

            var userId = httpContextServiceManager.GetCurrentUserId();
            var access = await resourceAuth.CanAccessArchiveRecordAsync(userId, id, AccessLevel.View);
            if (access.IsError)
                return access.Errors;
            if (!access.Value)
                return ApplicationErrors.ArchiveRecordAccessDenied;

            dto.Files ??= default!;

            var validationResult = ValidateFiles(dto.Files);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            var recordResult = await unitOfWork.ArchiveRecords.GetAsync(
                x => x.Id == id,
                query => query.Include(x => x.ArchiveRecordTemplateValuesId)!.ThenInclude(x => x!.ArchiveRecordFormInputValues)
                              .Include(x => x.PhysicalFiles));

            if (recordResult.IsError)
            {
                return recordResult.Errors;
            }

            var record = recordResult.Value;
            if (record == null)
            {
                return ApplicationErrors.ArchiveRecordNotFound;
            }

            var folderResult = await unitOfWork.Folders.GetByIdAsync(dto.FolderId);
            if (folderResult.IsError)
            {
                return folderResult.Errors;
            }

            if (folderResult.Value == null)
            {
                return ApplicationErrors.FolderNotFound;
            }

            var storageSubDir = folderResult.Value.DefaultStoragePath ?? await GetDefaultStoragePathAsync();

            var formResolutionResult = await ResolveFormAsync(dto.FormId);
            if (formResolutionResult is not null && formResolutionResult.IsError)
                return formResolutionResult.Errors;

            var filesToRemove = ResolveFilesToRemove(record, dto);
            var newPhysicalFiles = await StoreFilesAsync(record, storageSubDir, dto.Files, uploadedPaths);
            if (newPhysicalFiles.IsError)
            {
                await CleanupStoredFilesAsync(uploadedPaths);
                return newPhysicalFiles.Errors;
            }
            var dbContext = unitOfWork.Context;
            var executionStrategy = dbContext.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                await unitOfWork.BeginTransactionAsync();

                record.FolderId = dto.FolderId;
                record.FormId = dto.FormId;

                if (record.ArchiveRecordTemplateValuesId == null)
                {
                    if (dto.FormId.HasValue)
                    {
                        record.ArchiveRecordTemplateValuesId = BuildTemplateValues(record, dto);
                        var addTemplateValuesResult = await unitOfWork.ArchiveRecordTemplateValues.AddAsync(record.ArchiveRecordTemplateValuesId);
                        if (addTemplateValuesResult.IsError)
                        {
                            await unitOfWork.RollbackTransactionAsync();
                            return addTemplateValuesResult.Errors;
                        }
                    }
                }
                else
                {
                    if (dto.FormId.HasValue)
                    {
                        record.ArchiveRecordTemplateValuesId.ArchiveFormTemplateId = dto.FormId.Value;
                        record.ArchiveRecordTemplateValuesId.ArchiveRecordFormInputValues = [.. dto.Content.Select(x => new ArchiveRecordFormInputValue
                        {
                            Key = x.Key,
                            Value = x.Value
                        })];
                    }
                    else
                    {
                        var removeResult = await unitOfWork.ArchiveRecordTemplateValues.RemoveAsync(x => x.Id == record.ArchiveRecordTemplateValuesId.Id);
                        if (removeResult.IsError)
                        {
                            await unitOfWork.RollbackTransactionAsync();
                            return removeResult.Errors;
                        }
                        record.ArchiveRecordTemplateValuesId = null!;
                    }
                }

                var addFiles = newPhysicalFiles.Value!.ToList();
                record.PhysicalFiles ??= [];
                foreach (var file in addFiles)
                {
                    record.PhysicalFiles.Add(file);
                    var addFileResult = await unitOfWork.PhysicalFiles.AddAsync(file);
                    if (addFileResult.IsError)
                    {
                        await unitOfWork.RollbackTransactionAsync();
                        return addFileResult.Errors;
                    }
                }

                foreach (var file in filesToRemove)
                {
                    var removeResult = await unitOfWork.PhysicalFiles.RemoveAsync(x => x.Id == file.Id);
                    if (removeResult.IsError)
                    {
                        await unitOfWork.RollbackTransactionAsync();
                        return removeResult.Errors;
                    }
                }

                var updateResult = await unitOfWork.ArchiveRecords.UpdateAsync(record);
                if (updateResult.IsError)
                {
                    await unitOfWork.RollbackTransactionAsync();
                    return updateResult.Errors;
                }

                var saveResult = await unitOfWork.SaveChangesAsync();
                if (saveResult <= 0)
                {
                    await unitOfWork.RollbackTransactionAsync();
                    return ApplicationErrors.DatabaseError;
                }

                await unitOfWork.CommitTransactionAsync();

                if (CanAutoIndex)
                    _ = TryAutoIndexPhysicalFilesAsync(newPhysicalFiles.Value!);

                foreach (var file in filesToRemove)
                {
                    var deleteResult = await DeleteStoredFileAsync(file.StoragePath);
                    if (deleteResult.IsError)
                    {
                        logger.LogWarning("Record {RecordId} updated, but file cleanup failed for {Path}: {Error}", id, file.StoragePath, deleteResult.Errors);
                    }
                }

                var ipAddress = httpContextServiceManager.GetClientIpAddress();
                var userAgent = httpContextServiceManager.GetUserAgent();
                await _auditLogService.LogAsync(id, userId.ToString(), AuditAction.Update, "Updated archive record", ipAddress, userAgent);

                return await GetByIdAsync(record.Id);
            });
        }
        catch (Exception ex)
        {
            if (unitOfWork.HasActiveTransaction)
            {
                await unitOfWork.RollbackTransactionAsync();
            }

            await CleanupStoredFilesAsync(uploadedPaths);
            logger.LogError(ex, "Error updating archive record {RecordId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<ArchiveRecordDto>> MoveRecordAsync(Guid id, MoveArchiveRecordDto dto)
    {
        try
        {
            if (id == Guid.Empty || dto == null || dto.DestinationFolderId == Guid.Empty)
            {
                logger.LogWarning("Move record request validation failed: Invalid id or destination folder.");
                return ApplicationErrors.InvalidInput;
            }

            var userId = httpContextServiceManager.GetCurrentUserId();

            var sourceAccess = await resourceAuth.CanAccessArchiveRecordAsync(userId, id, AccessLevel.FullControl);
            if (sourceAccess.IsError)
                return sourceAccess.Errors;
            if (!sourceAccess.Value)
                return ApplicationErrors.ArchiveRecordAccessDenied;

            var destAccess = await resourceAuth.CanAccessFolderAsync(userId, dto.DestinationFolderId, AccessLevel.Write);
            if (destAccess.IsError)
                return destAccess.Errors;
            if (!destAccess.Value)
                return ApplicationErrors.FolderAccessDenied;

            var recordResult = await unitOfWork.ArchiveRecords.GetAsync(
                x => x.Id == id,
                query => query.Include(x => x.PhysicalFiles)
                              .Include(x => x.Folder));
            if (recordResult.IsError)
                return recordResult.Errors;
            var record = recordResult.Value;
            if (record == null)
                return ApplicationErrors.ArchiveRecordNotFound;

            var destFolderResult = await unitOfWork.Folders.GetByIdAsync(dto.DestinationFolderId);
            if (destFolderResult.IsError)
                return destFolderResult.Errors;
            var destFolder = destFolderResult.Value;
            if (destFolder == null)
                return ApplicationErrors.FolderNotFound;

            if (record.FolderId == dto.DestinationFolderId)
                return ApplicationErrors.InvalidInput;

            if (record.DepartmentId.HasValue && destFolder.DepartmentId.HasValue && record.DepartmentId != destFolder.DepartmentId)
                return ApplicationErrors.InvalidInput;

            var oldFolder = record.Folder;
            var defaultPath = await GetDefaultStoragePathAsync();
            var oldSubDir = oldFolder.DefaultStoragePath ?? defaultPath;
            var newSubDir = destFolder.DefaultStoragePath ?? defaultPath;
            var oldRelativeRecordDir = Path.Combine(UploadRootDirectory, "Uploads", oldSubDir, record.Id.ToString());
            var newRelativeRecordDir = Path.Combine(UploadRootDirectory, "Uploads", newSubDir, record.Id.ToString());

            var dbContext = unitOfWork.Context;
            var executionStrategy = dbContext.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                await unitOfWork.BeginTransactionAsync();

                try
                {
                    record.FolderId = dto.DestinationFolderId;
                    record.Folder = destFolder;

                    foreach (var physicalFile in record.PhysicalFiles.Where(pf => !pf.IsDeleted))
                    {
                        var fileName = Path.GetFileName(physicalFile.StoragePath);
                        physicalFile.StoragePath = Path.Combine(newRelativeRecordDir, fileName);
                    }

                    var updateResult = await unitOfWork.ArchiveRecords.UpdateAsync(record);
                    if (updateResult.IsError)
                    {
                        await unitOfWork.RollbackTransactionAsync();
                        return updateResult.Errors;
                    }

                    var saveResult = await unitOfWork.SaveChangesAsync();
                    if (saveResult <= 0)
                    {
                        await unitOfWork.RollbackTransactionAsync();
                        return ApplicationErrors.DatabaseError;
                    }

                    await unitOfWork.CommitTransactionAsync();
                }
                catch
                {
                    await unitOfWork.RollbackTransactionAsync();
                    throw;
                }

                var absoluteOldDir = NormalizePath(oldRelativeRecordDir);
                var absoluteNewDir = NormalizePath(newRelativeRecordDir);

                if (fileManager.DirectoryExists(absoluteOldDir))
                {
                    var moveResult = await fileManager.MoveDirectoryAsync(absoluteOldDir, absoluteNewDir);
                    if (!moveResult.Success)
                    {
                        logger.LogWarning("OS directory move failed for record {RecordId} from {OldDir} to {NewDir}: {Error}",
                            id, absoluteOldDir, absoluteNewDir, moveResult.ErrorMessage);
                    }
                }

                var ipAddress = httpContextServiceManager.GetClientIpAddress();
                var userAgent = httpContextServiceManager.GetUserAgent();
                await _auditLogService.LogAsync(id, userId.ToString(), AuditAction.Move,
                    $"Moved from folder '{oldFolder.Name}' to folder '{destFolder.Name}'", ipAddress, userAgent);

                return await GetByIdAsync(record.Id);
            });
        }
        catch (Exception ex)
        {
            if (unitOfWork.HasActiveTransaction)
            {
                await unitOfWork.RollbackTransactionAsync();
            }

            logger.LogError(ex, "Error moving archive record {RecordId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<ArchiveRecordDto>> AddFilesAsync(Guid id, IFormFileCollection files)
    {
        var uploadedPaths = new List<string>();

        try
        {
            if (id == Guid.Empty || files == null || files.Count == 0)
            {
                return ApplicationErrors.InvalidInput;
            }

            var userId = httpContextServiceManager.GetCurrentUserId();
            var access = await resourceAuth.CanAccessArchiveRecordAsync(userId, id, AccessLevel.View);
            if (access.IsError)
                return access.Errors;
            if (!access.Value)
                return ApplicationErrors.ArchiveRecordAccessDenied;

            var validationResult = ValidateFiles(files);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            var recordResult = await unitOfWork.ArchiveRecords.GetAsync(
                x => x.Id == id,
                query => query.Include(x => x.PhysicalFiles)
                              .Include(x => x.ArchiveRecordTemplateValuesId)!.ThenInclude(x => x!.ArchiveRecordFormInputValues));

            if (recordResult.IsError)
            {
                return recordResult.Errors;
            }

            var record = recordResult.Value;
            if (record == null)
            {
                return ApplicationErrors.ArchiveRecordNotFound;
            }

            var folderResult = await unitOfWork.Folders.GetByIdAsync(record.FolderId);
            if (folderResult.IsError)
            {
                return folderResult.Errors;
            }

            var storageSubDir = folderResult.Value?.DefaultStoragePath ?? await GetDefaultStoragePathAsync();

            var isUploadingQr = files.Any(f => f.FileName.StartsWith("QR_Cover_", StringComparison.OrdinalIgnoreCase) || 
                                               f.FileName.Contains("QR_Cover", StringComparison.OrdinalIgnoreCase));
            if (isUploadingQr)
            {
                var hasQrPage = record.PhysicalFiles.Any(f => f.IsQrPage && !f.IsDeleted);
                if (hasQrPage)
                {
                    return ApplicationErrors.QrPageAlreadyExists;
                }
            }

            var newPhysicalFiles = await StoreFilesAsync(record, storageSubDir, files, uploadedPaths);
            if (newPhysicalFiles.IsError)
            {
                await CleanupStoredFilesAsync(uploadedPaths);
                return newPhysicalFiles.Errors;
            }
            var dbContext = unitOfWork.Context;
            var executionStrategy = dbContext.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                await unitOfWork.BeginTransactionAsync();

                foreach (var file in newPhysicalFiles.Value!)
                {
                    record.PhysicalFiles.Add(file);
                    var addFileResult = await unitOfWork.PhysicalFiles.AddAsync(file);
                    if (addFileResult.IsError)
                    {
                        await unitOfWork.RollbackTransactionAsync();
                        return addFileResult.Errors;
                    }
                }

                var updateResult = await unitOfWork.ArchiveRecords.UpdateAsync(record);
                if (updateResult.IsError)
                {
                    await unitOfWork.RollbackTransactionAsync();
                    return updateResult.Errors;
                }

                var saveResult = await unitOfWork.SaveChangesAsync();
                if (saveResult <= 0)
                {
                    await unitOfWork.RollbackTransactionAsync();
                    return ApplicationErrors.DatabaseError;
                }

                await unitOfWork.CommitTransactionAsync();
                var ipAddress = httpContextServiceManager.GetClientIpAddress();
                var userAgent = httpContextServiceManager.GetUserAgent();
                await _auditLogService.LogAsync(record.Id, userId.ToString(), AuditAction.AddFiles, $"Added {newPhysicalFiles.Value!.Count} file(s) to archive record", ipAddress, userAgent);
                if (CanAutoIndex)
                    _ = TryAutoIndexPhysicalFilesAsync(newPhysicalFiles.Value!);
                return await GetByIdAsync(record.Id);
            });
        }
        catch (Exception ex)
        {
            if (unitOfWork.HasActiveTransaction)
            {
                await unitOfWork.RollbackTransactionAsync();
            }

            await CleanupStoredFilesAsync(uploadedPaths);
            logger.LogError(ex, "Error adding files to archive record {RecordId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> RemoveFileAsync(Guid id, Guid fileId)
    {
        try
        {
            if (id == Guid.Empty || fileId == Guid.Empty)
            {
                return ApplicationErrors.InvalidInput;
            }

            var userId = httpContextServiceManager.GetCurrentUserId();
            var access = await resourceAuth.CanAccessArchiveRecordAsync(userId, id, AccessLevel.View);
            if (access.IsError)
                return access.Errors;
            if (!access.Value)
                return ApplicationErrors.ArchiveRecordAccessDenied;

            var recordResult = await unitOfWork.ArchiveRecords.GetAsync(
                x => x.Id == id,
                query => query.Include(x => x.PhysicalFiles));

            if (recordResult.IsError)
            {
                return recordResult.Errors;
            }

            var record = recordResult.Value;
            if (record == null)
            {
                return ApplicationErrors.ArchiveRecordNotFound;
            }

            var file = record.PhysicalFiles.FirstOrDefault(x => x.Id == fileId && !x.IsDeleted);
            if (file == null)
            {
                return ApplicationErrors.AttachmentNotFound;
            }
            var dbContext = unitOfWork.Context;
            var executionStrategy = dbContext.Database.CreateExecutionStrategy();
            var transactionResult = await executionStrategy.ExecuteAsync(async () =>
            {
                await unitOfWork.BeginTransactionAsync();

                file.IsDeleted = true;
                file.DeletedAt = DateTime.UtcNow;

                var updateFileResult = await unitOfWork.PhysicalFiles.UpdateAsync(file);
                if (updateFileResult.IsError)
                {
                    await unitOfWork.RollbackTransactionAsync();
                    return (Result<bool>)updateFileResult.Errors;
                }

                var updateResult = await unitOfWork.ArchiveRecords.UpdateAsync(record);
                if (updateResult.IsError)
                {
                    await unitOfWork.RollbackTransactionAsync();
                    return (Result<bool>)updateResult.Errors;
                }

                var saveResult = await unitOfWork.SaveChangesAsync();
                if (saveResult <= 0)
                {
                    await unitOfWork.RollbackTransactionAsync();
                    return (Result<bool>)ApplicationErrors.DatabaseError;
                }

                await unitOfWork.CommitTransactionAsync();
                return (Result<bool>)true;
            });
            if (transactionResult.IsError)
            {
                return transactionResult.Errors;
            }

            var ipAddress = httpContextServiceManager.GetClientIpAddress();
            var userAgent = httpContextServiceManager.GetUserAgent();
            await _auditLogService.LogAsync(id, userId.ToString(), AuditAction.RemoveFiles, $"Removed file '{file.FileName}' from archive record", ipAddress, userAgent);

            var deleteResult = await DeleteStoredFileAsync(file.StoragePath);
            if (deleteResult.IsError)
            {
                logger.LogWarning("Archive file metadata removed for {Path}, but storage cleanup failed: {Error}", file.StoragePath, deleteResult.Errors);
                return deleteResult.Errors;
            }

            return true;
        }
        catch (Exception ex)
        {
            if (unitOfWork.HasActiveTransaction)
            {
                await unitOfWork.RollbackTransactionAsync();
            }

            logger.LogError(ex, "Error removing archive file {FileId} from record {RecordId}", fileId, id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<ArchiveFileConsistencyDto>> CheckFileConsistencyAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return ApplicationErrors.InvalidInput;
            }

            var userId = httpContextServiceManager.GetCurrentUserId();
            var access = await resourceAuth.CanAccessArchiveRecordAsync(userId, id, AccessLevel.View);
            if (access.IsError)
                return access.Errors;
            if (!access.Value)
                return ApplicationErrors.ArchiveRecordAccessDenied;

            var recordResult = await unitOfWork.ArchiveRecords.GetAsync(
                x => x.Id == id,
                query => query.Include(x => x.Folder)
                              .Include(x => x.PhysicalFiles));

            if (recordResult.IsError)
            {
                return recordResult.Errors;
            }

            var record = recordResult.Value;
            if (record == null)
            {
                return ApplicationErrors.ArchiveRecordNotFound;
            }

            var report = new ArchiveFileConsistencyDto
            {
                ArchiveRecordId = record.Id
            };

            var defaultPath = await GetDefaultStoragePathAsync();
            var consistencySubDir = record.Folder?.DefaultStoragePath ?? defaultPath;

            foreach (var physicalFile in record.PhysicalFiles.Where(x => !x.IsDeleted))
            {
                var expectedPath = BuildExpectedStoragePath(record, consistencySubDir, physicalFile.FileName, defaultPath);
                if (!NormalizePath(physicalFile.StoragePath).Equals(NormalizePath(expectedPath), StringComparison.OrdinalIgnoreCase))
                {
                    report.MissingStoragePaths.Add(physicalFile.StoragePath);
                }

                if (!filesManagerService.FileExists(NormalizePath(physicalFile.StoragePath)))
                {
                    report.MissingPhysicalFileIds.Add(physicalFile.Id);
                }
            }

            return report;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking file consistency for archive record {RecordId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<ArchiveFileCleanupDto>> CleanupOrphanFilesAsync()
    {
        try
        {
            var defaultPath = await GetDefaultStoragePathAsync();
            var uploadsRoot = GetUploadsRootPath(defaultPath);
            var listing = await fileManager.ListDirectoryAsync(uploadsRoot, includeSubdirectories: true);
            if (!listing.Success)
            {
                return ApplicationErrors.FileOperationFailed(listing.ErrorMessage ?? $"Unable to list {uploadsRoot}");
            }

            var filePathsInDb = await unitOfWork.PhysicalFiles.GetAllAsync();
            if (filePathsInDb.IsError)
            {
                return filePathsInDb.Errors;
            }

            var dbPaths = new HashSet<string>(
                filePathsInDb.Value!.Where(x => !x.IsDeleted).Select(x => NormalizePath(x.StoragePath)),
                StringComparer.OrdinalIgnoreCase);

            var cleanup = new ArchiveFileCleanupDto();

            foreach (var item in listing.Items.Where(x => !x.IsDirectory))
            {
                if (string.IsNullOrWhiteSpace(item.FullName))
                {
                    continue;
                }

                if (dbPaths.Contains(NormalizePath(item.FullName)))
                {
                    continue;
                }

                var deleteResult = await DeleteStoredFileAsync(item.FullName);
                if (deleteResult.IsError)
                {
                    cleanup.FailedStoragePaths.Add(item.FullName);
                    continue;
                }

                cleanup.FilesDeleted++;
                cleanup.DeletedStoragePaths.Add(item.FullName);
            }

            return cleanup;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error cleaning up orphan archive files");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return ApplicationErrors.InvalidInput;
            }

            var userId = httpContextServiceManager.GetCurrentUserId();
            var access = await resourceAuth.CanAccessArchiveRecordAsync(userId, id, AccessLevel.FullControl);
            if (access.IsError)
                return access.Errors;
            if (!access.Value)
                return ApplicationErrors.ArchiveRecordAccessDenied;

            var recordResult = await unitOfWork.ArchiveRecords.GetAsync(
                x => x.Id == id,
                query => query.Include(x => x.PhysicalFiles));

            if (recordResult.IsError)
            {
                return recordResult.Errors;
            }

            var record = recordResult.Value;
            if (record == null)
            {
                return ApplicationErrors.ArchiveRecordNotFound;
            }

            var storedFiles = record.PhysicalFiles.ToList();

            var dbContext = unitOfWork.Context;
            var executionStrategy = dbContext.Database.CreateExecutionStrategy();
            var transactionResult = await executionStrategy.ExecuteAsync(async () =>
            {
                await unitOfWork.BeginTransactionAsync();

                var removeResult = await unitOfWork.ArchiveRecords.RemoveAsync(x => x.Id == id);
                if (removeResult.IsError)
                {
                    await unitOfWork.RollbackTransactionAsync();
                    return (Result<bool>)removeResult.Errors;
                }

                var saveResult = await unitOfWork.SaveChangesAsync();
                if (saveResult <= 0)
                {
                    await unitOfWork.RollbackTransactionAsync();
                    return (Result<bool>)ApplicationErrors.DatabaseError;
                }

                await unitOfWork.CommitTransactionAsync();
                return (Result<bool>)true;
            });
            if (transactionResult.IsError)
            {
                return transactionResult.Errors;
            }

            var ipAddress = httpContextServiceManager.GetClientIpAddress();
            var userAgent = httpContextServiceManager.GetUserAgent();
            await _auditLogService.LogAsync(id, userId.ToString(), AuditAction.Delete, "Deleted archive record", ipAddress, userAgent);

            foreach (var physicalFile in storedFiles.Where(x => !x.IsDeleted))
            {
                var deleteResult = await DeleteStoredFileAsync(physicalFile.StoragePath);
                if (deleteResult.IsError)
                {
                    logger.LogWarning("Archive record {RecordId} deleted, but file cleanup failed for {Path}: {Error}", id, physicalFile.StoragePath, deleteResult.Errors);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            if (unitOfWork.HasActiveTransaction)
            {
                await unitOfWork.RollbackTransactionAsync();
            }

            logger.LogError(ex, "Error deleting archive record {RecordId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    private Result<Success> ValidateCreateRequest(CreateArchiveRecordDto dto)
    {
        if (dto == null)
        {
            logger.LogWarning("Create request validation failed: DTO is null.");
            return ApplicationErrors.InvalidInput;
        }

        if (dto.FolderId == Guid.Empty)
        {
            logger.LogWarning("Create request validation failed: FolderId is Guid.Empty.");
            return ApplicationErrors.InvalidInput;
        }

        if (dto.FormId.HasValue && dto.FormId.Value == Guid.Empty)
        {
            logger.LogWarning("Create request validation failed: FormId is Guid.Empty.");
            return ApplicationErrors.InvalidInput;
        }

        return Result.Success;
    }

    private Result<Success> ValidateFiles(IFormFileCollection? files)
    {
        if (files == null)
        {
            return Result.Success;
        }

        var rejectedFileNames = new List<string>();

        foreach (var file in files)
        {
            if (file == null || file.Length <= 0)
            {
                logger.LogWarning("File validation failed: File is null or has length <= 0. Name: {FileName}", file?.FileName ?? "Unknown");
                return ApplicationErrors.InvalidInput;
            }

            //if (file.Length > UploadSettings.MaxFileSize)
            //{
            //    logger.LogWarning("File validation failed: File exceeds max size limit. Name: {FileName}, Size: {Size}, MaxSize: {MaxSize}", file.FileName, file.Length, UploadSettings.MaxFileSize);
            //    return ApplicationErrors.AttachmentTooLarge;
            //}

            var extension = Path.GetExtension(file.FileName);
            if (!filesManagerService.IsValidFileExtension(extension, UploadSettings.AllowedExtensions))
            {
                logger.LogWarning("File validation failed: File extension is not allowed. Name: {FileName}, Extension: {Extension}", file.FileName, extension);
                rejectedFileNames.Add(file.FileName);
            }
        }

        if (rejectedFileNames.Count > 0)
        {
            return ApplicationErrors.InvalidAttachmentType(rejectedFileNames);
        }

        return Result.Success;
    }

    private Result<ArchiveRecordTemplateValues> BuildTemplateValues(ArchiveRecord record, CreateArchiveRecordDto dto)
    {
        if (dto.FormId is null)
        {
            return ApplicationErrors.FormIdMustHasValue;
        }
        return new ArchiveRecordTemplateValues
        {
            Id = Guid.NewGuid(),
            ArchiveRecordId = record.Id,
            ArchiveFormTemplateId = dto.FormId.Value,
            ArchiveRecord = record,
            ArchiveRecordFormInputValues = [.. dto.Content.Select(x => new ArchiveRecordFormInputValue
            {
                Key = x.Key,
                Value = x.Value
            })]
        };
    }

    private ArchiveRecordTemplateValues BuildTemplateValues(ArchiveRecord record, UpdateArchiveRecordDto dto)
    {
        return new ArchiveRecordTemplateValues
        {
            Id = Guid.NewGuid(),
            ArchiveRecordId = record.Id,
            ArchiveFormTemplateId = dto.FormId!.Value,
            ArchiveRecord = record,
            ArchiveRecordFormInputValues = [.. dto.Content.Select(x => new ArchiveRecordFormInputValue
            {
                Key = x.Key,
                Value = x.Value
            })]
        };
    }

    private List<PhysicalFile> ResolveFilesToRemove(ArchiveRecord record, UpdateArchiveRecordDto dto)
    {
        if (dto.ReplaceFiles)
        {
            return record.PhysicalFiles.ToList();
        }

        if (dto.FileIdsToRemove.Count == 0)
        {
            return [];
        }

        var targetIds = dto.FileIdsToRemove.ToHashSet();
        return record.PhysicalFiles.Where(x => targetIds.Contains(x.Id)).ToList();
    }

    public async Task<Result<ArchiveRecordFilesMetadataPageDto>> GetFilesMetadataByRecordIdAsync(Guid recordId, int page = 1, int pageSize = 10, bool includeDeleted = false)
    {
        try
        {
            if (recordId == Guid.Empty || page <= 0 || pageSize <= 0 || pageSize > 100)
            {
                return ApplicationErrors.InvalidInput;
            }

            var userId = httpContextServiceManager.GetCurrentUserId();
            var access = await resourceAuth.CanAccessArchiveRecordAsync(userId, recordId, AccessLevel.View);
            if (access.IsError)
                return access.Errors;
            if (!access.Value)
                return ApplicationErrors.ArchiveRecordAccessDenied;

            var recordExists = await unitOfWork.ArchiveRecords.AnyAsync(x => x.Id == recordId);
            if (!recordExists)
            {
                return ApplicationErrors.ArchiveRecordNotFound;
            }

            var metadataResult = await unitOfWork.PhysicalFiles.GetPagedProjectedAsync(
                page,
                pageSize,
                filter: x => x.ArchiveRecordId == recordId && (includeDeleted || !x.IsDeleted),
                selector: x => new PhysicalFileMetadataDto
                {
                    Id = x.Id,
                    OriginalFileName = x.FileName,
                    FileSize = x.FileSize,
                    ContentType = x.ContentType,
                    IsQrPage = x.IsQrPage,
                    CreatedAt = x.CreatedAt,
                    StoragePath = x.StoragePath,
                    ArchiveRecordId = x.ArchiveRecordId
                });

            if (metadataResult.IsError)
            {
                return metadataResult.Errors;
            }

            return new ArchiveRecordFilesMetadataPageDto
            {
                RecordId = recordId,
                TotalCount = metadataResult.Value!.TotalItems,
                Files = metadataResult.Value.Items.ToList()
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching physical file metadata for archive record {RecordId}", recordId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<ArchivePhysicalFileDownloadDto>> GetPhysicalFileStreamAsync(Guid fileId, Guid? recordId = null, bool includeDeleted = false, bool isDownload = false)
    {
        try
        {
            if (fileId == Guid.Empty || (recordId.HasValue && recordId.Value == Guid.Empty))
            {
                return ApplicationErrors.InvalidInput;
            }

            var userId = httpContextServiceManager.GetCurrentUserId();
            var access = await resourceAuth.CanAccessPhysicalFileAsync(userId, fileId, AccessLevel.View);
            if (access.IsError)
                return access.Errors;
            if (!access.Value)
                return ApplicationErrors.PhysicalFileAccessDenied;

            var fileResult = await unitOfWork.PhysicalFiles.GetAsync(
                x => x.Id == fileId && (includeDeleted || !x.IsDeleted));

            if (fileResult.IsError)
            {
                return fileResult.Errors;
            }

            var physicalFile = fileResult.Value;
            if (physicalFile == null)
            {
                return ApplicationErrors.AttachmentNotFound;
            }

            if (!filesManagerService.FileExists(NormalizePath(physicalFile.StoragePath)))
            {
                return ApplicationErrors.ArchivePhysicalFileMissingFromStorage(physicalFile.StoragePath);
            }
            var absolutePath = NormalizePath(physicalFile.StoragePath);

            var streamResult = await filesManagerService.GetFileStreamAsync(absolutePath);
            if (streamResult.IsError)
            {
                if (!filesManagerService.FileExists(absolutePath))
                {
                    return ApplicationErrors.ArchivePhysicalFileMissingFromStorage(physicalFile.StoragePath);
                }

                return streamResult.Errors;
            }

            var contentType = string.IsNullOrWhiteSpace(physicalFile.ContentType)
                ? filesManagerService.GetContentType(physicalFile.FileExtension)
                : physicalFile.ContentType;

            var ipAddress = httpContextServiceManager.GetClientIpAddress();
            var userAgent = httpContextServiceManager.GetUserAgent();
            var auditAction = isDownload ? AuditAction.Download : AuditAction.View;
            var auditDetails = isDownload
                ? $"Downloaded file: {physicalFile.FileName}"
                : $"Viewed file: {physicalFile.FileName}";
            await _auditLogService.LogAsync(physicalFile.ArchiveRecordId, userId.ToString(), auditAction, auditDetails, ipAddress, userAgent);

            return new ArchivePhysicalFileDownloadDto
            {
                FileId = physicalFile.Id,
                ArchiveRecordId = physicalFile.ArchiveRecordId,
                FileName = physicalFile.FileName,
                IsQrPage = physicalFile.IsQrPage,
                ContentType = contentType,
                ContentLength = physicalFile.FileSize,
                ContentStream = streamResult.Value!
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving physical file stream for file {FileId}, record {RecordId}", fileId, recordId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedFileResult<ArchivePhysicalFilePageItemDto>>> GetPaginatedFilesAsync(
        Guid recordId,
        int page = 1,
        int pageSize = 10,
        ArchiveFileRetrievalMode mode = ArchiveFileRetrievalMode.MetadataOnly,
        ArchiveFileSortBy sortBy = ArchiveFileSortBy.CreatedAt,
        ArchiveFileSortOrder sortOrder = ArchiveFileSortOrder.Desc,
        string? searchTerm = null,
        IReadOnlyCollection<string>? fileTypes = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (recordId == Guid.Empty || page <= 0 || pageSize <= 0 || pageSize > 100)
            {
                return ApplicationErrors.InvalidInput;
            }

            var userId = httpContextServiceManager.GetCurrentUserId();
            var access = await resourceAuth.CanAccessArchiveRecordAsync(userId, recordId, AccessLevel.View);
            if (access.IsError)
                return access.Errors;
            if (!access.Value)
                return ApplicationErrors.ArchiveRecordAccessDenied;

            var normalizedSearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim().ToLowerInvariant();
            var normalizedFileTypes = fileTypes?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLowerInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var cacheKey = BuildPagedFilesCacheKey(recordId, page, pageSize, mode, sortBy, sortOrder, normalizedSearchTerm, normalizedFileTypes);
            if (memoryCache.TryGetValue<PagedFileResult<ArchivePhysicalFilePageItemDto>>(cacheKey, out var cachedResult) && cachedResult is not null)
            {
                return cachedResult;
            }

            var queryLock = QueryLocks.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));
            await queryLock.WaitAsync(cancellationToken);

            try
            {
                if (memoryCache.TryGetValue<PagedFileResult<ArchivePhysicalFilePageItemDto>>(cacheKey, out cachedResult) && cachedResult is not null)
                {
                    return cachedResult;
                }

                var allFilesResult = await unitOfWork.PhysicalFiles.GetAllAsync(
                    filter: x => x.ArchiveRecordId == recordId && !x.IsDeleted,
                    transform: query => query.AsNoTracking());

                if (allFilesResult.IsError)
                {
                    return allFilesResult.Errors;
                }

                var filteredFiles = allFilesResult.Value!
                    .Where(file => normalizedSearchTerm == null || file.FileName.ToLowerInvariant().Contains(normalizedSearchTerm))
                    .Where(file => normalizedFileTypes == null || normalizedFileTypes.Length == 0 || normalizedFileTypes.Contains(file.ContentType.ToLowerInvariant()))
                    .ToList();

                var summary = BuildPagedFileSummary(recordId, filteredFiles);
                var sortedFiles = SortPagedFiles(filteredFiles, sortBy, sortOrder);

                var totalCount = sortedFiles.Count;
                var pageItems = new List<ArchivePhysicalFilePageItemDto>();
                foreach (var file in sortedFiles.Skip((page - 1) * pageSize).Take(pageSize))
                {
                    pageItems.Add(await BuildPagedFileItemAsync(file, mode));
                }

                var pagedResult = new PagedFileResult<ArchivePhysicalFilePageItemDto>
                {
                    RecordId = recordId,
                    Items = pageItems,
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalSize = summary.TotalSize,
                    AverageSize = summary.AverageSize,
                    FileTypeBreakdown = summary.FileTypeBreakdown
                };

                memoryCache.Set(cacheKey, pagedResult, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(Math.Max(1, ZipSettings.CacheExpirationMinutes))
                });

                return pagedResult;
            }
            finally
            {
                queryLock.Release();
            }
        }
        catch (OperationCanceledException)
        {
            return ApplicationErrors.InternalServerError;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching paginated archive files for record {RecordId}", recordId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<ArchiveRecordZipBundleDto>> GetZipBundleAsync(Guid recordId, bool flatten = false, string? password = null, CompressionLevel compression = CompressionLevel.Optimal, bool includeMetadata = false, CancellationToken cancellationToken = default)
    {
        try
        {
            if (recordId == Guid.Empty)
            {
                return ApplicationErrors.InvalidInput;
            }

            var userId = httpContextServiceManager.GetCurrentUserId();
            var access = await resourceAuth.CanAccessArchiveRecordAsync(userId, recordId, AccessLevel.View);
            if (access.IsError)
                return access.Errors;
            if (!access.Value)
                return ApplicationErrors.ArchiveRecordAccessDenied;

            var normalizedPassword = string.IsNullOrWhiteSpace(password) ? null : password;
            var recordResult = await unitOfWork.ArchiveRecords.GetAsync(
                x => x.Id == recordId,
                query => query.Include(x => x.Folder)
                              .Include(x => x.PhysicalFiles));

            if (recordResult.IsError)
            {
                return recordResult.Errors;
            }

            var record = recordResult.Value;
            if (record == null)
            {
                return ApplicationErrors.ArchiveRecordNotFound;
            }

            var activeFiles = record.PhysicalFiles.Where(x => !x.IsDeleted).ToList();
            if (activeFiles.Count == 0)
            {
                return ApplicationErrors.ArchiveRecordHasNoFiles;
            }

            foreach (var physicalFile in activeFiles)
            {
                if (!filesManagerService.FileExists(NormalizePath(physicalFile.StoragePath)))
                {
                    return ApplicationErrors.ArchivePhysicalFileMissingFromStorage(physicalFile.StoragePath);
                }
            }

            var totalSize = activeFiles.Sum(x => x.FileSize);
            if (ZipSettings.MaxTotalSizeBytes > 0 && totalSize > ZipSettings.MaxTotalSizeBytes)
            {
                return ApplicationErrors.ArchiveRecordZipTooLarge;
            }

            var cacheKey = BuildZipCacheKey(recordId, flatten, normalizedPassword, compression, includeMetadata);
            if (memoryCache.TryGetValue<CachedZipBundle>(cacheKey, out var cachedBundle) && cachedBundle != null && File.Exists(cachedBundle.ZipFilePath))
            {
                return new ArchiveRecordZipBundleDto
                {
                    ArchiveRecordId = recordId,
                    ZipFilePath = cachedBundle.ZipFilePath,
                    DownloadFileName = cachedBundle.DownloadFileName,
                    ContentLength = cachedBundle.ContentLength,
                    ContentType = "application/zip"
                };
            }

            if (cachedBundle is not null)
            {
                RemoveCachedZip(cacheKey, cachedBundle.ZipFilePath);
            }

            var generationTimeout = TimeSpan.FromSeconds(Math.Max(1, ZipSettings.GenerationTimeoutSeconds));
            using var timeoutCts = new CancellationTokenSource(generationTimeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            var tempDirectory = Path.Combine(Path.GetTempPath(), ZipSettings.CacheDirectoryName);
            Directory.CreateDirectory(tempDirectory);

            var downloadFileName = BuildZipDownloadFileName(record.Id);
            var tempZipPath = Path.Combine(tempDirectory, BuildZipCacheFileName(cacheKey));

            await GenerateZipBundleAsync(record, activeFiles, tempZipPath, flatten, normalizedPassword, compression, includeMetadata, linkedCts.Token);

            var contentLength = new FileInfo(tempZipPath).Length;
            var bundle = new CachedZipBundle(tempZipPath, downloadFileName, contentLength);

            var entryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(Math.Max(1, ZipSettings.CacheExpirationMinutes))
            };

            entryOptions.RegisterPostEvictionCallback(static (_, value, _, _) =>
            {
                if (value is CachedZipBundle zipBundle)
                {
                    RemoveZipFile(zipBundle.ZipFilePath);
                }
            });

            memoryCache.Set(cacheKey, bundle, entryOptions);

            var ipAddress = httpContextServiceManager.GetClientIpAddress();
            var userAgent = httpContextServiceManager.GetUserAgent();
            await _auditLogService.LogAsync(recordId, userId.ToString(), AuditAction.Download, "Downloaded archive record as ZIP", ipAddress, userAgent);

            return new ArchiveRecordZipBundleDto
            {
                ArchiveRecordId = recordId,
                ZipFilePath = bundle.ZipFilePath,
                DownloadFileName = bundle.DownloadFileName,
                ContentLength = bundle.ContentLength,
                ContentType = "application/zip"
            };
        }
        catch (OperationCanceledException)
        {
            return ApplicationErrors.ArchiveRecordZipGenerationTimedOut;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating zip bundle for archive record {RecordId}", recordId);
            return ApplicationErrors.InternalServerError;
        }
    }

    private async Task GenerateZipBundleAsync(ArchiveRecord record, IReadOnlyCollection<PhysicalFile> files, string zipFilePath, bool flatten, string? password, CompressionLevel compression, bool includeMetadata, CancellationToken cancellationToken)
    {
        await using var zipFileStream = new FileStream(
            zipFilePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            64 * 1024,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        using var zipOutputStream = new ZipOutputStream(zipFileStream)
        {
            IsStreamOwner = false
        };

        zipOutputStream.SetLevel(MapCompressionLevel(compression));
        if (!string.IsNullOrWhiteSpace(password))
        {
            zipOutputStream.Password = password;
        }

        var usedEntryNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var entryPrefix = flatten ? string.Empty : $"record-{record.Id.ToString("N")[..8]}/";

        if (includeMetadata)
        {
            await WriteMetadataEntryAsync(zipOutputStream, record, files, entryPrefix, usedEntryNames, cancellationToken);
        }

        foreach (var physicalFile in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var entryName = BuildZipEntryName(entryPrefix, physicalFile.FileName, usedEntryNames);
            var zipEntry = new ZipEntry(entryName)
            {
                DateTime = physicalFile.CreatedAt ?? DateTime.UtcNow,
                CompressionMethod = compression == CompressionLevel.NoCompression ? CompressionMethod.Stored : CompressionMethod.Deflated
            };

            zipOutputStream.PutNextEntry(zipEntry);
            var absolutePath = NormalizePath(physicalFile.StoragePath);
            var streamResult = await filesManagerService.GetFileStreamAsync(absolutePath);
            if (streamResult.IsError)
            {
                throw new FileNotFoundException($"File missing from storage: {absolutePath}", absolutePath);
            }

            await using var sourceStream = streamResult.Value!;
            await sourceStream.CopyToAsync(zipOutputStream, 64 * 1024, cancellationToken);
            zipOutputStream.CloseEntry();
        }

        zipOutputStream.Finish();
        await zipFileStream.FlushAsync(cancellationToken);
    }

    private async Task WriteMetadataEntryAsync(ZipOutputStream zipOutputStream, ArchiveRecord record, IReadOnlyCollection<PhysicalFile> files, string entryPrefix, HashSet<string> usedEntryNames, CancellationToken cancellationToken)
    {
        var metadataPayload = new
        {
            recordId = record.Id,
            folderId = record.FolderId,
            folderName = record.Folder?.Name,
            generatedAtUtc = DateTime.UtcNow,
            fileCount = files.Count,
            files = files.Select(file => new
            {
                fileId = file.Id,
                fileName = file.FileName,
                fileExtension = file.FileExtension,
                contentType = file.ContentType,
                contentLength = file.FileSize,
                archiveRecordId = file.ArchiveRecordId
            })
        };

        var metadataBytes = JsonSerializer.SerializeToUtf8Bytes(metadataPayload, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        var metadataEntryName = BuildZipEntryName(entryPrefix, "metadata.json", usedEntryNames);
        var metadataEntry = new ZipEntry(metadataEntryName)
        {
            DateTime = DateTime.UtcNow,
            CompressionMethod = CompressionMethod.Deflated
        };

        zipOutputStream.PutNextEntry(metadataEntry);
        await zipOutputStream.WriteAsync(metadataBytes, cancellationToken);
        zipOutputStream.CloseEntry();
    }

    private static int MapCompressionLevel(CompressionLevel compression)
    {
        return compression switch
        {
            CompressionLevel.NoCompression => 0,
            CompressionLevel.Fastest => 1,
            CompressionLevel.Optimal => 9,
            _ => 9
        };
    }

    private static string BuildZipEntryName(string prefix, string fileName, HashSet<string> usedEntryNames)
    {
        var safeFileName = Path.GetFileName(fileName);
        var entryName = NormalizeZipEntryPath(Path.Combine(prefix, safeFileName));

        if (usedEntryNames.Add(entryName))
        {
            return entryName;
        }

        var directoryName = Path.GetDirectoryName(entryName) ?? string.Empty;
        var baseName = Path.GetFileNameWithoutExtension(safeFileName);
        var extension = Path.GetExtension(safeFileName);

        var suffix = 1;
        while (true)
        {
            var candidateName = string.IsNullOrEmpty(directoryName)
                ? $"{baseName} ({suffix}){extension}"
                : Path.Combine(directoryName, $"{baseName} ({suffix}){extension}");

            candidateName = NormalizeZipEntryPath(candidateName);
            if (usedEntryNames.Add(candidateName))
            {
                return candidateName;
            }

            suffix++;
        }
    }

    private static string NormalizeZipEntryPath(string path)
    {
        return path.Replace('\\', '/').TrimStart('/');
    }

    private static void RemoveZipFile(string zipFilePath)
    {
        try
        {
            if (File.Exists(zipFilePath))
            {
                File.Delete(zipFilePath);
            }
        }
        catch
        {
        }
    }

    private void RemoveCachedZip(string cacheKey, string zipFilePath)
    {
        memoryCache.Remove(cacheKey);
        RemoveZipFile(zipFilePath);
    }

    private string BuildZipCacheKey(Guid recordId, bool flatten, string? password, CompressionLevel compression, bool includeMetadata)
    {
        var passwordHash = string.IsNullOrEmpty(password)
            ? "nopassword"
            : Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));

        return $"{ZipCachePrefix}:{recordId:N}:{flatten}:{compression}:{includeMetadata}:{passwordHash}";
    }

    private static string BuildZipCacheFileName(string cacheKey)
    {
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(cacheKey)));
        return $"{hash}.zip";
    }

    private static string BuildPagedFilesCacheKey(Guid recordId, int page, int pageSize, ArchiveFileRetrievalMode mode, ArchiveFileSortBy sortBy, ArchiveFileSortOrder sortOrder, string? searchTerm, IReadOnlyCollection<string>? fileTypes)
    {
        var fileTypesPart = fileTypes is null || fileTypes.Count == 0 ? "*" : string.Join(',', fileTypes.OrderBy(x => x, StringComparer.OrdinalIgnoreCase));
        return $"archive-record-files:{recordId:N}:{page}:{pageSize}:{mode}:{sortBy}:{sortOrder}:{searchTerm ?? string.Empty}:{fileTypesPart}";
    }

    private static (long TotalSize, double AverageSize, Dictionary<string, int> FileTypeBreakdown) BuildPagedFileSummary(Guid recordId, IReadOnlyCollection<PhysicalFile> files)
    {
        var totalSize = files.Sum(x => x.FileSize);
        var averageSize = files.Count == 0 ? 0 : (double)totalSize / files.Count;

        var fileTypeBreakdown = files
            .GroupBy(x => string.IsNullOrWhiteSpace(x.ContentType) ? x.FileExtension : x.ContentType, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.Count(), StringComparer.OrdinalIgnoreCase);

        return (totalSize, averageSize, fileTypeBreakdown);
    }

    private static List<PhysicalFile> SortPagedFiles(List<PhysicalFile> files, ArchiveFileSortBy sortBy, ArchiveFileSortOrder sortOrder)
    {
        IOrderedEnumerable<PhysicalFile> orderedFiles = sortBy switch
        {
            ArchiveFileSortBy.FileName => sortOrder == ArchiveFileSortOrder.Asc
                ? files.OrderBy(x => x.FileName, StringComparer.OrdinalIgnoreCase).ThenBy(x => x.Id)
                : files.OrderByDescending(x => x.FileName, StringComparer.OrdinalIgnoreCase).ThenByDescending(x => x.Id),
            ArchiveFileSortBy.FileSize => sortOrder == ArchiveFileSortOrder.Asc
                ? files.OrderBy(x => x.FileSize).ThenBy(x => x.Id)
                : files.OrderByDescending(x => x.FileSize).ThenByDescending(x => x.Id),
            _ => sortOrder == ArchiveFileSortOrder.Asc
                ? files.OrderBy(x => x.CreatedAt ?? DateTime.MinValue).ThenBy(x => x.Id)
                : files.OrderByDescending(x => x.CreatedAt ?? DateTime.MinValue).ThenByDescending(x => x.Id)
        };

        return orderedFiles.ToList();
    }

    private async Task<ArchivePhysicalFilePageItemDto> BuildPagedFileItemAsync(PhysicalFile file, ArchiveFileRetrievalMode mode)
    {
        var item = new ArchivePhysicalFilePageItemDto
        {
            Id = file.Id,
            ArchiveRecordId = file.ArchiveRecordId,
            FileName = file.FileName,
            FileExtension = file.FileExtension,
            ContentType = file.ContentType,
            FileSize = file.FileSize,
            IsQrPage = file.IsQrPage,
            CreatedAt = file.CreatedAt,
            UpdatedAt = file.UpdatedAt
        };

        if (mode != ArchiveFileRetrievalMode.MetadataOnly)
        {
            item.DownloadUrl = $"/api/archive-records/{file.ArchiveRecordId}/files/{file.Id}?download=true";
            item.ViewUrl = $"/api/archive-records/{file.ArchiveRecordId}/files/{file.Id}?download=false";
        }

        if (mode == ArchiveFileRetrievalMode.WithData && file.FileSize <= ZipSettings.MaxInlineDataSizeBytes)
        {
            var fileBytesResult = await filesManagerService.GetFileBytesAsync(file.StoragePath);
            if (!fileBytesResult.IsError)
            {
                item.Base64Data = Convert.ToBase64String(fileBytesResult.Value!);
            }
        }

        return item;
    }

    private static string BuildZipDownloadFileName(Guid recordId)
    {
        var shortGuid = recordId.ToString("N")[..8];
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        return $"archive-record-{shortGuid}-{timestamp}.zip";
    }

    private sealed record CachedZipBundle(string ZipFilePath, string DownloadFileName, long ContentLength);

    private async Task<Result<List<PhysicalFile>>> StoreFilesAsync(ArchiveRecord record, string subDirectory, IFormFileCollection? files, List<string> storedPaths)
    {
        var physicalFiles = new List<PhysicalFile>();

        if (files == null)
        {
            return physicalFiles;
        }

        var recordSubDir = Path.Combine(subDirectory, record.Id.ToString());

        foreach (var file in files)
        {
            var storageName = BuildStorageFileName(record.Id, file.FileName);
            var saveResult = await SaveFileWithRetryAsync(file, recordSubDir, storageName);
            if (saveResult.IsError)
            {
                return saveResult.Errors;
            }

            storedPaths.Add(saveResult.Value!.FilePath);

            var isQr = file.FileName.StartsWith("QR_Cover_", StringComparison.OrdinalIgnoreCase) ||
                       file.FileName.Contains("QR_Cover", StringComparison.OrdinalIgnoreCase);

            physicalFiles.Add(new PhysicalFile
            {
                Id = Guid.NewGuid(),
                ArchiveRecordId = record.Id,
                ArchiveRecord = record,
                FileName = Path.GetFileName(file.FileName),
                FileExtension = Path.GetExtension(file.FileName).ToLowerInvariant(),
                StoragePath = saveResult.Value.FilePath,
                FileSize = saveResult.Value.FileSize,
                ContentType = saveResult.Value.ContentType,
                IsDeleted = false,
                IsQrPage = isQr,
                DeletedAt = null
            });
        }

        return physicalFiles;
    }

    private async Task<Result<FileMetadata>> SaveFileWithRetryAsync(IFormFile file, string subDirectory, string storageName)
    {
        Result<FileMetadata>? lastFailure = null;
        var attemptCount = Math.Max(1, UploadSettings.RetryCount);

        for (var attempt = 1; attempt <= attemptCount; attempt++)
        {
            var result = await filesManagerService.SaveFileAsync(file, subDirectory, storageName);
            if (!result.IsError)
            {
                return result;
            }

            lastFailure = result;
            if (attempt < attemptCount)
            {
                await Task.Delay(Math.Max(0, UploadSettings.RetryDelayMilliseconds));
            }
        }

        return lastFailure is not null ? lastFailure.Errors : ApplicationErrors.FailedToUploadAttachment;
    }

    private async Task<Result<bool>> DeleteStoredFileAsync(string storagePath)
    {
        var absolutePath = NormalizePath(storagePath);
        var deleteResult = await filesManagerService.DeleteFileAsync(absolutePath);
        if (deleteResult.IsError)
        {
            return deleteResult.Errors;
        }

        return true;
    }

    private async Task CleanupStoredFilesAsync(IEnumerable<string> storedPaths)
    {
        foreach (var storedPath in storedPaths.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var absolutePath = NormalizePath(storedPath);
            var deleteResult = await filesManagerService.DeleteFileAsync(absolutePath);
            if (deleteResult.IsError)
            {
                logger.LogWarning("Cleanup failed for stored file at path: {Path}. Error: {Error}", absolutePath, deleteResult.Errors);
            }
        }
    }

    private string BuildStorageFileName(Guid recordId, string originalFileName)
    {
        return filesManagerService.GenerateSafeFileName(Path.GetFileName(originalFileName));
    }

    private string GetUploadsRootPath(string defaultPath)
    {
        return Path.Combine(fileManager.RootDirectory, UploadRootDirectory, defaultPath);
    }

    private string BuildExpectedStoragePath(ArchiveRecord record, string subDirectory, string originalFileName, string defaultPath)
    {
        return Path.Combine(UploadRootDirectory, defaultPath, subDirectory, record.Id.ToString(), BuildStorageFileName(record.Id, originalFileName));
    }

    private string NormalizePath(string path)
    {
        if (Path.IsPathRooted(path))
        {
            return Path.GetFullPath(path);
        }

        return Path.GetFullPath(Path.Combine(fileManager.RootDirectory, path));
    }

    private async Task TryAutoIndexPhysicalFilesAsync(IEnumerable<PhysicalFile> files)
    {
        foreach (var file in files)
        {
            var ext = file.FileExtension.ToLowerInvariant();
            if (file.IsQrPage || !IsIndexableExtension(ext))
                continue;

            try
            {
                var result = await semanticSearchService.IndexPhysicalFileAsync(file.Id);
                if (result.IsError)
                {
                    logger.LogWarning("Auto-indexing skipped for file {FileId} ({FileName}): {Error}",
                        file.Id, file.FileName, result.TopError.Description);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Auto-indexing failed for file {FileId} ({FileName})",
                    file.Id, file.FileName);
            }
        }
    }

    private static bool IsIndexableExtension(string extension)
    {
        return extension is ".docx" or ".xlsx" or ".txt" or ".md" or ".pdf" or ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".tiff" or ".tif";
    }

    public async Task<Result<Success>> LogPrintAsync(Guid recordId)
    {
        try
        {
            if (recordId == Guid.Empty)
            {
                return ApplicationErrors.InvalidInput;
            }

            var userId = httpContextServiceManager.GetCurrentUserId();
            var access = await resourceAuth.CanAccessArchiveRecordAsync(userId, recordId, AccessLevel.Read);
            if (access.IsError)
                return access.Errors;
            if (!access.Value)
                return ApplicationErrors.ArchiveRecordAccessDenied;

            var ipAddress = httpContextServiceManager.GetClientIpAddress();
            var userAgent = httpContextServiceManager.GetUserAgent();
            await _auditLogService.LogAsync(recordId, userId.ToString(), AuditAction.Print, "Printed archive record", ipAddress, userAgent);

            return Result.Success;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error logging print action for archive record {RecordId}", recordId);
            return ApplicationErrors.InternalServerError;
        }
    }
}
