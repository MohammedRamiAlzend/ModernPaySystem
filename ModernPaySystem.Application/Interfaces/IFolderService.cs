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
}
