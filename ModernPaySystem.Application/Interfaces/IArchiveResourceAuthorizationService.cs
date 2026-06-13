using ModernPaySystem.Domain.Entities.Archiving;

namespace ModernPaySystem.Application.Interfaces;

public interface IArchiveResourceAuthorizationService
{
    Task<Result<bool>> CanAccessFolderAsync(Guid userId, Guid folderId, AccessLevel minimumLevel = AccessLevel.View);
    Task<Result<bool>> CanAccessArchiveRecordAsync(Guid userId, Guid recordId, AccessLevel minimumLevel = AccessLevel.View);
    Task<Result<bool>> CanAccessPhysicalFileAsync(Guid userId, Guid fileId, AccessLevel minimumLevel = AccessLevel.View);
    Task<Result<AccessLevel>> GetFolderAccessLevelAsync(Guid userId, Guid folderId);
    Task<Result<List<Guid>>> GetAccessibleFolderIdsAsync(Guid userId, AccessLevel minimumLevel = AccessLevel.View);
}
