using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Application.Interfaces;
using ModernPaySystem.Application.Services;
using ModernPaySystem.Domain.Entities.Archiving;

namespace ModernPaySystem.Infrastructure.Services;

public class ArchiveLeaderService(
    IUnitOfWork unitOfWork,
    IHttpContextServiceManager httpContextServiceManager,
    ILogger<ArchiveLeaderService> logger) : IArchiveLeaderService
{
    public async Task<Result<IEnumerable<ArchiveLeaderAssignmentDto>>> GetByDepartmentAsync(Guid departmentId)
    {
        try
        {
            if (departmentId == Guid.Empty)
            {
                return ApplicationErrors.InvalidInput;
            }

            var items = await unitOfWork.DepartmentArchiveLeaders.GetAllAsync(
                x => x.DepartmentId == departmentId && !x.IsDeleted,
                query => query.Include(x => x.User));

            if (items.IsError)
            {
                return items.Errors;
            }

            return items.Value!.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching archive leaders for department {DepartmentId}", departmentId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<ArchiveLeaderAssignmentDto>> AssignAsync(Guid departmentId, Guid userId)
    {
        try
        {
            if (departmentId == Guid.Empty || userId == Guid.Empty)
            {
                return ApplicationErrors.InvalidInput;
            }

            var departmentResult = await unitOfWork.Departments.GetByIdAsync(departmentId);
            if (departmentResult.IsError || departmentResult.Value == null)
            {
                return ApplicationErrors.DepartmentNotFound;
            }

            var userResult = await unitOfWork.Users.GetByIdAsync(userId);
            if (userResult.IsError || userResult.Value == null)
            {
                return ApplicationErrors.UserNotFound;
            }

            if (userResult.Value.DepartmentId != departmentId)
            {
                return ApplicationErrors.InvalidInput;
            }

            var currentUserId = httpContextServiceManager.GetCurrentUserId().ToString();
            await unitOfWork.BeginTransactionAsync();

            var existing = await unitOfWork.Context.DepartmentArchiveLeaders
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.DepartmentId == departmentId && x.UserId == userId);

            if (existing is null)
            {
                var assignment = new DepartmentArchiveLeader
                {
                    Id = Guid.NewGuid(),
                    DepartmentId = departmentId,
                    UserId = userId,
                    IsDeleted = false
                };

                var addResult = await unitOfWork.DepartmentArchiveLeaders.AddAsync(assignment);
                if (addResult.IsError)
                {
                    await unitOfWork.RollbackTransactionAsync();
                    return addResult.Errors;
                }

                var saveResult = await unitOfWork.SaveChangesAsync();
                if (saveResult <= 0)
                {
                    await unitOfWork.RollbackTransactionAsync();
                    return ApplicationErrors.DatabaseError;
                }

                await unitOfWork.CommitTransactionAsync();
                return MapToDto(assignment);
            }

            if (!existing.IsDeleted)
            {
                await unitOfWork.RollbackTransactionAsync();
                return MapToDto(existing);
            }

            existing.IsDeleted = false;
            existing.DeletedAt = null;
            existing.DeletedByUserId = null;

            var updateResult = await unitOfWork.DepartmentArchiveLeaders.UpdateAsync(existing);
            if (updateResult.IsError)
            {
                await unitOfWork.RollbackTransactionAsync();
                return updateResult.Errors;
            }

            var updated = await unitOfWork.SaveChangesAsync();
            if (updated <= 0)
            {
                await unitOfWork.RollbackTransactionAsync();
                return ApplicationErrors.DatabaseError;
            }

            await unitOfWork.CommitTransactionAsync();
            return MapToDto(existing);
        }
        catch (Exception ex)
        {
            if (unitOfWork.HasActiveTransaction)
            {
                await unitOfWork.RollbackTransactionAsync();
            }

            logger.LogError(ex, "Error assigning archive leader {UserId} to department {DepartmentId}", userId, departmentId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> UnassignAsync(Guid departmentId, Guid userId)
    {
        try
        {
            if (departmentId == Guid.Empty || userId == Guid.Empty)
            {
                return ApplicationErrors.InvalidInput;
            }

            var currentUserId = httpContextServiceManager.GetCurrentUserId().ToString();
            await unitOfWork.BeginTransactionAsync();

            var assignment = await unitOfWork.Context.DepartmentArchiveLeaders
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.DepartmentId == departmentId && x.UserId == userId && !x.IsDeleted);

            if (assignment == null)
            {
                await unitOfWork.RollbackTransactionAsync();
                return true;
            }

            assignment.IsDeleted = true;
            assignment.DeletedAt = DateTime.UtcNow;
            assignment.DeletedByUserId = currentUserId;

            var updateResult = await unitOfWork.DepartmentArchiveLeaders.UpdateAsync(assignment);
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
            return true;
        }
        catch (Exception ex)
        {
            if (unitOfWork.HasActiveTransaction)
            {
                await unitOfWork.RollbackTransactionAsync();
            }

            logger.LogError(ex, "Error unassigning archive leader {UserId} from department {DepartmentId}", userId, departmentId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> RevokeAssignmentsForUserAsync(Guid userId, Guid? departmentId = null)
    {
        try
        {
            if (userId == Guid.Empty)
            {
                return ApplicationErrors.InvalidInput;
            }

            var currentUserId = httpContextServiceManager.GetCurrentUserId().ToString();
            await unitOfWork.BeginTransactionAsync();

            var assignments = await unitOfWork.Context.DepartmentArchiveLeaders
                .IgnoreQueryFilters()
                .Where(x => x.UserId == userId && !x.IsDeleted && (!departmentId.HasValue || x.DepartmentId == departmentId.Value))
                .ToListAsync();

            foreach (var assignment in assignments)
            {
                assignment.IsDeleted = true;
                assignment.DeletedAt = DateTime.UtcNow;
                assignment.DeletedByUserId = currentUserId;
                var updateResult = await unitOfWork.DepartmentArchiveLeaders.UpdateAsync(assignment);
                if (updateResult.IsError)
                {
                    await unitOfWork.RollbackTransactionAsync();
                    return updateResult.Errors;
                }
            }

            var saveResult = await unitOfWork.SaveChangesAsync();
            if (saveResult <= 0)
            {
                await unitOfWork.RollbackTransactionAsync();
                return true;
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

            logger.LogError(ex, "Error revoking archive leader assignments for user {UserId}", userId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> IsArchiveLeaderAsync(Guid userId, Guid departmentId)
        => await unitOfWork.DepartmentArchiveLeaders.AnyAsync(x => x.UserId == userId && x.DepartmentId == departmentId && !x.IsDeleted);

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
