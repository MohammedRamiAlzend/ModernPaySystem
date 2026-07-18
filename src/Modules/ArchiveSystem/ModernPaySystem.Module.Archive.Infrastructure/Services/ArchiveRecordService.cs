using FileManager.Abstractions;
using FileManager.Services.Abstraction;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModernPaySystem.Module.Archive.Application;
using ModernPaySystem.Module.Archive.Application.Interfaces;
using ModernPaySystem.Module.Archive.Domain;
using ModernPaySystem.Module.Archive.Domain.DTOs;
using ModernPaySystem.Module.Archive.Domain.Entities;
using ModernPaySystem.Module.Archive.Infrastructure.Options;
using ModernPaySystem.Module.Archive.Infrastructure.Persistence;
using ModernPaySystem.SharedKernel.Application.Interfaces;
using ModernPaySystem.SharedKernel.Application.Services;
using ModernPaySystem.SharedKernel.Domain.Commons;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ICSharpCode.SharpZipLib.Zip;

namespace ModernPaySystem.Module.Archive.Infrastructure.Services;

public class ArchiveRecordService(
    IArchiveUnitOfWork unitOfWork,
    ArchiveDbContext dbContext,
    IFilesManagerService filesManagerService,
    IFileManager fileManager,
    IMemoryCache memoryCache,
    IArchiveAuthorizationService archiveAuthorizationService,
    IArchiveResourceAuthorizationService resourceAuth,
    IDepartmentService departmentService,
    IOptions<ArchiveRecordFileUploadOptions> uploadOptions,
    IOptions<ArchiveRecordZipOptions> zipOptions,
    ILogger<ArchiveRecordService> logger,
    IHttpContextServiceManager httpContextServiceManager,
    ISemanticSearchService semanticSearchService,
    IOptions<ServerSettings> serverSettings,
    SystemHealthService healthService,
    IAuditLogService auditLogService) : IArchiveRecordService
{
    private const string UploadRootDirectory = "Diwan";
    private const string DefaultUploadsDirectory = "Uploads";
    private const string ZipCachePrefix = "archive-record-zip";
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> QueryLocks = new();

    private ArchiveRecordFileUploadOptions UploadSettings => uploadOptions.Value;
    private ArchiveRecordZipOptions ZipSettings => zipOptions.Value;
    private ServerSettings ServerSettingsValue => serverSettings.Value;

    private bool CanAutoIndex => ServerSettingsValue.ActivateSemanticSearch
                                 && healthService.IsOllamaHealthy
                                 && healthService.IsQdrantHealthy;

    private async Task<string> GetDefaultStoragePathAsync()
    {
        var config = await dbContext.ArchiveConfigs
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();

        return config?.DefaultPath ?? DefaultUploadsDirectory;
    }

    public async Task<Result<ArchiveRecordDto>> GetByIdAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
                return ArchiveErrors.InvalidInput;

            var userId = httpContextServiceManager.GetCurrentUserId();
            var access = await resourceAuth.CanAccessArchiveRecordAsync(userId, id, AccessLevel.View);
            if (access.IsError)
                return access.Errors;
            if (!access.Value)
                return ArchiveErrors.ArchiveRecordAccessDenied;

            var result = await unitOfWork.ArchiveRecords.GetAsync(
                x => x.Id == id,
                query => query.Include(x => x.Folder)
                              .Include(x => x.Form)
                              .Include(x => x.ArchiveRecordTemplateValuesId)!.ThenInclude(x => x!.ArchiveRecordFormInputValues)
                              .Include(x => x.PhysicalFiles));

            if (result.IsError)
                return result.Errors;

            if (result.Value == null)
                return ArchiveErrors.ArchiveRecordNotFound;

            var ipAddress = httpContextServiceManager.GetClientIpAddress();
            var userAgent = httpContextServiceManager.GetUserAgent();
            await auditLogService.LogAsync(id, userId.ToString(), AuditAction.View, "Viewed archive record", ipAddress, userAgent);

            return result.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching archive record {RecordId}", id);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<ArchiveRecordDto>>> GetByFolderIdAsync(Guid folderId, int page, int pageSize)
    {
        try
        {
            if (folderId == Guid.Empty || page <= 0 || pageSize <= 0 || pageSize > 100)
                return ArchiveErrors.InvalidInput;

            var userId = httpContextServiceManager.GetCurrentUserId();
            var access = await resourceAuth.CanAccessFolderAsync(userId, folderId, AccessLevel.View);
            if (access.IsError)
                return access.Errors;
            if (!access.Value)
                return ArchiveErrors.FolderAccessDenied;

            var result = await unitOfWork.ArchiveRecords.GetPagedAsync(
                page,
                pageSize,
                filter: x => x.FolderId == folderId,
                transform: query => query.Include(x => x.Folder)
                                         .Include(x => x.Form)
                                         .Include(x => x.ArchiveRecordTemplateValuesId)!.ThenInclude(x => x!.ArchiveRecordFormInputValues)
                                         .Include(x => x.PhysicalFiles));

            if (result.IsError)
                return result.Errors;

            var items = result.Value!.Items.Select(x => x.ToDto()).ToList();

            var deptIds = items.Where(i => i.DepartmentId.HasValue).Select(i => i.DepartmentId!.Value).Distinct().ToList();
            var deptNames = new Dictionary<Guid, string>();
            foreach (var dId in deptIds)
            {
                var deptResult = await departmentService.GetByIdAsync(dId);
                if (!deptResult.IsError && deptResult.Value != null)
                    deptNames[dId] = deptResult.Value.Name;
            }
            foreach (var item in items)
            {
                if (item.DepartmentId.HasValue && deptNames.TryGetValue(item.DepartmentId.Value, out var dn))
                    item.DepartmentName = dn;
            }

            return new PagedList<ArchiveRecordDto>(items, result.Value.TotalItems, page, pageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching archive records for folder {FolderId}", folderId);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<ArchiveRecordDto>>> GetByFormIdAsync(Guid formId, int page, int pageSize)
    {
        try
        {
            if (formId == Guid.Empty || page <= 0 || pageSize <= 0 || pageSize > 100)
                return ArchiveErrors.InvalidInput;

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
                return result.Errors;

            var items = result.Value!.Items.Select(x => x.ToDto()).ToList();
            return new PagedList<ArchiveRecordDto>(items, result.Value.TotalItems, page, pageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching archive records for form {FormId}", formId);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<ArchiveRecordDto>>> GetPagedAsync(ArchiveRecordPagedFilterDto? filterDto = null)
    {
        try
        {
            var page = filterDto?.Page ?? 1;
            var pageSize = filterDto?.PageSize ?? 10;

            if (page <= 0 || pageSize <= 0 || pageSize > 100)
                return ArchiveErrors.InvalidInput;

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
                if (!string.IsNullOrWhiteSpace(filterDto.SearchTerm))
                {
                    if (Guid.TryParse(filterDto.SearchTerm, out var searchId))
                        filters.Add(r => r.Id == searchId);
                }

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
            logger.LogError(ex, "Error fetching paged archive records");
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<ArchiveRecordDto>> CreateAsync(CreateArchiveRecordDto dto)
    {
        var uploadedPaths = new List<string>();

        try
        {
            if (dto == null || dto.FolderId == Guid.Empty)
                return ArchiveErrors.InvalidInput;

            if (dto.FormId.HasValue && dto.FormId.Value == Guid.Empty)
                return ArchiveErrors.InvalidInput;

            var allowedExtensions = await GetAllowedExtensionsAsync();
            var validationResult = ValidateFiles(dto.Files, allowedExtensions);
            if (validationResult.IsError)
                return validationResult.Errors;

            var userId = httpContextServiceManager.GetCurrentUserId();
            var folderAccess = await resourceAuth.CanAccessFolderAsync(userId, dto.FolderId, AccessLevel.View);
            if (folderAccess.IsError)
                return folderAccess.Errors;
            if (!folderAccess.Value)
                return ArchiveErrors.FolderAccessDenied;

            var folderResult = await unitOfWork.Folders.GetByIdAsync(dto.FolderId);
            if (folderResult.IsError)
                return folderResult.Errors;
            if (folderResult.Value == null)
                return ArchiveErrors.FolderNotFound;

            var folder = folderResult.Value;
            var storageSubDir = folder.DefaultStoragePath ?? await GetDefaultStoragePathAsync();

            var folderDepartmentResult = await archiveAuthorizationService.ResolveFolderDepartmentIdAsync(dto.FolderId);
            if (folderDepartmentResult.IsError)
                return folderDepartmentResult.Errors;
            if (!folderDepartmentResult.Value.HasValue)
                return ArchiveErrors.ArchiveRecordDepartmentNotConfigured;

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

            record.Name = dto.Name;
            if (string.IsNullOrWhiteSpace(record.Name))
            {
                if (formResolutionResult?.Value != null && dto.Content.Count > 0)
                {
                    var firstVal = dto.Content.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Value))?.Value;
                    record.Name = firstVal != null
                        ? $"{formResolutionResult.Value.FormName} ({firstVal})"
                        : formResolutionResult.Value.FormName;
                }
                else
                {
                    record.Name = $"مستند أرشيفي ({record.Id.ToString()[..8]})";
                }
            }

            if (dto.FormId is not null)
            {
                var buildTemplateValuesResult = BuildTemplateValues(record, dto);
                if (buildTemplateValuesResult.IsError)
                    return buildTemplateValuesResult.Errors;
                record.ArchiveRecordTemplateValuesId = buildTemplateValuesResult.Value!;
            }

            var physicalFiles = await StoreFilesAsync(record, storageSubDir, dto.Files, uploadedPaths);
            if (physicalFiles.IsError)
            {
                await CleanupStoredFilesAsync(uploadedPaths);
                return physicalFiles.Errors;
            }
            record.PhysicalFiles = [.. physicalFiles.Value!];

            var executionStrategy = dbContext.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                await unitOfWork.BeginTransactionAsync();
                try
                {
                    if (record.ArchiveRecordTemplateValuesId != null)
                    {
                        var addTemplateValuesResult = await unitOfWork.ArchiveRecordTemplateValues.AddAsync(record.ArchiveRecordTemplateValuesId);
                        if (addTemplateValuesResult.IsError)
                        {
                            await unitOfWork.RollbackTransactionAsync();
                            return addTemplateValuesResult.Errors;
                        }
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
                        return ArchiveErrors.DatabaseError;
                    }

                    await unitOfWork.CommitTransactionAsync();

                    var ipAddress = httpContextServiceManager.GetClientIpAddress();
                    var userAgent = httpContextServiceManager.GetUserAgent();
                    await auditLogService.LogAsync(record.Id, userId.ToString(), AuditAction.Create, "Created archive record", ipAddress, userAgent);

                    if (CanAutoIndex)
                        _ = TryAutoIndexPhysicalFilesAsync(record.PhysicalFiles);

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
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<ArchiveRecordDto>> UpdateAsync(Guid id, UpdateArchiveRecordDto dto)
    {
        var uploadedPaths = new List<string>();

        try
        {
            if (id == Guid.Empty || dto == null || dto.FolderId == Guid.Empty)
                return ArchiveErrors.InvalidInput;

            var userId = httpContextServiceManager.GetCurrentUserId();
            var access = await resourceAuth.CanAccessArchiveRecordAsync(userId, id, AccessLevel.View);
            if (access.IsError)
                return access.Errors;
            if (!access.Value)
                return ArchiveErrors.ArchiveRecordAccessDenied;

            dto.Files ??= default!;

            var allowedExtensions = await GetAllowedExtensionsAsync();
            var validationResult = ValidateFiles(dto.Files, allowedExtensions);
            if (validationResult.IsError)
                return validationResult.Errors;

            var recordResult = await unitOfWork.ArchiveRecords.GetAsync(
                x => x.Id == id,
                query => query.Include(x => x.ArchiveRecordTemplateValuesId)!.ThenInclude(x => x!.ArchiveRecordFormInputValues)
                              .Include(x => x.PhysicalFiles));

            if (recordResult.IsError)
                return recordResult.Errors;

            var record = recordResult.Value;
            if (record == null)
                return ArchiveErrors.ArchiveRecordNotFound;

            var folderResult = await unitOfWork.Folders.GetByIdAsync(dto.FolderId);
            if (folderResult.IsError)
                return folderResult.Errors;
            if (folderResult.Value == null)
                return ArchiveErrors.FolderNotFound;

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

            var executionStrategy = dbContext.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                await unitOfWork.BeginTransactionAsync();

                record.FolderId = dto.FolderId;
                record.FormId = dto.FormId;
                if (!string.IsNullOrWhiteSpace(dto.Name))
                    record.Name = dto.Name;

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
                    return ArchiveErrors.DatabaseError;
                }

                await unitOfWork.CommitTransactionAsync();

                foreach (var file in filesToRemove)
                {
                    var deleteResult = await DeleteStoredFileAsync(file.StoragePath);
                    if (deleteResult.IsError)
                    {
                        logger.LogWarning("Record {RecordId} updated, but file cleanup failed for {Path}", id, file.StoragePath);
                    }
                }

                var ipAddress = httpContextServiceManager.GetClientIpAddress();
                var userAgent = httpContextServiceManager.GetUserAgent();
                await auditLogService.LogAsync(id, userId.ToString(), AuditAction.Update, "Updated archive record", ipAddress, userAgent);

                if (CanAutoIndex && addFiles.Count > 0)
                    _ = TryAutoIndexPhysicalFilesAsync(addFiles);

                return await GetByIdAsync(record.Id);
            });
        }
        catch (Exception ex)
        {
            if (unitOfWork.HasActiveTransaction)
                await unitOfWork.RollbackTransactionAsync();

            await CleanupStoredFilesAsync(uploadedPaths);
            logger.LogError(ex, "Error updating archive record {RecordId}", id);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<ArchiveRecordDto>> MoveRecordAsync(Guid id, MoveArchiveRecordDto dto)
    {
        try
        {
            if (id == Guid.Empty || dto == null || dto.DestinationFolderId == Guid.Empty)
                return ArchiveErrors.InvalidInput;

            var userId = httpContextServiceManager.GetCurrentUserId();

            var sourceAccess = await resourceAuth.CanAccessArchiveRecordAsync(userId, id, AccessLevel.FullControl);
            if (sourceAccess.IsError)
                return sourceAccess.Errors;
            if (!sourceAccess.Value)
                return ArchiveErrors.ArchiveRecordAccessDenied;

            var destAccess = await resourceAuth.CanAccessFolderAsync(userId, dto.DestinationFolderId, AccessLevel.Write);
            if (destAccess.IsError)
                return destAccess.Errors;
            if (!destAccess.Value)
                return ArchiveErrors.FolderAccessDenied;

            var recordResult = await unitOfWork.ArchiveRecords.GetAsync(
                x => x.Id == id,
                query => query.Include(x => x.PhysicalFiles)
                              .Include(x => x.Folder));
            if (recordResult.IsError)
                return recordResult.Errors;
            var record = recordResult.Value;
            if (record == null)
                return ArchiveErrors.ArchiveRecordNotFound;

            var destFolderResult = await unitOfWork.Folders.GetByIdAsync(dto.DestinationFolderId);
            if (destFolderResult.IsError)
                return destFolderResult.Errors;
            var destFolder = destFolderResult.Value;
            if (destFolder == null)
                return ArchiveErrors.FolderNotFound;

            if (record.FolderId == dto.DestinationFolderId)
                return ArchiveErrors.InvalidInput;

            if (record.DepartmentId.HasValue && destFolder.DepartmentId.HasValue && record.DepartmentId != destFolder.DepartmentId)
                return ArchiveErrors.InvalidInput;

            var oldFolder = record.Folder;
            var defaultPath = await GetDefaultStoragePathAsync();
            var oldSubDir = oldFolder.DefaultStoragePath ?? defaultPath;
            var newSubDir = destFolder.DefaultStoragePath ?? defaultPath;
            var oldRelativeRecordDir = Path.Combine(UploadRootDirectory, "Uploads", oldSubDir, record.Id.ToString());
            var newRelativeRecordDir = Path.Combine(UploadRootDirectory, "Uploads", newSubDir, record.Id.ToString());

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
                        return ArchiveErrors.DatabaseError;
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
                await auditLogService.LogAsync(id, userId.ToString(), AuditAction.Move,
                    $"Moved from folder '{oldFolder.Name}' to folder '{destFolder.Name}'", ipAddress, userAgent);

                return await GetByIdAsync(record.Id);
            });
        }
        catch (Exception ex)
        {
            if (unitOfWork.HasActiveTransaction)
                await unitOfWork.RollbackTransactionAsync();

            logger.LogError(ex, "Error moving archive record {RecordId}", id);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<ArchiveRecordDto>> AddFilesAsync(Guid id, IFormFileCollection files)
    {
        var uploadedPaths = new List<string>();

        try
        {
            if (id == Guid.Empty || files == null || files.Count == 0)
                return ArchiveErrors.InvalidInput;

            var userId = httpContextServiceManager.GetCurrentUserId();
            var access = await resourceAuth.CanAccessArchiveRecordAsync(userId, id, AccessLevel.View);
            if (access.IsError)
                return access.Errors;
            if (!access.Value)
                return ArchiveErrors.ArchiveRecordAccessDenied;

            var allowedExtensions = await GetAllowedExtensionsAsync();
            var validationResult = ValidateFiles(files, allowedExtensions);
            if (validationResult.IsError)
                return validationResult.Errors;

            var recordResult = await unitOfWork.ArchiveRecords.GetAsync(
                x => x.Id == id,
                query => query.Include(x => x.PhysicalFiles)
                              .Include(x => x.ArchiveRecordTemplateValuesId)!.ThenInclude(x => x!.ArchiveRecordFormInputValues));

            if (recordResult.IsError)
                return recordResult.Errors;

            var record = recordResult.Value;
            if (record == null)
                return ArchiveErrors.ArchiveRecordNotFound;

            var folderResult = await unitOfWork.Folders.GetByIdAsync(record.FolderId);
            if (folderResult.IsError)
                return folderResult.Errors;

            var storageSubDir = folderResult.Value?.DefaultStoragePath ?? await GetDefaultStoragePathAsync();

            var isUploadingQr = files.Any(f => f.FileName.StartsWith("QR_Cover_", StringComparison.OrdinalIgnoreCase) ||
                                               f.FileName.Contains("QR_Cover", StringComparison.OrdinalIgnoreCase));
            if (isUploadingQr)
            {
                var hasQrPage = record.PhysicalFiles.Any(f => f.IsQrPage && !f.IsDeleted);
                if (hasQrPage)
                    return ArchiveErrors.QrPageAlreadyExists;
            }

            var newPhysicalFiles = await StoreFilesAsync(record, storageSubDir, files, uploadedPaths);
            if (newPhysicalFiles.IsError)
            {
                await CleanupStoredFilesAsync(uploadedPaths);
                return newPhysicalFiles.Errors;
            }

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
                    return ArchiveErrors.DatabaseError;
                }

                await unitOfWork.CommitTransactionAsync();

                var ipAddress = httpContextServiceManager.GetClientIpAddress();
                var userAgent = httpContextServiceManager.GetUserAgent();
                await auditLogService.LogAsync(record.Id, userId.ToString(), AuditAction.AddFiles,
                    $"Added {newPhysicalFiles.Value!.Count} file(s) to archive record", ipAddress, userAgent);

                if (CanAutoIndex)
                    _ = TryAutoIndexPhysicalFilesAsync(newPhysicalFiles.Value!);

                return await GetByIdAsync(record.Id);
            });
        }
        catch (Exception ex)
        {
            if (unitOfWork.HasActiveTransaction)
                await unitOfWork.RollbackTransactionAsync();

            await CleanupStoredFilesAsync(uploadedPaths);
            logger.LogError(ex, "Error adding files to archive record {RecordId}", id);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> RemoveFileAsync(Guid id, Guid fileId)
    {
        try
        {
            if (id == Guid.Empty || fileId == Guid.Empty)
                return ArchiveErrors.InvalidInput;

            var userId = httpContextServiceManager.GetCurrentUserId();
            var access = await resourceAuth.CanAccessArchiveRecordAsync(userId, id, AccessLevel.View);
            if (access.IsError)
                return access.Errors;
            if (!access.Value)
                return ArchiveErrors.ArchiveRecordAccessDenied;

            var recordResult = await unitOfWork.ArchiveRecords.GetAsync(
                x => x.Id == id,
                query => query.Include(x => x.PhysicalFiles));

            if (recordResult.IsError)
                return recordResult.Errors;

            var record = recordResult.Value;
            if (record == null)
                return ArchiveErrors.ArchiveRecordNotFound;

            var file = record.PhysicalFiles.FirstOrDefault(x => x.Id == fileId && !x.IsDeleted);
            if (file == null)
                return ArchiveErrors.AttachmentNotFound;

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
                    return (Result<bool>)ArchiveErrors.DatabaseError;
                }

                await unitOfWork.CommitTransactionAsync();
                return (Result<bool>)true;
            });

            if (transactionResult.IsError)
                return transactionResult.Errors;

            var deleteResult = await DeleteStoredFileAsync(file.StoragePath);
            if (deleteResult.IsError)
            {
                logger.LogWarning("Archive file metadata removed for {Path}, but storage cleanup failed", file.StoragePath);
                return deleteResult.Errors;
            }

            var ipAddress = httpContextServiceManager.GetClientIpAddress();
            var userAgent = httpContextServiceManager.GetUserAgent();
            await auditLogService.LogAsync(id, userId.ToString(), AuditAction.RemoveFiles,
                $"Removed file '{file.FileName}' from archive record", ipAddress, userAgent);

            return true;
        }
        catch (Exception ex)
        {
            if (unitOfWork.HasActiveTransaction)
                await unitOfWork.RollbackTransactionAsync();

            logger.LogError(ex, "Error removing archive file {FileId} from record {RecordId}", fileId, id);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<ArchivePhysicalFileDownloadDto>> GetPhysicalFileStreamAsync(Guid fileId, Guid? recordId = null, bool includeDeleted = false, bool isDownload = false)
    {
        try
        {
            if (fileId == Guid.Empty || (recordId.HasValue && recordId.Value == Guid.Empty))
                return ArchiveErrors.InvalidInput;

            var userId = httpContextServiceManager.GetCurrentUserId();
            var access = await resourceAuth.CanAccessPhysicalFileAsync(userId, fileId, AccessLevel.View);
            if (access.IsError)
                return access.Errors;
            if (!access.Value)
                return ArchiveErrors.PhysicalFileAccessDenied;

            var fileResult = await unitOfWork.PhysicalFiles.GetAsync(
                x => x.Id == fileId && (includeDeleted || !x.IsDeleted));

            if (fileResult.IsError)
                return fileResult.Errors;

            var physicalFile = fileResult.Value;
            if (physicalFile == null)
                return ArchiveErrors.AttachmentNotFound;

            if (!filesManagerService.FileExists(NormalizePath(physicalFile.StoragePath)))
                return ArchiveErrors.ArchiveRecordNotFound;

            var absolutePath = NormalizePath(physicalFile.StoragePath);

            var streamResult = await filesManagerService.GetFileStreamAsync(absolutePath);
            if (streamResult.IsError)
            {
                if (!filesManagerService.FileExists(absolutePath))
                    return ArchiveErrors.ArchiveRecordNotFound;

                return streamResult.Errors;
            }

            var contentType = string.IsNullOrWhiteSpace(physicalFile.ContentType)
                ? filesManagerService.GetContentType(physicalFile.FileExtension)
                : physicalFile.ContentType;

            var auditAction = isDownload ? AuditAction.Download : AuditAction.View;
            var auditDetails = isDownload ? "Downloaded archive record file" : "Viewed archive record file";
            var ipAddress = httpContextServiceManager.GetClientIpAddress();
            var userAgent = httpContextServiceManager.GetUserAgent();
            await auditLogService.LogAsync(physicalFile.ArchiveRecordId, userId.ToString(), auditAction, auditDetails, ipAddress, userAgent);

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
            logger.LogError(ex, "Error retrieving physical file stream for file {FileId}", fileId);
            return ArchiveErrors.InternalServerError;
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
                return ArchiveErrors.InvalidInput;

            var userId = httpContextServiceManager.GetCurrentUserId();
            var access = await resourceAuth.CanAccessArchiveRecordAsync(userId, recordId, AccessLevel.View);
            if (access.IsError)
                return access.Errors;
            if (!access.Value)
                return ArchiveErrors.ArchiveRecordAccessDenied;

            var normalizedSearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim().ToLowerInvariant();
            var normalizedFileTypes = fileTypes?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLowerInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var cacheKey = BuildPagedFilesCacheKey(recordId, page, pageSize, mode, sortBy, sortOrder, normalizedSearchTerm, normalizedFileTypes);
            if (memoryCache.TryGetValue<PagedFileResult<ArchivePhysicalFilePageItemDto>>(cacheKey, out var cachedResult) && cachedResult is not null)
                return cachedResult;

            var queryLock = QueryLocks.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));
            await queryLock.WaitAsync(cancellationToken);

            try
            {
                if (memoryCache.TryGetValue<PagedFileResult<ArchivePhysicalFilePageItemDto>>(cacheKey, out cachedResult) && cachedResult is not null)
                    return cachedResult;

                var allFilesResult = await unitOfWork.PhysicalFiles.GetAllAsync(
                    filter: x => x.ArchiveRecordId == recordId && !x.IsDeleted,
                    transform: query => query.AsNoTracking());

                if (allFilesResult.IsError)
                    return allFilesResult.Errors;

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
            return ArchiveErrors.InternalServerError;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching paginated archive files for record {RecordId}", recordId);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<ArchiveRecordZipBundleDto>> GetZipBundleAsync(Guid recordId, bool flatten = false, string? password = null, CompressionLevel compression = CompressionLevel.Optimal, bool includeMetadata = false, CancellationToken cancellationToken = default)
    {
        try
        {
            if (recordId == Guid.Empty)
                return ArchiveErrors.InvalidInput;

            var userId = httpContextServiceManager.GetCurrentUserId();
            var access = await resourceAuth.CanAccessArchiveRecordAsync(userId, recordId, AccessLevel.View);
            if (access.IsError)
                return access.Errors;
            if (!access.Value)
                return ArchiveErrors.ArchiveRecordAccessDenied;

            var ipAddress = httpContextServiceManager.GetClientIpAddress();
            var userAgent = httpContextServiceManager.GetUserAgent();
            await auditLogService.LogAsync(recordId, userId.ToString(), AuditAction.Download, "Downloaded archive record as ZIP", ipAddress, userAgent);

            var normalizedPassword = string.IsNullOrWhiteSpace(password) ? null : password;
            var recordResult = await unitOfWork.ArchiveRecords.GetAsync(
                x => x.Id == recordId,
                query => query.Include(x => x.Folder)
                              .Include(x => x.PhysicalFiles));

            if (recordResult.IsError)
                return recordResult.Errors;

            var record = recordResult.Value;
            if (record == null)
                return ArchiveErrors.ArchiveRecordNotFound;

            var activeFiles = record.PhysicalFiles.Where(x => !x.IsDeleted).ToList();
            if (activeFiles.Count == 0)
                return ArchiveErrors.ArchiveRecordHasNoFiles;

            foreach (var physicalFile in activeFiles)
            {
                if (!filesManagerService.FileExists(NormalizePath(physicalFile.StoragePath)))
                    return ArchiveErrors.ArchiveRecordNotFound;
            }

            var totalSize = activeFiles.Sum(x => x.FileSize);
            if (ZipSettings.MaxTotalSizeBytes > 0 && totalSize > ZipSettings.MaxTotalSizeBytes)
                return ArchiveErrors.InternalServerError;

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
                RemoveCachedZip(cacheKey, cachedBundle.ZipFilePath);

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
                    RemoveZipFile(zipBundle.ZipFilePath);
            });

            memoryCache.Set(cacheKey, bundle, entryOptions);

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
            return ArchiveErrors.InternalServerError;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating zip bundle for archive record {RecordId}", recordId);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<ArchiveRecordFilesMetadataPageDto>> GetFilesMetadataByRecordIdAsync(Guid recordId, int page = 1, int pageSize = 10, bool includeDeleted = false)
    {
        try
        {
            if (recordId == Guid.Empty || page <= 0 || pageSize <= 0 || pageSize > 100)
                return ArchiveErrors.InvalidInput;

            var userId = httpContextServiceManager.GetCurrentUserId();
            var access = await resourceAuth.CanAccessArchiveRecordAsync(userId, recordId, AccessLevel.View);
            if (access.IsError)
                return access.Errors;
            if (!access.Value)
                return ArchiveErrors.ArchiveRecordAccessDenied;

            var recordExists = await unitOfWork.ArchiveRecords.AnyAsync(x => x.Id == recordId);
            if (!recordExists)
                return ArchiveErrors.ArchiveRecordNotFound;

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
                return metadataResult.Errors;

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
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<ArchiveFileConsistencyDto>> CheckFileConsistencyAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
                return ArchiveErrors.InvalidInput;

            var userId = httpContextServiceManager.GetCurrentUserId();
            var access = await resourceAuth.CanAccessArchiveRecordAsync(userId, id, AccessLevel.View);
            if (access.IsError)
                return access.Errors;
            if (!access.Value)
                return ArchiveErrors.ArchiveRecordAccessDenied;

            var recordResult = await unitOfWork.ArchiveRecords.GetAsync(
                x => x.Id == id,
                query => query.Include(x => x.Folder)
                              .Include(x => x.PhysicalFiles));

            if (recordResult.IsError)
                return recordResult.Errors;

            var record = recordResult.Value;
            if (record == null)
                return ArchiveErrors.ArchiveRecordNotFound;

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
                    report.MissingStoragePaths.Add(physicalFile.StoragePath);

                if (!filesManagerService.FileExists(NormalizePath(physicalFile.StoragePath)))
                    report.MissingPhysicalFileIds.Add(physicalFile.Id);
            }

            return report;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking file consistency for archive record {RecordId}", id);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
                return ArchiveErrors.InvalidInput;

            var userId = httpContextServiceManager.GetCurrentUserId();
            var access = await resourceAuth.CanAccessArchiveRecordAsync(userId, id, AccessLevel.FullControl);
            if (access.IsError)
                return access.Errors;
            if (!access.Value)
                return ArchiveErrors.ArchiveRecordAccessDenied;

            var recordResult = await unitOfWork.ArchiveRecords.GetAsync(
                x => x.Id == id,
                query => query.Include(x => x.PhysicalFiles));

            if (recordResult.IsError)
                return recordResult.Errors;

            var record = recordResult.Value;
            if (record == null)
                return ArchiveErrors.ArchiveRecordNotFound;

            var storedFiles = record.PhysicalFiles.ToList();

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
                    return (Result<bool>)ArchiveErrors.DatabaseError;
                }

                await unitOfWork.CommitTransactionAsync();
                return (Result<bool>)true;
            });

            if (transactionResult.IsError)
                return transactionResult.Errors;

            foreach (var physicalFile in storedFiles.Where(x => !x.IsDeleted))
            {
                var deleteResult = await DeleteStoredFileAsync(physicalFile.StoragePath);
                if (deleteResult.IsError)
                {
                    logger.LogWarning("Archive record {RecordId} deleted, but file cleanup failed for {Path}", id, physicalFile.StoragePath);
                }
            }

            var ipAddress = httpContextServiceManager.GetClientIpAddress();
            var userAgent = httpContextServiceManager.GetUserAgent();
            await auditLogService.LogAsync(id, userId.ToString(), AuditAction.Delete, "Deleted archive record", ipAddress, userAgent);

            return true;
        }
        catch (Exception ex)
        {
            if (unitOfWork.HasActiveTransaction)
                await unitOfWork.RollbackTransactionAsync();

            logger.LogError(ex, "Error deleting archive record {RecordId}", id);
            return ArchiveErrors.InternalServerError;
        }
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
                return ArchiveErrors.InvalidInput;

            var userId = httpContextServiceManager.GetCurrentUserId();
            var access = await resourceAuth.CanAccessArchiveRecordAsync(userId, recordId, AccessLevel.View);
            if (access.IsError)
                return access.Errors;
            if (!access.Value)
                return ArchiveErrors.ArchiveRecordAccessDenied;

            var ipAddress = httpContextServiceManager.GetClientIpAddress();
            var userAgent = httpContextServiceManager.GetUserAgent();
            await auditLogService.LogAsync(recordId, userId.ToString(), AuditAction.Print, "Printed archive record", ipAddress, userAgent);

            return Result.Success;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error logging print action for archive record {RecordId}", recordId);
            return ArchiveErrors.InternalServerError;
        }
    }

    // Private helpers

    private Result<Success> ValidateFiles(IFormFileCollection? files, string[] allowedExtensions)
    {
        if (files == null)
            return Result.Success;

        var rejectedFileNames = new List<string>();

        foreach (var file in files)
        {
            if (file == null || file.Length <= 0)
                return ArchiveErrors.InvalidInput;

            var extension = Path.GetExtension(file.FileName);
            if (!filesManagerService.IsValidFileExtension(extension, allowedExtensions))
                rejectedFileNames.Add(file.FileName);
        }

        if (rejectedFileNames.Count > 0)
            return ArchiveErrors.InvalidAttachmentType(rejectedFileNames);

        return Result.Success;
    }

    private async Task<string[]> GetAllowedExtensionsAsync()
    {
        var config = await dbContext.ArchiveConfigs
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();

        if (config != null)
        {
            var extensions = config.GetAllowedExtensionsArray();
            if (extensions.Length > 0)
                return extensions;
        }

        return UploadSettings.AllowedExtensions;
    }

    private async Task<Result<ArchiveFormTemplate?>> ResolveFormAsync(Guid? formId)
    {
        if (!formId.HasValue)
            return null!;

        var formResult = await unitOfWork.DynamicForms.GetByIdAsync(formId.Value);
        if (formResult.IsError)
            return formResult.Errors;

        if (formResult.Value == null)
            return ArchiveErrors.DynamicFormNotFound;

        return formResult.Value;
    }

    private Result<ArchiveRecordTemplateValues> BuildTemplateValues(ArchiveRecord record, CreateArchiveRecordDto dto)
    {
        if (dto.FormId is null)
            return ArchiveErrors.InvalidInput;

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
            return record.PhysicalFiles.ToList();

        if (dto.FileIdsToRemove.Count == 0)
            return [];

        var targetIds = dto.FileIdsToRemove.ToHashSet();
        return record.PhysicalFiles.Where(x => targetIds.Contains(x.Id)).ToList();
    }

    private async Task<Result<List<PhysicalFile>>> StoreFilesAsync(ArchiveRecord record, string subDirectory, IFormFileCollection? files, List<string> storedPaths)
    {
        var physicalFiles = new List<PhysicalFile>();

        if (files == null)
            return physicalFiles;

        var recordSubDir = Path.Combine(subDirectory, record.Id.ToString());

        foreach (var file in files)
        {
            var storageName = BuildStorageFileName(record.Id, file.FileName);
            var saveResult = await SaveFileWithRetryAsync(file, recordSubDir, storageName);
            if (saveResult.IsError)
                return saveResult.Errors;

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
                return result;

            lastFailure = result;
            if (attempt < attemptCount)
                await Task.Delay(Math.Max(0, UploadSettings.RetryDelayMilliseconds));
        }

        return lastFailure is not null ? lastFailure.Errors : ArchiveErrors.InternalServerError;
    }

    private async Task<Result<bool>> DeleteStoredFileAsync(string storagePath)
    {
        var absolutePath = NormalizePath(storagePath);
        var deleteResult = await filesManagerService.DeleteFileAsync(absolutePath);
        if (deleteResult.IsError)
            return deleteResult.Errors;

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
                logger.LogWarning("Cleanup failed for stored file at path: {Path}", absolutePath);
            }
        }
    }

    private string BuildStorageFileName(Guid recordId, string originalFileName)
    {
        return filesManagerService.GenerateSafeFileName(Path.GetFileName(originalFileName));
    }

    private string BuildExpectedStoragePath(ArchiveRecord record, string subDirectory, string originalFileName, string defaultPath)
    {
        return Path.Combine(UploadRootDirectory, defaultPath, subDirectory, record.Id.ToString(), BuildStorageFileName(record.Id, originalFileName));
    }

    private string NormalizePath(string path)
    {
        if (Path.IsPathRooted(path))
            return Path.GetFullPath(path);

        return Path.GetFullPath(Path.Combine(fileManager.RootDirectory, path));
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
            zipOutputStream.Password = password;

        var usedEntryNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var entryPrefix = flatten ? string.Empty : $"record-{record.Id.ToString("N")[..8]}/";

        if (includeMetadata)
            await WriteMetadataEntryAsync(zipOutputStream, record, files, entryPrefix, usedEntryNames, cancellationToken);

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
                throw new FileNotFoundException($"File missing from storage: {absolutePath}", absolutePath);

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
            return entryName;

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
                return candidateName;

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
                File.Delete(zipFilePath);
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

    public async Task<Result<PagedList<ArchiveAuditLog>>> GetAuditLogsByDepartmentAsync(
        Guid departmentId,
        int page = 1,
        int pageSize = 50,
        AuditAction? action = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        try
        {
            if (departmentId == Guid.Empty || page <= 0 || pageSize <= 0 || pageSize > 100)
            {
                return ArchiveErrors.InvalidInput;
            }

            var query = from auditLog in dbContext.Set<ArchiveAuditLog>()
                        join archiveRecord in dbContext.Set<ArchiveRecord>().IgnoreQueryFilters()
                            on auditLog.ArchiveRecordId equals archiveRecord.Id
                        where archiveRecord.DepartmentId == departmentId
                        select auditLog;

            if (action.HasValue)
            {
                query = query.Where(x => x.Action == action.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(x => x.Timestamp >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(x => x.Timestamp <= toDate.Value);
            }

            query = query.OrderByDescending(x => x.Timestamp);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedList<ArchiveAuditLog>(items, totalCount, page, pageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting audit logs by department");
            return ArchiveErrors.InternalServerError;
        }
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
                item.Base64Data = Convert.ToBase64String(fileBytesResult.Value!);
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
}