using FileManager.Abstractions;
using FileManager.Services.Abstraction;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernPaySystem.Application.Interfaces;
using ModernPaySystem.Application.Services;
using ModernPaySystem.Domain.Commons;
using ModernPaySystem.Domain.Entities.Archiving;
using System.IO;
using System.Text.Json;

namespace ModernPaySystem.Infrastructure.Services;

public class ArchiveEditWorkflowService(
    IUnitOfWork unitOfWork,
    IHttpContextServiceManager httpContextServiceManager,
    IArchiveAuthorizationService archiveAuthorizationService,
    IArchiveLeaderService archiveLeaderService,
    IFilesManagerService filesManagerService,
    ILogger<ArchiveEditWorkflowService> logger) : IArchiveEditWorkflowService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Result<EditArchiveRequestDto>> SubmitRequestAsync(CreateEditArchiveRequestDto dto)
    {
        var uploadedPaths = new List<string>();
        try
        {
            if (dto.ArchiveRecordId == Guid.Empty || string.IsNullOrWhiteSpace(dto.Justification))
            {
                return ApplicationErrors.InvalidInput;
            }

            var recordResult = await unitOfWork.ArchiveRecords.GetAsync(
                x => x.Id == dto.ArchiveRecordId && !x.IsDeleted,
                q => q.Include(r => r.ArchiveRecordTemplateValuesId)
                      .ThenInclude(v => v.ArchiveRecordFormInputValues));

            if (recordResult.IsError || recordResult.Value == null)
            {
                return ApplicationErrors.ArchiveRecordNotFound;
            }

            var record = recordResult.Value;
            var departmentId = record.DepartmentId;
            if (!departmentId.HasValue)
            {
                return ApplicationErrors.ArchiveRecordDepartmentNotConfigured;
            }

            var requesterId = httpContextServiceManager.GetCurrentUserId();
            var requesterResult = await unitOfWork.Users.GetByIdAsync(requesterId);
            if (requesterResult.IsError || requesterResult.Value == null)
            {
                return ApplicationErrors.UserNotFound;
            }

            // Check if department archive leaders are assigned for this department
            var leadersResult = await archiveLeaderService.GetByDepartmentAsync(departmentId.Value);
            if (leadersResult.IsError || !leadersResult.Value.Any(l => !l.IsDeleted))
            {
                return ApplicationErrors.DepartmentArchiveLeaderNotAssigned;
            }

            // Original Snapshot JSON
            var originalData = new
            {
                record.ArchivalNumber,
                FormId = record.FormId,
                Content = record.ArchiveRecordTemplateValuesId?.ArchiveRecordFormInputValues
                    .Select(x => new { x.Key, x.Value }).ToList() ?? []
            };
            var originalSnapshotJson = JsonSerializer.Serialize(originalData, JsonOptions);

            var requestedChangesJson = JsonSerializer.Serialize(dto.RequestedChanges, JsonOptions);

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
                OriginalSnapshotJson = originalSnapshotJson,
                RowVersion = Guid.NewGuid().ToByteArray()
            };

            // Store files if any
            var physicalFiles = new List<PhysicalFile>();
            if (dto.Files != null && dto.Files.Count > 0)
            {
                var validFiles = dto.Files.Where(f => f != null && f.Length > 0).ToList();
                foreach (var file in validFiles)
                {
                    // Validate file size (e.g. 20MB limit)
                    // if (file.Length > 20 * 1024 * 1024)
                    // {
                    //     await CleanupStoredFilesAsync(uploadedPaths);
                    //     return ApplicationErrors.AttachmentTooLarge;
                    // }

                    // Validate file extension
                    var extension = Path.GetExtension(file.FileName);
                    if (!filesManagerService.IsValidFileExtension(extension, null))
                    {
                        await CleanupStoredFilesAsync(uploadedPaths);
                        return ApplicationErrors.InvalidAttachmentType;
                    }

                    var safeFileName = filesManagerService.GenerateSafeFileName(Path.GetFileName(file.FileName));
                    var storageName = $"{record.Id}_{safeFileName}";
                    
                    var saveResult = await filesManagerService.SaveFileAsync(file, record.FolderId.ToString(), storageName);
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
                    return ApplicationErrors.DatabaseError;
                }

                await unitOfWork.CommitTransactionAsync();
                
                // Return mapping
                request.Requester = requesterResult.Value;
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
            logger.LogError(ex, "Error submitting edit archive request for record {RecordId}. Inner exception: {InnerMessage}", dto.ArchiveRecordId, ex.InnerException?.Message);
            return new Error("SUBMIT_ERROR", $"{ex.Message} (Inner: {ex.InnerException?.Message})", ErrorKind.Unexpected);
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
            {
                return ApplicationErrors.EditRequestNotFound;
            }

            return EditArchiveRequestDto.FromEntity(request.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching edit archive request {RequestId}", requestId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<EditArchiveRequestDto>>> GetPendingForDepartmentAsync(Guid departmentId, int page = 1, int pageSize = 20)
    {
        try
        {
            var query = unitOfWork.Context.EditArchiveRequests
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
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<EditArchiveRequestDto>>> GetMyRequestsAsync(Guid requesterId, int page = 1, int pageSize = 20)
    {
        try
        {
            var query = unitOfWork.Context.EditArchiveRequests
                .Include(x => x.Requester)
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
            return ApplicationErrors.InternalServerError;
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
            {
                return ApplicationErrors.EditRequestNotFound;
            }

            var requestEntity = request.Value;
            if (requestEntity.Status != EditArchiveRequestStatus.Pending)
            {
                return ApplicationErrors.EditRequestAlreadyHandled;
            }

            var approverId = httpContextServiceManager.GetCurrentUserId();
            var approverResult = await unitOfWork.Users.GetByIdAsync(approverId);
            if (approverResult.IsError || approverResult.Value == null)
            {
                return ApplicationErrors.UserNotFound;
            }

            await unitOfWork.BeginTransactionAsync();
            try
            {
                requestEntity.Status = EditArchiveRequestStatus.Approved;
                requestEntity.ApproverId = approverId;
                requestEntity.ApprovedByUserId = approverId;
                requestEntity.ApprovedAt = DateTime.UtcNow;
                requestEntity.ApprovalNotes = notes;

                // Apply changes to the ArchiveRecord's template values
                var changes = string.IsNullOrEmpty(requestEntity.RequestedChangesJson)
                    ? new List<ArchiveRecordFormInputValueDto>()
                    : JsonSerializer.Deserialize<List<ArchiveRecordFormInputValueDto>>(requestEntity.RequestedChangesJson, JsonOptions) ?? [];

                var record = requestEntity.ArchiveRecord;
                if (record != null)
                {
                    if (record.ArchiveRecordTemplateValuesId == null)
                    {
                        var newTemplateValues = new ArchiveRecordTemplateValues
                        {
                            Id = Guid.NewGuid(),
                            ArchiveRecordId = record.Id,
                            ArchiveFormTemplateId = record.FormId ?? Guid.Empty,
                            ArchiveRecordFormInputValues = changes.Select(x => new ArchiveRecordFormInputValue
                            {
                                Id = Guid.NewGuid(),
                                Key = x.Key,
                                Value = x.Value
                            }).ToList()
                        };
                        record.ArchiveRecordTemplateValuesId = newTemplateValues;
                        await unitOfWork.ArchiveRecordTemplateValues.AddAsync(newTemplateValues);
                    }
                    else
                    {
                        var existingInputValues = record.ArchiveRecordTemplateValuesId.ArchiveRecordFormInputValues.ToList();
                        record.ArchiveRecordTemplateValuesId.ArchiveRecordFormInputValues.Clear();
                        
                        // Explicit Remove is not needed and causes DbUpdateConcurrencyException because of Cascade Delete
                        
                        foreach (var change in changes)
                        {
                            record.ArchiveRecordTemplateValuesId.ArchiveRecordFormInputValues.Add(new ArchiveRecordFormInputValue
                            {
                                Id = Guid.NewGuid(),
                                Key = change.Key,
                                Value = change.Value
                            });
                        }
                    }
                }

                // Mark any files attached to this request as permanent by clearing EditArchiveRequestId
                var attachedFiles = await unitOfWork.PhysicalFiles.GetAllAsync(x => x.EditArchiveRequestId == requestId);
                if (!attachedFiles.IsError && attachedFiles.Value != null)
                {
                    foreach (var file in attachedFiles.Value)
                    {
                        file.EditArchiveRequestId = null;
                    }
                }

                // Removed manual RowVersion update since it conflicts with ValueGeneratedOnAddOrUpdate in EF Core

                var saveResult = await unitOfWork.SaveChangesAsync();
                if (saveResult <= 0)
                {
                    await unitOfWork.RollbackTransactionAsync();
                    return ApplicationErrors.DatabaseError;
                }

                await unitOfWork.CommitTransactionAsync();
                requestEntity.Approver = approverResult.Value;
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
            return new Error("500", $"حدث خطأ أثناء الموافقة على الطلب: {ex.Message} - {ex.InnerException?.Message}", ErrorKind.Failure);
        }
    }

    public async Task<Result<EditArchiveRequestDto>> RejectAsync(Guid requestId, string reason)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return ApplicationErrors.DeleteRequestRejectionRequiresReason;
            }

            var request = await unitOfWork.EditArchiveRequests.GetAsync(
                x => x.Id == requestId,
                q => q.Include(x => x.Requester)
                      .Include(x => x.ArchiveRecord)
                      .Include(x => x.PhysicalFiles));

            if (request.IsError || request.Value == null)
            {
                return ApplicationErrors.EditRequestNotFound;
            }

            var requestEntity = request.Value;
            if (requestEntity.Status != EditArchiveRequestStatus.Pending)
            {
                return ApplicationErrors.EditRequestAlreadyHandled;
            }

            var approverId = httpContextServiceManager.GetCurrentUserId();
            var approverResult = await unitOfWork.Users.GetByIdAsync(approverId);
            if (approverResult.IsError || approverResult.Value == null)
            {
                return ApplicationErrors.UserNotFound;
            }

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

                // Removed manual RowVersion update since it conflicts with ValueGeneratedOnAddOrUpdate in EF Core

                var saveResult = await unitOfWork.SaveChangesAsync();
                if (saveResult <= 0)
                {
                    await unitOfWork.RollbackTransactionAsync();
                    return ApplicationErrors.DatabaseError;
                }

                await unitOfWork.CommitTransactionAsync();
                requestEntity.Approver = approverResult.Value;
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
            return new Error("500", $"حدث خطأ أثناء رفض الطلب: {ex.Message} - {ex.InnerException?.Message}", ErrorKind.Failure);
        }
    }

    private async Task CleanupStoredFilesAsync(List<string> absolutePaths)
    {
        foreach (var absolutePath in absolutePaths)
        {
            var deleteResult = await filesManagerService.DeleteFileAsync(absolutePath);
            if (deleteResult.IsError)
            {
                logger.LogWarning("Cleanup failed for stored file at path: {Path}. Error: {Error}", absolutePath, deleteResult.Errors);
            }
        }
    }
}
