using Microsoft.Extensions.Logging;
using ModernPaySystem.Module.Archive.Application;
using ModernPaySystem.Module.Archive.Application.Interfaces;
using ModernPaySystem.Module.Archive.Domain;
using ModernPaySystem.Module.Archive.Domain.Entities;
using ModernPaySystem.SharedKernel.Application.Services;
using ModernPaySystem.SharedKernel.Domain.Commons;

namespace ModernPaySystem.Module.Archive.Infrastructure.Services;

public class ArchiveLeaderService(
    IArchiveUnitOfWork unitOfWork,
    ILogger<ArchiveLeaderService> logger,
    IHttpContextServiceManager httpContextServiceManager) : IArchiveLeaderService
{
    public async Task<Result<IEnumerable<ArchiveLeaderAssignmentDto>>> GetByDepartmentAsync(Guid departmentId)
    {
        try
        {
            if (departmentId == Guid.Empty)
                return ArchiveErrors.InvalidInput;

            var itemsResult = await unitOfWork.DepartmentArchiveLeaders.GetAllAsync(
                x => x.DepartmentId == departmentId);

            if (itemsResult.IsError)
                return itemsResult.Errors;

            return itemsResult.Value!.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching archive leaders for department {DepartmentId}", departmentId);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<ArchiveLeaderAssignmentDto>> AssignAsync(Guid departmentId, Guid userId)
    {
        try
        {
            if (departmentId == Guid.Empty || userId == Guid.Empty)
                return ArchiveErrors.InvalidInput;

            var assignment = new DepartmentArchiveLeader
            {
                Id = Guid.NewGuid(),
                DepartmentId = departmentId,
                UserId = userId
            };

            var addResult = await unitOfWork.DepartmentArchiveLeaders.AddAsync(assignment);
            if (addResult.IsError)
                return addResult.Errors;

            var saveResult = await unitOfWork.SaveChangesAsync();
            if (saveResult <= 0)
                return ArchiveErrors.DatabaseError;

            return MapToDto(assignment);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error assigning archive leader {UserId} to department {DepartmentId}", userId, departmentId);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> UnassignAsync(Guid departmentId, Guid userId)
    {
        try
        {
            if (departmentId == Guid.Empty || userId == Guid.Empty)
                return ArchiveErrors.InvalidInput;

            var assignmentResult = await unitOfWork.DepartmentArchiveLeaders.GetAsync(
                x => x.DepartmentId == departmentId && x.UserId == userId);

            if (assignmentResult.IsError || assignmentResult.Value is null)
                return true;

            var assignment = assignmentResult.Value;
            assignment.IsDeleted = true;
            assignment.DeletedAt = DateTime.UtcNow;
            assignment.DeletedByUserId = httpContextServiceManager.GetCurrentUserId().ToString();

            var updateResult = await unitOfWork.DepartmentArchiveLeaders.UpdateAsync(assignment);
            if (updateResult.IsError)
                return updateResult.Errors;

            var saveResult = await unitOfWork.SaveChangesAsync();
            if (saveResult <= 0)
                return ArchiveErrors.DatabaseError;

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error unassigning archive leader {UserId} from department {DepartmentId}", userId, departmentId);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> RevokeAssignmentsForUserAsync(Guid userId, Guid? departmentId = null)
    {
        try
        {
            if (userId == Guid.Empty)
                return ArchiveErrors.InvalidInput;

            var currentUserId = httpContextServiceManager.GetCurrentUserId().ToString();
            var assignmentsResult = await unitOfWork.DepartmentArchiveLeaders.GetAllAsync(
                x => x.UserId == userId && (!departmentId.HasValue || x.DepartmentId == departmentId.Value));

            if (assignmentsResult.IsError)
                return assignmentsResult.Errors;

            foreach (var assignment in assignmentsResult.Value!)
            {
                assignment.IsDeleted = true;
                assignment.DeletedAt = DateTime.UtcNow;
                assignment.DeletedByUserId = currentUserId;

                var updateResult = await unitOfWork.DepartmentArchiveLeaders.UpdateAsync(assignment);
                if (updateResult.IsError)
                    return updateResult.Errors;
            }

            var saveResult = await unitOfWork.SaveChangesAsync();
            if (saveResult <= 0)
                return ArchiveErrors.DatabaseError;

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error revoking archive leader assignments for user {UserId}", userId);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> IsArchiveLeaderAsync(Guid userId, Guid departmentId)
    {
        try
        {
            if (userId == Guid.Empty || departmentId == Guid.Empty)
                return ArchiveErrors.InvalidInput;

            var exists = await unitOfWork.DepartmentArchiveLeaders.AnyAsync(
                x => x.UserId == userId && x.DepartmentId == departmentId);

            return exists;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking archive leader status for user {UserId} in department {DepartmentId}", userId, departmentId);
            return ArchiveErrors.InternalServerError;
        }
    }

    private static ArchiveLeaderAssignmentDto MapToDto(DepartmentArchiveLeader assignment)
        => new()
        {
            Id = assignment.Id,
            DepartmentId = assignment.DepartmentId,
            UserId = assignment.UserId,
            UserName = assignment.User?.UserName,
            IsDeleted = assignment.IsDeleted,
            DeletedAt = assignment.DeletedAt,
            DeletedByUserId = assignment.DeletedByUserId,
            CreatedByUserId = assignment.CreatedByUserId,
            CreatedAt = assignment.CreatedAt,
            UpdatedByUserId = assignment.UpdatedByUserId,
            UpdatedAt = assignment.UpdatedAt
        };
}
