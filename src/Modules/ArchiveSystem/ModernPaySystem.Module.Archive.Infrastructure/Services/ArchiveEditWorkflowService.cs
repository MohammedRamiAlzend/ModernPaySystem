using FileManager.Abstractions;
using FileManager.Services.Abstraction;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernPaySystem.Module.Archive.Application;
using ModernPaySystem.Module.Archive.Application.Interfaces;
using ModernPaySystem.Module.Archive.Domain;
using ModernPaySystem.Module.Archive.Domain.Entities;
using ModernPaySystem.Module.Archive.Infrastructure.Persistence;
using ModernPaySystem.SharedKernel.Application.Services;
using ModernPaySystem.SharedKernel.Domain.Commons;
using System.IO;
using System.Text.Json;

namespace ModernPaySystem.Module.Archive.Infrastructure.Services;

public class ArchiveEditWorkflowService(
    IArchiveUnitOfWork unitOfWork,
    ArchiveDbContext dbContext,
    IHttpContextServiceManager httpContextServiceManager,
    IArchiveAuthorizationService archiveAuthorizationService,
    IArchiveLeaderService archiveLeaderService,
    IFilesManagerService filesManagerService,
    ILogger<ArchiveEditWorkflowService> logger) : IArchiveEditWorkflowService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private const string DefaultUploadsDirectory = "Uploads";

    private async Task<string> GetDefaultStoragePathAsync()
    {
        var config = await dbContext.ArchiveConfigs
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();

        return config?.DefaultPath ?? DefaultUploadsDirectory;
    }

    public async Task<Result<EditArchiveRequestDto>> SubmitRequestAsync(CreateEditArchiveRequestDto dto)
    {
        var uploadedPaths = new List<string>();
        try
        {
            if (dto.ArchiveRecordId == Guid.Empty || string.IsNullOrWhiteSpace(dto.Justification))
                return ArchiveErrors.InvalidInput;

            var recordResult = await unitOfWork.ArchiveRecords.GetAsync(
                x => x.Id == dto.ArchiveRecordId && !x.IsDeleted,
                q => q.Include(r => r.ArchiveRecordTemplateValuesId)
                      .ThenInclude(v => v.ArchiveRecordFormInputValues));

            if (recordResult.IsError || recordResult.Value == null)
                return ArchiveErrors.ArchiveRecordNotFound;

            var record = recordResult.Value;
            var departmentId = record.DepartmentId;
            if (!departmentId.HasValue)
                return ArchiveErrors.ArchiveRecordDepartmentNotConfigured;

            var folderResult = await unitOfWork.Folders.GetByIdAsync(record.FolderId);
            if (folderResult.IsError || folderResult.Value == null)
                return ArchiveErrors.FolderNotFound;

            var storageSubDir = folderResult.Value.DefaultStoragePath ?? await GetDefaultStoragePathAsync();

            var requesterId = httpContextServiceManager.GetCurrentUserId();

            // Check if department archive leaders are assigned
            var leadersResult = await archiveLeaderService.GetByDepartmentAsync(departmentId.Value);
            if (leadersResult.IsError || !leadersResult.Value.Any(l => !l.IsDeleted))
                return ArchiveErrors.DepartmentArchiveLeaderNotAssigned;

            // Original Snapshot JSON
            var originalData = new
            {
                Name = record.Name,
                FormId = record.FormId,
                Content = record.ArchiveRecordTemplateValuesId?.ArchiveRecordFormInputValues
                    .Select(x => new { x.Key, x.Value }).ToList() ?? []
            };
            var originalSnapshotJson = JsonSerializer.Serialize(originalData, JsonOptions);

            var requestedChangesJson = JsonSerializer.Serialize(dto.RequestedChanges, JsonOptions);

            // Validate and store file deletion IDs if any
            string? fileDeletionIdsJson = null;
            if (dto.FileIdsToDelete != null && dto.FileIdsToDelete.Count > 0)
            {
                var distinctIds = dto.FileIdsToDelete.Distinct().ToList();
                var existingFiles = await unitOfWork.PhysicalFiles.GetAllAsync(
                    x => distinctIds.Contains(x.Id)
                      && x.ArchiveRecordId == record.Id
                      && !x.IsDeleted
                      && x.EditArchiveRequestId == null);

                if (existingFiles.IsError || existingFiles.Value == null || existingFiles.Value.Count != distinctIds.Count)
                    return ArchiveErrors.ArchiveRecordFileDeletionNotBelongToRecord;

                fileDeletionIdsJson = JsonSerializer.Serialize(distinctIds, JsonOptions);
            }

            var requestId = Guid.NewGuid();
            var request = new EditArchiveRequest
            {
                Id = requestId,
                DepartmentId = departmentId.Value,
                ArchiveRecordId = dto.ArchiveRecordId,
                RequesterId = requesterId,
                Status = EditArchiveRequestStatus.Pending,
                Justification = dto.Justification,
                RequestedChangesJson = requestedChangesJson,
                RequestedRecordName = dto.RequestedRecordName,
                RequestedFileDeletionIdsJson = fileDeletionIdsJson,
                OriginalSnapshotJson = originalSnapshotJson
            };

            // Store files if any
            var physicalFiles = new List<PhysicalFile>();
            if (dto.Files != null && dto.Files.Count > 0)
            {
                var validFiles = dto.Files.Where(f => f != null && f.Length > 0).ToList();

                // Get allowed extensions from config
                var archiveConfig = await dbContext.ArchiveConfigs
                    .AsNoTracking()
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.Id)
                    .FirstOrDefaultAsync();
                var configExtensions = archiveConfig?.GetAllowedExtensionsArray();
                string[]? allowedExtensions = configExtensions is { Length: > 0 } ? configExtensions : null;

                // Validate all file extensions
                var rejectedFileNames = new List<string>();
                foreach (var file in validFiles)
                {
                    var extension = Path.GetExtension(file.FileName);
                    if (!filesManagerService.IsValidFileExtension(extension, allowedExtensions))
                        rejectedFileNames.Add(file.FileName);
                }

                if (rejectedFileNames.Count > 0)
                    return ArchiveErrors.InvalidAttachmentType(rejectedFileNames);

                foreach (var file in validFiles)
                {
                    var recordSubDir = Path.Combine(storageSubDir, record.Id.ToString());
                    var safeFileName = filesManagerService.GenerateSafeFileName(Path.GetFileName(file.FileName));

                    var saveResult = await filesManagerService.SaveFileAsync(file, recordSubDir, safeFileName);
                    if (saveResult.IsError)
                    {
                        await CleanupStoredFilesAsync(uploadedPaths);
                        return saveResult.Errors;
                    }

                    uploadedPaths.Add(saveResult.Value!.FilePath);

                    physicalFiles.Add(new PhysicalFile
                    {
                        Id = Guid.NewGuid(),
                        ArchiveRecordId = record.Id,
                        EditArchiveRequestId = requestId,
                        FileName = Path.GetFileName(file.FileName),
                        FileExtension = Path.GetExtension(file.FileName).ToLowerInvariant(),
                        StoragePath = saveResult.Value.FilePath,
                        FileSize = saveResult.Value.FileSize,
                        ContentType = saveResult.Value.ContentType,
                        IsDeleted = false
                    });
                }
            }

            await unitOfWork.BeginTransactionAsync();
            try
            {
                var addResult = await unitOfWork.EditArchiveRequests.AddAsync(request);
                if (addResult.IsError)
                {
                    await unitOfWork.RollbackTransactionAsync();
                    await CleanupStoredFilesAsync(uploadedPaths);
                    return addResult.Errors;
                }

                foreach (var pf in physicalFiles)
                {
                    var addFileResult = await unitOfWork.PhysicalFiles.AddAsync(pf);
                    if (addFileResult.IsError)
                    {
                        await unitOfWork.RollbackTransactionAsync();
                        await CleanupStoredFilesAsync(uploadedPaths);
                        return addFileResult.Errors;
                    }
                }

                var saveResult = await unitOfWork.SaveChangesAsync();
                if (saveResult <= 0)
                {
                    await unitOfWork.RollbackTransactionAsync();
                    await CleanupStoredFilesAsync(uploadedPaths);
                    return ArchiveErrors.DatabaseError;
                }

                await unitOfWork.CommitTransactionAsync();

                request.ArchiveRecord = record;
                return EditArchiveRequestDto.FromEntity(request);
            }
            catch (Exception)
            {
                await unitOfWork.RollbackTransactionAsync();
                await CleanupStoredFilesAsync(uploadedPaths);
                throw;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error submitting edit archive request for record {RecordId}", dto.ArchiveRecordId);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<EditArchiveRequestDto>> GetByIdAsync(Guid requestId)
    {
        try
        {
            var request = await unitOfWork.EditArchiveRequests.GetAsync(
                x => x.Id == requestId,
                q => q.Include(x => x.Requester)
                      .Include(x => x.Approver)
                      .Include(x => x.ArchiveRecord)
                      .Include(x => x.PhysicalFiles));

            if (request.IsError || request.Value == null)
                return ArchiveErrors.EditRequestNotFound;

            return EditArchiveRequestDto.FromEntity(request.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching edit archive request {RequestId}", requestId);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<EditArchiveRequestDto>>> GetPendingForDepartmentAsync(Guid departmentId, int page = 1, int pageSize = 20)
    {
        try
        {
            var query = dbContext.EditArchiveRequests
                .Include(x => x.Requester)
                .Include(x => x.ArchiveRecord)
                .Include(x => x.PhysicalFiles)
                .Where(x => x.DepartmentId == departmentId && x.Status == EditArchiveRequestStatus.Pending)
                .OrderByDescending(x => x.CreatedAt);

            var count = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var dtos = items.Select(EditArchiveRequestDto.FromEntity).ToList();
            return new PagedList<EditArchiveRequestDto>(dtos, count, page, pageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching pending edit requests for department {DepartmentId}", departmentId);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<EditArchiveRequestDto>>> GetMyRequestsAsync(Guid requesterId, int page = 1, int pageSize = 20)
    {
        try
        {
            var query = dbContext.EditArchiveRequests
                .Include(x => x.Requester)
                .Include(x => x.Approver)
                .Include(x => x.ArchiveRecord)
                .Include(x => x.PhysicalFiles)
                .Where(x => x.RequesterId == requesterId)
                .OrderByDescending(x => x.CreatedAt);

            var count = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var dtos = items.Select(EditArchiveRequestDto.FromEntity).ToList();
            return new PagedList<EditArchiveRequestDto>(dtos, count, page, pageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching my edit archive requests for user {RequesterId}", requesterId);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<EditArchiveRequestDto>> ApproveAsync(Guid requestId, string? notes = null)
    {
        try
        {
            var request = await unitOfWork.EditArchiveRequests.GetAsync(
                x => x.Id == requestId,
                q => q.Include(x => x.Requester)
                      .Include(x => x.ArchiveRecord)
                        .ThenInclude(r => r.ArchiveRecordTemplateValuesId)
                            .ThenInclude(v => v.ArchiveRecordFormInputValues)
                      .Include(x => x.PhysicalFiles));

            if (request.IsError || request.Value == null)
                return ArchiveErrors.EditRequestNotFound;

            var requestEntity = request.Value;
            if (requestEntity.Status != EditArchiveRequestStatus.Pending)
                return ArchiveErrors.EditRequestAlreadyHandled;

            var approverId = httpContextServiceManager.GetCurrentUserId();

            var isLeaderResult = await archiveLeaderService.IsArchiveLeaderAsync(approverId, requestEntity.DepartmentId);
            if (isLeaderResult.IsError || !isLeaderResult.Value)
                return ArchiveErrors.InternalServerError;

            await unitOfWork.BeginTransactionAsync();
            try
            {
                requestEntity.Status = EditArchiveRequestStatus.Approved;
                requestEntity.ApproverId = approverId;
                requestEntity.ApprovedByUserId = approverId;
                requestEntity.ApprovedAt = DateTime.UtcNow;
                requestEntity.ApprovalNotes = notes;

                var changes = string.IsNullOrEmpty(requestEntity.RequestedChangesJson)
                    ? []
                    : JsonSerializer.Deserialize<List<ArchiveRecordFormInputValueDto>>(requestEntity.RequestedChangesJson, JsonOptions) ?? [];

                var record = requestEntity.ArchiveRecord;
                if (record != null && record.ArchiveRecordTemplateValuesId != null && changes.Count != 0)
                {
                    var oldValues = record.ArchiveRecordTemplateValuesId.ArchiveRecordFormInputValues.ToList();
                    if (oldValues.Count > 0)
                    {
                        foreach (var change in changes)
                        {
                            var existingValue = oldValues.Find(x => x.Key == change.Key);
                            if (existingValue != null)
                            {
                                existingValue.Value = change.Value;
                                var updateResult = await unitOfWork.ArchiveRecordFormInputValues.UpdateAsync(existingValue);
                                if (updateResult.IsError)
                                    return updateResult.Errors;
                            }
                        }
                    }
                }

                if (record != null && !string.IsNullOrWhiteSpace(requestEntity.RequestedRecordName))
                    record.Name = requestEntity.RequestedRecordName;

                var attachedFiles = await unitOfWork.PhysicalFiles.GetAllAsync(x => x.EditArchiveRequestId == requestId);
                if (!attachedFiles.IsError && attachedFiles.Value != null)
                {
                    foreach (var file in attachedFiles.Value)
                        file.EditArchiveRequestId = null;
                }

                var fileDeletionIds = string.IsNullOrEmpty(requestEntity.RequestedFileDeletionIdsJson)
                    ? null
                    : JsonSerializer.Deserialize<List<Guid>>(requestEntity.RequestedFileDeletionIdsJson, JsonOptions);

                if (fileDeletionIds != null && fileDeletionIds.Count > 0)
                {
                    var filesToDelete = await unitOfWork.PhysicalFiles.GetAllAsync(
                        x => fileDeletionIds.Contains(x.Id)
                          && x.ArchiveRecordId == requestEntity.ArchiveRecordId
                          && !x.IsDeleted);

                    if (!filesToDelete.IsError && filesToDelete.Value != null)
                    {
                        foreach (var file in filesToDelete.Value)
                        {
                            file.IsDeleted = true;
                            file.DeletedAt = DateTime.UtcNow;
                            file.DeletedByUserId = approverId.ToString();
                        }
                    }
                }

                var saveResult = await unitOfWork.SaveChangesAsync();
                if (saveResult <= 0)
                {
                    await unitOfWork.RollbackTransactionAsync();
                    return ArchiveErrors.DatabaseError;
                }

                await unitOfWork.CommitTransactionAsync();

                return EditArchiveRequestDto.FromEntity(requestEntity);
            }
            catch (Exception)
            {
                await unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error approving edit archive request {RequestId}", requestId);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<EditArchiveRequestDto>> RejectAsync(Guid requestId, string reason)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(reason))
                return ArchiveErrors.DeleteRequestRejectionRequiresReason;

            var request = await unitOfWork.EditArchiveRequests.GetAsync(
                x => x.Id == requestId,
                q => q.Include(x => x.Requester)
                      .Include(x => x.ArchiveRecord)
                      .Include(x => x.PhysicalFiles));

            if (request.IsError || request.Value == null)
                return ArchiveErrors.EditRequestNotFound;

            var requestEntity = request.Value;
            if (requestEntity.Status != EditArchiveRequestStatus.Pending)
                return ArchiveErrors.EditRequestAlreadyHandled;

            var approverId = httpContextServiceManager.GetCurrentUserId();

            await unitOfWork.BeginTransactionAsync();
            try
            {
                requestEntity.Status = EditArchiveRequestStatus.Rejected;
                requestEntity.ApproverId = approverId;
                requestEntity.RejectedByUserId = approverId;
                requestEntity.RejectedAt = DateTime.UtcNow;
                requestEntity.RejectionReason = reason;

                // Soft-delete files associated with this rejected request
                var attachedFiles = await unitOfWork.PhysicalFiles.GetAllAsync(x => x.EditArchiveRequestId == requestId);
                if (!attachedFiles.IsError && attachedFiles.Value != null)
                {
                    foreach (var file in attachedFiles.Value)
                    {
                        file.IsDeleted = true;
                        file.DeletedAt = DateTime.UtcNow;
                        file.DeletedByUserId = approverId.ToString();
                    }
                }

                var saveResult = await unitOfWork.SaveChangesAsync();
                if (saveResult <= 0)
                {
                    await unitOfWork.RollbackTransactionAsync();
                    return ArchiveErrors.DatabaseError;
                }

                await unitOfWork.CommitTransactionAsync();

                return EditArchiveRequestDto.FromEntity(requestEntity);
            }
            catch (Exception)
            {
                await unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error rejecting edit archive request {RequestId}", requestId);
            return ArchiveErrors.InternalServerError;
        }
    }

    private async Task CleanupStoredFilesAsync(List<string> absolutePaths)
    {
        foreach (var absolutePath in absolutePaths)
        {
            var deleteResult = await filesManagerService.DeleteFileAsync(absolutePath);
            if (deleteResult.IsError)
                logger.LogWarning("Cleanup failed for stored file at path: {Path}", absolutePath);
        }
    }
}