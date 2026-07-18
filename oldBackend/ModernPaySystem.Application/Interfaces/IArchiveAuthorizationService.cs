namespace ModernPaySystem.Application.Interfaces;

public interface IArchiveAuthorizationService
{
    Task<Result<bool>> IsArchiveLeaderAsync(Guid userId, Guid departmentId);
    Task<Result<bool>> IsDepartmentHeadAsync(Guid userId, Guid departmentId);
    Task<Result<Guid?>> ResolveFolderDepartmentIdAsync(Guid folderId);
    Task<Result<Guid?>> ResolveArchiveRecordDepartmentIdAsync(Guid recordId);
    Task<Result<List<Guid>>> GetUserArchiveLeaderDepartmentsAsync(Guid userId);
}
