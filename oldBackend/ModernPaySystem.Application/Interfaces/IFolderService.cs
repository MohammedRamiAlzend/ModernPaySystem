using ModernPaySystem.Domain.Entities.Archiving;

namespace ModernPaySystem.Application.Interfaces;

public interface IFolderService
{
    Task<Result<IEnumerable<FolderDto>>> GetAllAsync();
    Task<Result<FolderDto>> GetByIdAsync(Guid id);
    Task<Result<FolderDto>> CreateAsync(CreateFolderDto dto);
    Task<Result<FolderDto>> UpdateAsync(Guid id, UpdateFolderDto dto);
    Task<Result<FolderDto>> MoveFolderAsync(Guid id, Guid destinationFolderId);
    Task<Result<bool>> DeleteAsync(Guid id);

    Task<Result<List<FolderPermissionDto>>> GetPermissionsByFolderAsync(Guid folderId);
    Task<Result<FolderPermissionDto>> GetPermissionByIdAsync(Guid id);
    Task<Result<FolderPermissionDto>> CreatePermissionAsync(CreateFolderPermissionDto dto);
    Task<Result<FolderPermissionDto>> UpdatePermissionAsync(Guid id, UpdateFolderPermissionDto dto);
    Task<Result<bool>> DeletePermissionAsync(Guid id);
}
