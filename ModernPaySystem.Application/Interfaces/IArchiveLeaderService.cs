using ModernPaySystem.Domain.Entities.Archiving;

namespace ModernPaySystem.Application.Interfaces;

public interface IArchiveLeaderService
{
    Task<Result<IEnumerable<ArchiveLeaderAssignmentDto>>> GetByDepartmentAsync(Guid departmentId);
    Task<Result<ArchiveLeaderAssignmentDto>> AssignAsync(Guid departmentId, Guid userId);
    Task<Result<bool>> UnassignAsync(Guid departmentId, Guid userId);
    Task<Result<bool>> RevokeAssignmentsForUserAsync(Guid userId, Guid? departmentId = null);
    Task<Result<bool>> IsArchiveLeaderAsync(Guid userId, Guid departmentId);
}
