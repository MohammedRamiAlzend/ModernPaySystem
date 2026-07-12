using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernPaySystem.Module.Archive.Application;
using ModernPaySystem.Module.Archive.Application.Interfaces;
using ModernPaySystem.Module.Archive.Domain;
using ModernPaySystem.Module.Archive.Domain.Entities;
using ModernPaySystem.Module.Archive.Infrastructure.Persistence;
using ModernPaySystem.SharedKernel.Application.Services;
using ModernPaySystem.SharedKernel.Domain.Commons;
using System.Text.Json;

namespace ModernPaySystem.Module.Archive.Infrastructure.Services;

public class ArchiveDeletionWorkflowService(
    IArchiveUnitOfWork unitOfWork,
    ArchiveDbContext dbContext,
    IHttpContextServiceManager httpContextServiceManager,
    IArchiveAuthorizationService archiveAuthorizationService,
    IArchiveLeaderService archiveLeaderService,
    IAuditLogService auditLogService,
    ILogger<ArchiveDeletionWorkflowService> logger) : IArchiveDeletionWorkflowService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Result<DeleteArchiveRequestDto>> SubmitRequestAsync(CreateDeleteArchiveRequestDto dto)
    {
        try
        {
            if (dto.TargetId == Guid.Empty || string.IsNullOrWhiteSpace(dto.Justification))
                return ArchiveErrors.InvalidInput;

            var requesterId = httpContextServiceManager.GetCurrentUserId();

            var target = await ResolveTargetAsync(dto.TargetType, dto.TargetId);
            if (target.IsError)
                return target.Errors;

            if (dto.TargetType == ArchiveDeletionTargetType.Folder)
            {
                var hasSubFolders = await dbContext.Folders
                    .AnyAsync(f => f.ParentId == dto.TargetId && !f.IsDeleted);

                var hasRecords = await dbContext.ArchiveRecords
                    .AnyAsync(r => r.FolderId == dto.TargetId && !r.IsDeleted);

                if (hasSubFolders || hasRecords)
                    return ArchiveErrors.FolderHasChildren;
            }

            if (target.Value.departmentId is null)
                return dto.TargetType == ArchiveDeletionTargetType.Folder
                    ? ArchiveErrors.FolderDepartmentNotConfigured
                    : ArchiveErrors.ArchiveRecordDepartmentNotConfigured;

            var leaderCheck = await archiveLeaderService.IsArchiveLeaderAsync(requesterId, target.Value.departmentId.Value);
            if (leaderCheck.IsError)
                return leaderCheck.Errors;

            if (leaderCheck.Value)
                return dto.TargetType == ArchiveDeletionTargetType.Folder
                    ? ArchiveErrors.FolderArchiveLeaderRequired
                    : ArchiveErrors.ArchiveRecordArchiveLeaderRequired;

            var existingPending = await unitOfWork.DeleteArchiveRequests.AnyAsync(x =>
                x.DepartmentId == target.Value.departmentId.Value &&
                x.TargetType == dto.TargetType &&
                x.TargetId == dto.TargetId &&
                x.Status == DeleteArchiveRequestStatus.Pending);

            if (existingPending)
                return dto.TargetType == ArchiveDeletionTargetType.Folder
                    ? ArchiveErrors.FolderDeleteRequestExists
                    : ArchiveErrors.DeleteRequestAlreadyHandled;

            var snapshot = await BuildSnapshotAsync(dto.TargetType, dto.TargetId, target.Value.departmentId.Value);
            var dependencies = await BuildDependenciesAsync(dto.TargetType, dto.TargetId, target.Value.departmentId.Value);

            var request = new DeleteArchiveRequest
            {
                Id = Guid.NewGuid(),
                DepartmentId = target.Value.departmentId.Value,
                TargetType = dto.TargetType,
                TargetId = dto.TargetId,
                RequesterId = requesterId,
                Status = DeleteArchiveRequestStatus.Pending,
                Justification = dto.Justification.Trim(),
                TargetSnapshotJson = JsonSerializer.Serialize(snapshot, JsonOptions),
                DependenciesSnapshotJson = JsonSerializer.Serialize(dependencies, JsonOptions),
                SourceFolderId = target.Value.sourceFolderId,
                TargetDisplayName = snapshot.DisplayName
            };

            var addResult = await unitOfWork.DeleteArchiveRequests.AddAsync(request);
            if (addResult.IsError)
                return addResult.Errors;

            var saveResult = await unitOfWork.SaveChangesAsync();
            if (saveResult <= 0)
                return ArchiveErrors.DatabaseError;

            logger.LogInformation("Submitted archive delete request {RequestId} for {TargetType}:{TargetId}", request.Id, dto.TargetType, dto.TargetId);

            if (dto.TargetType == ArchiveDeletionTargetType.Record)
            {
                var ipAddress = httpContextServiceManager.GetClientIpAddress();
                var userAgent = httpContextServiceManager.GetUserAgent();
                await auditLogService.LogAsync(dto.TargetId, requesterId.ToString(), AuditAction.SubmitDeleteRequest, "Submitted delete request for archive record", ipAddress, userAgent);
            }

            return request.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error submitting archive delete request for {TargetType}:{TargetId}", dto.TargetType, dto.TargetId);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<DeleteArchiveRequestDto>> GetByIdAsync(Guid requestId)
    {
        try
        {
            if (requestId == Guid.Empty)
                return ArchiveErrors.InvalidInput;

            var requestResult = await unitOfWork.DeleteArchiveRequests.GetAsync(
                x => x.Id == requestId);

            if (requestResult.IsError)
                return requestResult.Errors;

            if (requestResult.Value == null)
                return ArchiveErrors.DeleteRequestNotFound;

            return requestResult.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching archive delete request {RequestId}", requestId);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<DeleteArchiveRequestDto>>> GetPendingForDepartmentAsync(Guid departmentId, int page = 1, int pageSize = 20)
    {
        try
        {
            if (departmentId == Guid.Empty || page <= 0 || pageSize <= 0 || pageSize > 100)
                return ArchiveErrors.InvalidInput;

            var result = await unitOfWork.DeleteArchiveRequests.GetPagedAsync(
                page,
                pageSize,
                filter: x => x.DepartmentId == departmentId && x.Status == DeleteArchiveRequestStatus.Pending);

            if (result.IsError)
                return result.Errors;

            var items = result.Value!.Items.Select(x => x.ToDto()).ToList();
            return new PagedList<DeleteArchiveRequestDto>(items, result.Value.TotalItems, page, pageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching delete requests for department {DepartmentId}", departmentId);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<DeleteArchiveRequestDto>> ApproveAsync(Guid requestId, string? notes = null)
    {
        try
        {
            if (requestId == Guid.Empty)
                return ArchiveErrors.InvalidInput;

            var currentUserId = httpContextServiceManager.GetCurrentUserId();
            var requestResult = await unitOfWork.DeleteArchiveRequests.GetAsync(
                x => x.Id == requestId);

            if (requestResult.IsError)
                return requestResult.Errors;

            var request = requestResult.Value;
            if (request == null)
                return ArchiveErrors.DeleteRequestNotFound;

            var headCheck = await archiveAuthorizationService.IsDepartmentHeadAsync(currentUserId, request.DepartmentId);
            if (headCheck.IsError || !headCheck.Value)
                return ArchiveErrors.DeleteRequestApprovalRequiresDepartmentHead;

            if (request.Status is DeleteArchiveRequestStatus.Rejected or DeleteArchiveRequestStatus.Executed)
                return ArchiveErrors.DeleteRequestAlreadyHandled;

            await unitOfWork.BeginTransactionAsync();

            request.Status = DeleteArchiveRequestStatus.Approved;
            request.ApprovedByUserId = currentUserId;
            request.ApprovedAt = DateTime.UtcNow;
            request.ApprovalNotes = notes;

            var executionResult = request.TargetType switch
            {
                ArchiveDeletionTargetType.Folder => await SoftDeleteFolderTreeAsync(request.TargetId, request.Id, "System"),
                ArchiveDeletionTargetType.Record => await SoftDeleteRecordAsync(request.TargetId, request.Id, "System", "System"),
                _ => ArchiveErrors.InvalidInput
            };

            if (executionResult.IsError)
            {
                await unitOfWork.RollbackTransactionAsync();
                return executionResult.Errors;
            }

            request.Status = DeleteArchiveRequestStatus.Executed;
            request.ExecutedByUserId = null;
            request.ExecutedAt = DateTime.UtcNow;
            request.RequesterNotificationMessage = "Your archive delete request was approved and executed.";
            request.RequesterNotifiedAt = DateTime.UtcNow;

            var saveResult = await unitOfWork.SaveChangesAsync();
            if (saveResult <= 0)
            {
                await unitOfWork.RollbackTransactionAsync();
                return ArchiveErrors.DatabaseError;
            }

            await unitOfWork.CommitTransactionAsync();
            logger.LogInformation("Approved and executed archive delete request {RequestId}", requestId);

            if (request.TargetType == ArchiveDeletionTargetType.Record)
            {
                var ipAddress = httpContextServiceManager.GetClientIpAddress();
                var userAgent = httpContextServiceManager.GetUserAgent();
                await auditLogService.LogAsync(request.TargetId, currentUserId.ToString(), AuditAction.ApproveDelete,
                    $"Approved delete request for archive record{(notes != null ? $": {notes}" : "")}", ipAddress, userAgent);
            }

            return request.ToDto();
        }
        catch (Exception ex)
        {
            if (unitOfWork.HasActiveTransaction)
                await unitOfWork.RollbackTransactionAsync();

            logger.LogError(ex, "Error approving delete request {RequestId}", requestId);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<DeleteArchiveRequestDto>> RejectAsync(Guid requestId, string reason)
    {
        try
        {
            if (requestId == Guid.Empty || string.IsNullOrWhiteSpace(reason))
                return ArchiveErrors.DeleteRequestRejectionRequiresReason;

            var currentUserId = httpContextServiceManager.GetCurrentUserId();
            var requestResult = await unitOfWork.DeleteArchiveRequests.GetAsync(
                x => x.Id == requestId);

            if (requestResult.IsError)
                return requestResult.Errors;

            var request = requestResult.Value;
            if (request == null)
                return ArchiveErrors.DeleteRequestNotFound;

            var headCheck = await archiveAuthorizationService.IsDepartmentHeadAsync(currentUserId, request.DepartmentId);
            if (headCheck.IsError || !headCheck.Value)
                return ArchiveErrors.DeleteRequestApprovalRequiresDepartmentHead;

            if (request.Status is DeleteArchiveRequestStatus.Rejected or DeleteArchiveRequestStatus.Executed)
                return ArchiveErrors.DeleteRequestAlreadyHandled;

            request.Status = DeleteArchiveRequestStatus.Rejected;
            request.RejectedByUserId = currentUserId;
            request.RejectedAt = DateTime.UtcNow;
            request.RejectionReason = reason.Trim();
            request.RequesterNotificationMessage = reason.Trim();
            request.RequesterNotifiedAt = DateTime.UtcNow;

            var saveResult = await unitOfWork.SaveChangesAsync();
            if (saveResult <= 0)
                return ArchiveErrors.DatabaseError;

            logger.LogInformation("Rejected archive delete request {RequestId}", requestId);

            if (request.TargetType == ArchiveDeletionTargetType.Record)
            {
                var ipAddress = httpContextServiceManager.GetClientIpAddress();
                var userAgent = httpContextServiceManager.GetUserAgent();
                await auditLogService.LogAsync(request.TargetId, currentUserId.ToString(), AuditAction.RejectDelete,
                    $"Rejected delete request for archive record: {reason}", ipAddress, userAgent);
            }

            return request.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error rejecting delete request {RequestId}", requestId);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> DeleteFolderAsync(Guid folderId, Guid? requestId = null)
    {
        try
        {
            if (folderId == Guid.Empty)
                return ArchiveErrors.InvalidInput;

            var hasSubFolders = await dbContext.Folders
                .AnyAsync(f => f.ParentId == folderId && !f.IsDeleted);

            var hasRecords = await dbContext.ArchiveRecords
                .AnyAsync(r => r.FolderId == folderId && !r.IsDeleted);

            if (hasSubFolders || hasRecords)
                return ArchiveErrors.FolderHasChildren;

            var departmentResult = await archiveAuthorizationService.ResolveFolderDepartmentIdAsync(folderId);
            if (departmentResult.IsError)
                return departmentResult.Errors;

            if (!departmentResult.Value.HasValue)
                return ArchiveErrors.FolderDepartmentNotConfigured;

            var userId = httpContextServiceManager.GetCurrentUserId();
            var isLeader = await archiveLeaderService.IsArchiveLeaderAsync(userId, departmentResult.Value.Value);
            if (isLeader.IsError || !isLeader.Value)
                return ArchiveErrors.FolderArchiveLeaderRequired;

            await unitOfWork.BeginTransactionAsync();
            var deleteResult = await SoftDeleteFolderTreeAsync(folderId, requestId, userId.ToString());
            if (deleteResult.IsError)
            {
                await unitOfWork.RollbackTransactionAsync();
                return deleteResult.Errors;
            }

            var saveResult = await unitOfWork.SaveChangesAsync();
            if (saveResult <= 0)
            {
                await unitOfWork.RollbackTransactionAsync();
                return ArchiveErrors.DatabaseError;
            }

            await unitOfWork.CommitTransactionAsync();
            return true;
        }
        catch (Exception ex)
        {
            if (unitOfWork.HasActiveTransaction)
                await unitOfWork.RollbackTransactionAsync();

            logger.LogError(ex, "Error deleting folder {FolderId}", folderId);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> DeleteArchiveRecordAsync(Guid recordId, Guid? requestId = null)
    {
        try
        {
            if (recordId == Guid.Empty)
                return ArchiveErrors.InvalidInput;

            var departmentResult = await archiveAuthorizationService.ResolveArchiveRecordDepartmentIdAsync(recordId);
            if (departmentResult.IsError)
                return departmentResult.Errors;

            if (!departmentResult.Value.HasValue)
                return ArchiveErrors.ArchiveRecordDepartmentNotConfigured;

            var userId = httpContextServiceManager.GetCurrentUserId();
            var isLeader = await archiveLeaderService.IsArchiveLeaderAsync(userId, departmentResult.Value.Value);
            if (isLeader.IsError || !isLeader.Value)
                return ArchiveErrors.ArchiveRecordArchiveLeaderRequired;

            await unitOfWork.BeginTransactionAsync();
            var deleteResult = await SoftDeleteRecordAsync(recordId, requestId, userId.ToString());
            if (deleteResult.IsError)
            {
                await unitOfWork.RollbackTransactionAsync();
                return deleteResult.Errors;
            }

            var saveResult = await unitOfWork.SaveChangesAsync();
            if (saveResult <= 0)
            {
                await unitOfWork.RollbackTransactionAsync();
                return ArchiveErrors.DatabaseError;
            }

            await unitOfWork.CommitTransactionAsync();

            return true;
        }
        catch (Exception ex)
        {
            if (unitOfWork.HasActiveTransaction)
                await unitOfWork.RollbackTransactionAsync();

            logger.LogError(ex, "Error deleting archive record {RecordId}", recordId);
            return ArchiveErrors.InternalServerError;
        }
    }

    private async Task<Result<(Guid? departmentId, Guid? sourceFolderId, string displayName)>> ResolveTargetAsync(ArchiveDeletionTargetType targetType, Guid targetId)
    {
        return targetType switch
        {
            ArchiveDeletionTargetType.Folder => await ResolveFolderTargetAsync(targetId),
            ArchiveDeletionTargetType.Record => await ResolveRecordTargetAsync(targetId),
            _ => ArchiveErrors.InvalidInput
        };
    }

    private async Task<Result<(Guid? departmentId, Guid? sourceFolderId, string displayName)>> ResolveFolderTargetAsync(Guid folderId)
    {
        var folder = await dbContext.Folders
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.Id == folderId)
            .Select(x => new { x.DepartmentId, x.ParentId, x.Name })
            .SingleOrDefaultAsync();

        if (folder == null)
            return ArchiveErrors.FolderNotFound;

        var departmentId = folder.DepartmentId;
        if (!departmentId.HasValue && folder.ParentId.HasValue)
        {
            var resolved = await archiveAuthorizationService.ResolveFolderDepartmentIdAsync(folder.ParentId.Value);
            if (!resolved.IsError)
                departmentId = resolved.Value;
        }

        return (departmentId, folder.ParentId, folder.Name);
    }

    private async Task<Result<(Guid? departmentId, Guid? sourceFolderId, string displayName)>> ResolveRecordTargetAsync(Guid recordId)
    {
        var record = await dbContext.ArchiveRecords
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.Id == recordId)
            .Select(x => new { x.DepartmentId, x.FolderId })
            .SingleOrDefaultAsync();

        if (record == null)
            return ArchiveErrors.ArchiveRecordNotFound;

        var departmentId = record.DepartmentId;
        if (!departmentId.HasValue)
        {
            var resolved = await archiveAuthorizationService.ResolveFolderDepartmentIdAsync(record.FolderId);
            if (!resolved.IsError)
                departmentId = resolved.Value;
        }

        return (departmentId, record.FolderId, string.Empty);
    }

    private async Task<ArchiveDeletionTargetSnapshotDto> BuildSnapshotAsync(ArchiveDeletionTargetType targetType, Guid targetId, Guid departmentId)
    {
        return targetType switch
        {
            ArchiveDeletionTargetType.Folder => await BuildFolderSnapshotAsync(targetId, departmentId),
            ArchiveDeletionTargetType.Record => await BuildRecordSnapshotAsync(targetId, departmentId),
            _ => throw new InvalidOperationException("Unsupported target type.")
        };
    }

    private async Task<ArchiveDeletionTargetSnapshotDto> BuildFolderSnapshotAsync(Guid folderId, Guid departmentId)
    {
        var folder = await dbContext.Folders
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.Id == folderId)
            .Select(x => new { x.Id, x.Name, x.DepartmentId, x.ParentId, x.CreatedAt, x.UpdatedAt, x.CreatedByUserId, x.UpdatedByUserId })
            .SingleAsync();

        var childrenCount = await dbContext.Folders.IgnoreQueryFilters().CountAsync(x => x.ParentId == folderId);
        var descendantCount = await CountDescendantsAsync(folderId);
        var recordCount = await dbContext.ArchiveRecords.IgnoreQueryFilters().CountAsync(x => x.FolderId == folderId);
        var fileCount = await dbContext.PhysicalFiles.CountAsync(x => x.ArchiveRecord.FolderId == folderId);

        return new ArchiveDeletionTargetSnapshotDto(
            ArchiveDeletionTargetType.Folder,
            folder.Id,
            departmentId,
            folder.Name,
            folder.ParentId?.ToString(),
            childrenCount,
            descendantCount,
            recordCount,
            fileCount,
            JsonSerializer.Serialize(new
            {
                folder.CreatedAt,
                folder.UpdatedAt,
                folder.CreatedByUserId,
                folder.UpdatedByUserId
            }, JsonOptions));
    }

    private async Task<ArchiveDeletionTargetSnapshotDto> BuildRecordSnapshotAsync(Guid recordId, Guid departmentId)
    {
        var record = await dbContext.ArchiveRecords
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.Id == recordId)
            .Select(x => new { x.Id, x.FolderId, x.CreatedAt, x.UpdatedAt, x.CreatedByUserId, x.UpdatedByUserId })
            .SingleAsync();

        var fileCount = await dbContext.PhysicalFiles.CountAsync(x => x.ArchiveRecordId == recordId);

        return new ArchiveDeletionTargetSnapshotDto(
            ArchiveDeletionTargetType.Record,
            record.Id,
            departmentId,
            record.FolderId.ToString(),
            record.FolderId.ToString(),
            0,
            0,
            1,
            fileCount,
            JsonSerializer.Serialize(new
            {
                record.CreatedAt,
                record.UpdatedAt,
                record.CreatedByUserId,
                record.UpdatedByUserId
            }, JsonOptions));
    }

    private async Task<List<ArchiveDeletionDependencyDto>> BuildDependenciesAsync(ArchiveDeletionTargetType targetType, Guid targetId, Guid departmentId)
    {
        return targetType switch
        {
            ArchiveDeletionTargetType.Folder => await BuildFolderDependenciesAsync(targetId, departmentId),
            ArchiveDeletionTargetType.Record => await BuildRecordDependenciesAsync(targetId),
            _ => []
        };
    }

    private async Task<List<ArchiveDeletionDependencyDto>> BuildFolderDependenciesAsync(Guid folderId, Guid departmentId)
    {
        var list = new List<ArchiveDeletionDependencyDto>();

        var children = await dbContext.Folders
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.ParentId == folderId)
            .Select(x => new { x.Id, x.Name })
            .ToListAsync();

        foreach (var child in children)
            list.Add(new ArchiveDeletionDependencyDto("child-folder", child.Id, child.Name, "Child folder will be soft-deleted."));

        var records = await dbContext.ArchiveRecords
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.FolderId == folderId)
            .Select(x => new { x.Id })
            .ToListAsync();

        foreach (var record in records)
            list.Add(new ArchiveDeletionDependencyDto("archive-record", record.Id, string.Empty, "Archive record will be soft-deleted."));

        var leaders = await dbContext.DepartmentArchiveLeaders
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.DepartmentId == departmentId && !x.IsDeleted)
            .Select(x => new { x.Id, x.UserId })
            .ToListAsync();

        foreach (var leader in leaders)
            list.Add(new ArchiveDeletionDependencyDto("archive-leader", leader.Id, leader.UserId.ToString(), "Leader assignment protects direct deletion only."));

        return list;
    }

    private async Task<List<ArchiveDeletionDependencyDto>> BuildRecordDependenciesAsync(Guid recordId)
    {
        var list = new List<ArchiveDeletionDependencyDto>();
        var files = await dbContext.PhysicalFiles
            .Where(x => x.ArchiveRecordId == recordId)
            .AsNoTracking()
            .Select(x => new { x.Id, x.FileName, x.FileExtension })
            .ToListAsync();

        foreach (var file in files)
            list.Add(new ArchiveDeletionDependencyDto("physical-file", file.Id, file.FileName, file.FileExtension));

        return list;
    }

    private async Task<Result<bool>> SoftDeleteFolderTreeAsync(Guid folderId, Guid? requestId, string deletedByUserId)
    {
        var exists = await dbContext.Folders.IgnoreQueryFilters().AnyAsync(x => x.Id == folderId);
        if (!exists)
            return ArchiveErrors.FolderNotFound;

        await SoftDeleteFolderRecursiveAsync(folderId, requestId, deletedByUserId);
        return true;
    }

    private async Task SoftDeleteFolderRecursiveAsync(Guid folderId, Guid? requestId, string deletedByUserId)
    {
        var folder = await dbContext.Folders
            .IgnoreQueryFilters()
            .Include(x => x.SubFolders)
            .Include(x => x.ArchiveRecords)
            .ThenInclude(x => x.PhysicalFiles)
            .SingleAsync(x => x.Id == folderId);

        folder.IsDeleted = true;
        folder.DeletedAt = DateTime.UtcNow;
        folder.DeletedByUserId = deletedByUserId;
        folder.DeletedByRequestId = requestId;

        if (requestId.HasValue)
        {
            foreach (var record in folder.ArchiveRecords)
                await SoftDeleteRecordAsync(record.Id, requestId, deletedByUserId, "System");
        }

        foreach (var child in folder.SubFolders)
            await SoftDeleteFolderRecursiveAsync(child.Id, requestId, deletedByUserId);

        var updateResult = await unitOfWork.Folders.UpdateAsync(folder);
        if (updateResult.IsError)
            throw new InvalidOperationException(updateResult.Errors.First().Description);
    }

    private async Task<Result<bool>> SoftDeleteRecordAsync(Guid recordId, Guid? requestId, string deletedByUserId, string? deletedByLabel = null)
    {
        var record = await dbContext.ArchiveRecords
            .IgnoreQueryFilters()
            .Include(x => x.PhysicalFiles)
            .SingleOrDefaultAsync(x => x.Id == recordId);

        if (record == null)
            return ArchiveErrors.ArchiveRecordNotFound;

        record.IsDeleted = true;
        record.DeletedAt = DateTime.UtcNow;
        record.DeletedByUserId = deletedByLabel ?? deletedByUserId;
        record.DeletedByRequestId = requestId;

        foreach (var file in record.PhysicalFiles)
        {
            file.IsDeleted = true;
            file.DeletedAt = DateTime.UtcNow;
            file.DeletedByUserId = deletedByLabel ?? deletedByUserId;
            var fileUpdate = await unitOfWork.PhysicalFiles.UpdateAsync(file);
            if (fileUpdate.IsError)
                throw new InvalidOperationException(fileUpdate.Errors.First().Description);
        }

        var updateResult = await unitOfWork.ArchiveRecords.UpdateAsync(record);
        if (updateResult.IsError)
            throw new InvalidOperationException(updateResult.Errors.First().Description);

        return true;
    }

    private async Task<int> CountDescendantsAsync(Guid folderId)
    {
        var descendants = 0;
        var children = await dbContext.Folders.IgnoreQueryFilters().AsNoTracking().Where(x => x.ParentId == folderId).Select(x => x.Id).ToListAsync();
        foreach (var childId in children)
        {
            descendants++;
            descendants += await CountDescendantsAsync(childId);
        }
        return descendants;
    }
}