using ModernPaySystem.Domain.Entities.Archiving;

namespace ModernPaySystem.Application.Interfaces;

public interface IFolderIconService
{
    Task<Result<List<FolderIconDto>>> GetAllAsync();
    Task<Result<FolderIconDto>> GetByIdAsync(Guid id);
    Task<Result<FolderIconDto>> CreateAsync(CreateFolderIconDto dto);
    Task<Result<FolderIconDto>> UpdateAsync(Guid id, UpdateFolderIconDto dto);
    Task<Result<bool>> DeleteAsync(Guid id);
    Task<Result<string>> GetIconSvgAsync(Guid id);
    Task<Result<FolderDto>> AssignIconToFolderAsync(Guid folderId, Guid? iconId);
}
