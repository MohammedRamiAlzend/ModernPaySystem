using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Application.Interfaces;
using ModernPaySystem.Application.Services;
using ModernPaySystem.Domain.Entities.Archiving;
using System.Text.Json;

namespace ModernPaySystem.Infrastructure.Services;

public class ArchiveDeletionWorkflowService(
    IUnitOfWork unitOfWork,
    IHttpContextServiceManager httpContextServiceManager,
    IArchiveAuthorizationService archiveAuthorizationService,
    IArchiveLeaderService archiveLeaderService,
    ILogger<ArchiveDeletionWorkflowService> logger,
    IAuditLogService auditLogService) : IArchiveDeletionWorkflowService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Result<DeleteArchiveRequestDto>> SubmitRequestAsync(CreateDeleteArchiveRequestDto dto)
    {
        try
        {
            if (dto.TargetId == Guid.Empty || string.IsNullOrWhiteSpace(dto.Justification))
            {
                return ApplicationErrors.InvalidInput;
            }

            var requesterId = httpContextServiceManager.GetCurrentUserId();
            var requesterResult = await unitOfWork.Users.GetByIdAsync(requesterId);
            if (requesterResult.IsError || requesterResult.Value == null)
            {
                return ApplicationErrors.UserNotFound;
            }

            if (!requesterResult.Value.DepartmentId.HasValue)
            {
                return ApplicationErrors.DepartmentNotFound;
            }

            var target = await ResolveTargetAsync(dto.TargetType, dto.TargetId);
            if (target.IsError)
            {
                return target.Errors;
            }

            if (dto.TargetType == ArchiveDeletionTargetType.Folder)
            {
                var hasSubFolders = await unitOfWork.Context.Folders
                    .AnyAsync(f => f.ParentId == dto.TargetId && !f.IsDeleted);

                var hasRecords = await unitOfWork.Context.ArchiveRecords
                    .AnyAsync(r => r.FolderId == dto.TargetId && !r.IsDeleted);

                if (hasSubFolders || hasRecords)
                {
                    return ApplicationErrors.FolderHasChildren;
                }
            }

            if (target.Value.departmentId is null)
            {
                return dto.TargetType == ArchiveDeletionTargetType.Folder
                    ? ApplicationErrors.FolderDepartmentNotConfigured
                    : ApplicationErrors.ArchiveRecordDepartmentNotConfigured;
            }

            if (target.Value.departmentId != requesterResult.Value.DepartmentId)
            {
                return ApplicationErrors.InvalidInput;
            }

            var leaderCheck = await archiveLeaderService.IsArchiveLeaderAsync(requesterId, target.Value.departmentId.Value);
            if (leaderCheck.IsError)
            {
                return leaderCheck.Errors;
            }

            if (leaderCheck.Value)
            {
                return dto.TargetType == ArchiveDeletionTargetType.Folder
                    ? ApplicationErrors.FolderArchiveLeaderRequired
                    : ApplicationErrors.ArchiveRecordArchiveLeaderRequired;
            }

            var departmentResult = await unitOfWork.Departments.GetAsync(x => x.Id == target.Value.departmentId.Value, x => x.Include(d => d.DepartmentHead));
            if (departmentResult.IsError || departmentResult.Value == null)
            {
                return ApplicationErrors.DepartmentNotFound;
            }

            if (departmentResult.Value.DepartmentHeadId is null)
            {
                return ApplicationErrors.DepartmentHeadMissing;
            }

            var existingPending = await unitOfWork.DeleteArchiveRequests.AnyAsync(x =>
                x.DepartmentId == target.Value.departmentId.Value &&
                x.TargetType == dto.TargetType &&
                x.TargetId == dto.TargetId &&
                x.Status == DeleteArchiveRequestStatus.Pending);

            if (existingPending)
            {
                return dto.TargetType == ArchiveDeletionTargetType.Folder
                    ? ApplicationErrors.FolderDeleteRequestExists
                    : ApplicationErrors.DeleteRequestAlreadyHandled;
            }

            var snapshot = await BuildSnapshotAsync(dto.TargetType, dto.TargetId, target.Value.departmentId.Value);
            var dependencies = await BuildDependenciesAsync(dto.TargetType, dto.TargetId, target.Value.departmentId.Value);
            var activity = BuildActivitySnapshot(dto.TargetType, dto.TargetId, requesterResult.Value);

            var request = new DeleteArchiveRequest
            {
                Id = Guid.NewGuid(),
                DepartmentId = target.Value.departmentId.Value,
                TargetType = dto.TargetType,
                TargetId = dto.TargetId,
                RequesterId = requesterId,
                ApproverId = departmentResult.Value.DepartmentHeadId.Value,
                Status = DeleteArchiveRequestStatus.Pending,
                Justification = dto.Justification.Trim(),
                TargetSnapshotJson = JsonSerializer.Serialize(snapshot, JsonOptions),
                DependenciesSnapshotJson = JsonSerializer.Serialize(dependencies, JsonOptions),
                ActivitySnapshotJson = JsonSerializer.Serialize(activity, JsonOptions),
                SourceFolderId = target.Value.sourceFolderId,
                TargetDisplayName = snapshot.DisplayName,
                RowVersion = Guid.NewGuid().ToByteArray()
            };

            var addResult = await unitOfWork.DeleteArchiveRequests.AddAsync(request);
            if (addResult.IsError)
            {
                return addResult.Errors;
            }

            var saveResult = await unitOfWork.SaveChangesAsync();
            if (saveResult <= 0)
            {
                return ApplicationErrors.DatabaseError;
            }

            logger.LogInformation("Submitted archive delete request {RequestId} for {TargetType}:{TargetId}", request.Id, dto.TargetType, dto.TargetId);

            if (dto.TargetType == ArchiveDeletionTargetType.Record)
            {
                var ipAddress = httpContextServiceManager.GetClientIpAddress();
                var userAgent = httpContextServiceManager.GetUserAgent();
                await auditLogService.LogAsync(dto.TargetId, requesterId.ToString(), AuditAction.SubmitDeleteRequest, $"Submitted delete request for archive record", ipAddress, userAgent);
            }

            return request.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error submitting archive delete request for {TargetType}:{TargetId}", dto.TargetType, dto.TargetId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<DeleteArchiveRequestDto>> GetByIdAsync(Guid requestId)
    {
        try
        {
            if (requestId == Guid.Empty)
            {
                return ApplicationErrors.InvalidInput;
            }

            var requestResult = await unitOfWork.DeleteArchiveRequests.GetAsync(
                x => x.Id == requestId,
                query => query.Include(x => x.Department).Include(x => x.Requester).Include(x => x.Approver));

            if (requestResult.IsError)
            {
                return requestResult.Errors;
            }

            if (requestResult.Value == null)
            {
                return ApplicationErrors.DeleteRequestNotFound;
            }

            return requestResult.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching archive delete request {RequestId}", requestId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<DeleteArchiveRequestDto>>> GetPendingForDepartmentAsync(Guid departmentId, int page = 1, int pageSize = 20)
    {
        try
        {
            if (departmentId == Guid.Empty || page <= 0 || pageSize <= 0 || pageSize > 100)
            {
                return ApplicationErrors.InvalidInput;
            }

            var result = await unitOfWork.DeleteArchiveRequests.GetPagedAsync(
                page,
                pageSize,
                filter: x => x.DepartmentId == departmentId && x.Status == DeleteArchiveRequestStatus.Pending,
                transform: query => query.Include(x => x.Department).Include(x => x.Requester).Include(x => x.Approver));

            if (result.IsError)
            {
                return result.Errors;
            }

            var items = result.Value!.Items.Select(x => x.ToDto()).ToList();
            return new PagedList<DeleteArchiveRequestDto>(items, result.Value.TotalItems, page, pageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching delete requests for department {DepartmentId}", departmentId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<DeleteArchiveRequestDto>> ApproveAsync(Guid requestId, string? notes = null)
    {
        try
        {
            if (requestId == Guid.Empty)
            {
                return ApplicationErrors.InvalidInput;
            }

            var currentUserId = httpContextServiceManager.GetCurrentUserId();
            var requestResult = await unitOfWork.DeleteArchiveRequests.GetAsync(
                x => x.Id == requestId,
                query => query.Include(x => x.Department).Include(x => x.Requester).Include(x => x.Approver));

            if (requestResult.IsError)
            {
                return requestResult.Errors;
            }

            var request = requestResult.Value;
            if (request == null)
            {
                return ApplicationErrors.DeleteRequestNotFound;
            }

            var headCheck = await archiveAuthorizationService.IsDepartmentHeadAsync(currentUserId, request.DepartmentId);
            if (headCheck.IsError || !headCheck.Value)
            {
                return ApplicationErrors.DeleteRequestApprovalRequiresDepartmentHead;
            }

            if (request.Status is DeleteArchiveRequestStatus.Rejected or DeleteArchiveRequestStatus.Executed)
            {
                return ApplicationErrors.DeleteRequestAlreadyHandled;
            }

            await unitOfWork.BeginTransactionAsync();

            request.Status = DeleteArchiveRequestStatus.Approved;
            request.ApprovedByUserId = currentUserId;
            request.ApprovedAt = DateTime.UtcNow;
            request.ApprovalNotes = notes;

            var executionResult = request.TargetType switch
            {
                ArchiveDeletionTargetType.Folder => await SoftDeleteFolderTreeAsync(request.TargetId, request.Id, "System"),
                ArchiveDeletionTargetType.Record => await SoftDeleteRecordAsync(request.TargetId, request.Id, "System", "System"),
                _ => ApplicationErrors.InvalidInput
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

            // Removed manual RowVersion update since it conflicts with ValueGeneratedOnAddOrUpdate in EF Core

            var saveResult = await unitOfWork.SaveChangesAsync();
            if (saveResult <= 0)
            {
                await unitOfWork.RollbackTransactionAsync();
                return ApplicationErrors.DatabaseError;
            }

            await unitOfWork.CommitTransactionAsync();
            logger.LogInformation("Approved and executed archive delete request {RequestId}", requestId);

            if (request.TargetType == ArchiveDeletionTargetType.Record)
            {
                var ipAddress = httpContextServiceManager.GetClientIpAddress();
                var userAgent = httpContextServiceManager.GetUserAgent();
                await auditLogService.LogAsync(request.TargetId, currentUserId.ToString(), AuditAction.ApproveDelete, $"الموافقة على طلب حذف سجل أرشيفي{(notes != null ? $": {notes}" : "")}", ipAddress, userAgent);
            }

            return request.ToDto();
        }
        catch (Exception ex)
        {
            if (unitOfWork.HasActiveTransaction)
            {
                await unitOfWork.RollbackTransactionAsync();
            }

            logger.LogError(ex, "Error approving delete request {RequestId}", requestId);
            return new Error("500", $"حدث خطأ أثناء الموافقة على طلب الحذف: {ex.Message} - {ex.InnerException?.Message}", ErrorKind.Failure);
        }
    }

    public async Task<Result<DeleteArchiveRequestDto>> RejectAsync(Guid requestId, string reason)
    {
        try
        {
            if (requestId == Guid.Empty || string.IsNullOrWhiteSpace(reason))
            {
                return ApplicationErrors.DeleteRequestRejectionRequiresReason;
            }

            var currentUserId = httpContextServiceManager.GetCurrentUserId();
            var requestResult = await unitOfWork.DeleteArchiveRequests.GetAsync(
                x => x.Id == requestId,
                query => query.Include(x => x.Department).Include(x => x.Requester).Include(x => x.Approver));

            if (requestResult.IsError)
            {
                return requestResult.Errors;
            }

            var request = requestResult.Value;
            if (request == null)
            {
                return ApplicationErrors.DeleteRequestNotFound;
            }

            var headCheck = await archiveAuthorizationService.IsDepartmentHeadAsync(currentUserId, request.DepartmentId);
            if (headCheck.IsError || !headCheck.Value)
            {
                return ApplicationErrors.DeleteRequestApprovalRequiresDepartmentHead;
            }

            if (request.Status is DeleteArchiveRequestStatus.Rejected or DeleteArchiveRequestStatus.Executed)
            {
                return ApplicationErrors.DeleteRequestAlreadyHandled;
            }

            request.Status = DeleteArchiveRequestStatus.Rejected;
            request.RejectedByUserId = currentUserId;
            request.RejectedAt = DateTime.UtcNow;
            request.RejectionReason = reason.Trim();
            request.RequesterNotificationMessage = reason.Trim();
            request.RequesterNotifiedAt = DateTime.UtcNow;

            // Removed manual RowVersion update since it conflicts with ValueGeneratedOnAddOrUpdate in EF Core

            var saveResult = await unitOfWork.SaveChangesAsync();
            if (saveResult <= 0)
            {
                return ApplicationErrors.DatabaseError;
            }

            logger.LogInformation("Rejected archive delete request {RequestId}", requestId);

            if (request.TargetType == ArchiveDeletionTargetType.Record)
            {
                var ipAddress = httpContextServiceManager.GetClientIpAddress();
                var userAgent = httpContextServiceManager.GetUserAgent();
                await auditLogService.LogAsync(request.TargetId, currentUserId.ToString(), AuditAction.RejectDelete, $"Rejected delete request for archive record: {reason}", ipAddress, userAgent);
            }

            return request.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error rejecting delete request {RequestId}", requestId);
            return new Error("500", $"حدث خطأ أثناء رفض طلب الحذف: {ex.Message} - {ex.InnerException?.Message}", ErrorKind.Failure);
        }
    }

    public async Task<Result<bool>> DeleteFolderAsync(Guid folderId, Guid? requestId = null)
    {
        try
        {
            if (folderId == Guid.Empty)
            {
                return ApplicationErrors.InvalidInput;
            }

            var hasSubFolders = await unitOfWork.Context.Folders
                .AnyAsync(f => f.ParentId == folderId && !f.IsDeleted);

            var hasRecords = await unitOfWork.Context.ArchiveRecords
                .AnyAsync(r => r.FolderId == folderId && !r.IsDeleted);

            if (hasSubFolders || hasRecords)
            {
                return ApplicationErrors.FolderHasChildren;
            }

            var departmentResult = await archiveAuthorizationService.ResolveFolderDepartmentIdAsync(folderId);
            if (departmentResult.IsError)
            {
                return departmentResult.Errors;
            }

            if (!departmentResult.Value.HasValue)
            {
                return ApplicationErrors.FolderDepartmentNotConfigured;
            }

            var userId = httpContextServiceManager.GetCurrentUserId();
            var isLeader = await archiveLeaderService.IsArchiveLeaderAsync(userId, departmentResult.Value.Value);
            if (isLeader.IsError || !isLeader.Value)
            {
                return ApplicationErrors.FolderArchiveLeaderRequired;
            }

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
                return ApplicationErrors.DatabaseError;
            }

            await unitOfWork.CommitTransactionAsync();
            return true;
        }
        catch (Exception ex)
        {
            if (unitOfWork.HasActiveTransaction)
            {
                await unitOfWork.RollbackTransactionAsync();
            }

            logger.LogError(ex, "Error deleting folder {FolderId}", folderId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> DeleteArchiveRecordAsync(Guid recordId, Guid? requestId = null)
    {
        try
        {
            if (recordId == Guid.Empty)
            {
                return ApplicationErrors.InvalidInput;
            }

            var departmentResult = await archiveAuthorizationService.ResolveArchiveRecordDepartmentIdAsync(recordId);
            if (departmentResult.IsError)
            {
                return departmentResult.Errors;
            }

            if (!departmentResult.Value.HasValue)
            {
                return ApplicationErrors.ArchiveRecordDepartmentNotConfigured;
            }

            var userId = httpContextServiceManager.GetCurrentUserId();
            var isLeader = await archiveLeaderService.IsArchiveLeaderAsync(userId, departmentResult.Value.Value);
            if (isLeader.IsError || !isLeader.Value)
            {
                return ApplicationErrors.ArchiveRecordArchiveLeaderRequired;
            }

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
                return ApplicationErrors.DatabaseError;
            }

            await unitOfWork.CommitTransactionAsync();

            var ipAddress = httpContextServiceManager.GetClientIpAddress();
            var userAgent = httpContextServiceManager.GetUserAgent();
            await auditLogService.LogAsync(recordId, userId.ToString(), AuditAction.Delete, "Deleted archive record", ipAddress, userAgent);

            return true;
        }
        catch (Exception ex)
        {
            if (unitOfWork.HasActiveTransaction)
            {
                await unitOfWork.RollbackTransactionAsync();
            }

            logger.LogError(ex, "Error deleting archive record {RecordId}", recordId);
            return ApplicationErrors.InternalServerError;
        }
    }

    private async Task<Result<(Guid? departmentId, Guid? sourceFolderId, string displayName)>> ResolveTargetAsync(ArchiveDeletionTargetType targetType, Guid targetId)
    {
        return targetType switch
        {
            ArchiveDeletionTargetType.Folder => await ResolveFolderTargetAsync(targetId),
            ArchiveDeletionTargetType.Record => await ResolveRecordTargetAsync(targetId),
            _ => ApplicationErrors.InvalidInput
        };
    }

    private async Task<Result<(Guid? departmentId, Guid? sourceFolderId, string displayName)>> ResolveFolderTargetAsync(Guid folderId)
    {
        var folder = await unitOfWork.Context.Folders
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.Id == folderId)
            .Select(x => new { x.DepartmentId, x.ParentId, x.Name })
            .SingleOrDefaultAsync();

        if (folder == null)
        {
            return ApplicationErrors.FolderNotFound;
        }

        var departmentId = folder.DepartmentId;
        if (!departmentId.HasValue && folder.ParentId.HasValue)
        {
            var resolved = await archiveAuthorizationService.ResolveFolderDepartmentIdAsync(folder.ParentId.Value);
            if (!resolved.IsError)
            {
                departmentId = resolved.Value;
            }
        }

        return (departmentId, folder.ParentId, folder.Name);
    }

    private async Task<Result<(Guid? departmentId, Guid? sourceFolderId, string displayName)>> ResolveRecordTargetAsync(Guid recordId)
    {
        var record = await unitOfWork.Context.ArchiveRecords
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.Id == recordId)
            .Select(x => new { x.DepartmentId, x.FolderId })
            .SingleOrDefaultAsync();

        if (record == null)
        {
            return ApplicationErrors.ArchiveRecordNotFound;
        }

        var departmentId = record.DepartmentId;
        if (!departmentId.HasValue)
        {
            var resolved = await archiveAuthorizationService.ResolveFolderDepartmentIdAsync(record.FolderId);
            if (!resolved.IsError)
            {
                departmentId = resolved.Value;
            }
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
        var folder = await unitOfWork.Context.Folders
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.Id == folderId)
            .Select(x => new { x.Id, x.Name, x.DepartmentId, x.ParentId, x.CreatedAt, x.UpdatedAt, x.CreatedByUserId, x.UpdatedByUserId })
            .SingleAsync();

        var childrenCount = await unitOfWork.Context.Folders.IgnoreQueryFilters().CountAsync(x => x.ParentId == folderId);
        var descendantCount = await CountDescendantsAsync(folderId);
        var recordCount = await unitOfWork.Context.ArchiveRecords.IgnoreQueryFilters().CountAsync(x => x.FolderId == folderId);
        var fileCount = await unitOfWork.Context.PhysicalFiles.CountAsync(x => x.ArchiveRecord.FolderId == folderId);

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
        var record = await unitOfWork.Context.ArchiveRecords
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.Id == recordId)
            .Select(x => new { x.Id, x.FolderId, x.CreatedAt, x.UpdatedAt, x.CreatedByUserId, x.UpdatedByUserId })
            .SingleAsync();

        var fileCount = await unitOfWork.Context.PhysicalFiles.CountAsync(x => x.ArchiveRecordId == recordId);

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

        var children = await unitOfWork.Context.Folders
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.ParentId == folderId)
            .Select(x => new { x.Id, x.Name })
            .ToListAsync();

        foreach (var child in children)
        {
            list.Add(new ArchiveDeletionDependencyDto("child-folder", child.Id, child.Name, "Child folder will be soft-deleted."));
        }

        var records = await unitOfWork.Context.ArchiveRecords
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.FolderId == folderId)
            .Select(x => new { x.Id })
            .ToListAsync();

        foreach (var record in records)
        {
            list.Add(new ArchiveDeletionDependencyDto("archive-record", record.Id, string.Empty, "Archive record will be soft-deleted."));
        }

        var leaders = await unitOfWork.Context.DepartmentArchiveLeaders
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.DepartmentId == departmentId && !x.IsDeleted)
            .Select(x => new { x.Id, x.UserId })
            .ToListAsync();

        foreach (var leader in leaders)
        {
            list.Add(new ArchiveDeletionDependencyDto("archive-leader", leader.Id, leader.UserId.ToString(), "Leader assignment protects direct deletion only."));
        }

        return list;
    }

    private async Task<List<ArchiveDeletionDependencyDto>> BuildRecordDependenciesAsync(Guid recordId)
    {
        var list = new List<ArchiveDeletionDependencyDto>();
        var files = await unitOfWork.Context.PhysicalFiles
            .Where(x => x.ArchiveRecordId == recordId)
            .AsNoTracking()
            .Select(x => new { x.Id, x.FileName, x.FileExtension })
            .ToListAsync();

        foreach (var file in files)
        {
            list.Add(new ArchiveDeletionDependencyDto("physical-file", file.Id, file.FileName, file.FileExtension));
        }

        return list;
    }

    private object BuildActivitySnapshot(ArchiveDeletionTargetType targetType, Guid targetId, User requester)
        => new
        {
            targetType,
            targetId,
            requester.Id,
            requester.UserName,
            requester.DepartmentId,
            RequestedAtUtc = DateTime.UtcNow
        };

    private async Task<Result<bool>> SoftDeleteFolderTreeAsync(Guid folderId, Guid? requestId, string deletedByUserId)
    {
        var exists = await unitOfWork.Context.Folders.IgnoreQueryFilters().AnyAsync(x => x.Id == folderId);
        if (!exists)
        {
            return ApplicationErrors.FolderNotFound;
        }

        await SoftDeleteFolderRecursiveAsync(folderId, requestId, deletedByUserId);
        return true;
    }

    private async Task SoftDeleteFolderRecursiveAsync(Guid folderId, Guid? requestId, string deletedByUserId)
    {
        var folder = await unitOfWork.Context.Folders
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
            {
                await SoftDeleteRecordAsync(record.Id, requestId, deletedByUserId, "System");
            }
        }

        foreach (var child in folder.SubFolders)
        {
            await SoftDeleteFolderRecursiveAsync(child.Id, requestId, deletedByUserId);
        }

        var updateResult = await unitOfWork.Folders.UpdateAsync(folder);
        if (updateResult.IsError)
        {
            throw new InvalidOperationException(updateResult.Errors.First().Description);
        }
    }

    private async Task<Result<bool>> SoftDeleteRecordAsync(Guid recordId, Guid? requestId, string deletedByUserId, string? deletedByLabel = null)
    {
        var record = await unitOfWork.Context.ArchiveRecords
            .IgnoreQueryFilters()
            .Include(x => x.PhysicalFiles)
            .SingleOrDefaultAsync(x => x.Id == recordId);

        if (record == null)
        {
            return ApplicationErrors.ArchiveRecordNotFound;
        }

        await SoftDeleteRecordAsync(record, requestId, deletedByUserId, deletedByLabel);
        return true;
    }

    private async Task SoftDeleteRecordAsync(ArchiveRecord record, Guid? requestId, string deletedByUserId, string? deletedByLabel = null)
    {
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
            {
                throw new InvalidOperationException(fileUpdate.Errors.First().Description);
            }
        }

        var updateResult = await unitOfWork.ArchiveRecords.UpdateAsync(record);
        if (updateResult.IsError)
        {
            throw new InvalidOperationException(updateResult.Errors.First().Description);
        }
    }

    private async Task<int> CountDescendantsAsync(Guid folderId)
    {
        var descendants = 0;
        var children = await unitOfWork.Context.Folders.IgnoreQueryFilters().AsNoTracking().Where(x => x.ParentId == folderId).Select(x => x.Id).ToListAsync();
        foreach (var childId in children)
        {
            descendants++;
            descendants += await CountDescendantsAsync(childId);
        }

        return descendants;
    }
}
