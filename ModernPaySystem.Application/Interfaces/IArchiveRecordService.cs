using Microsoft.AspNetCore.Http;
using ModernPaySystem.Domain.Entities.Archiving;

namespace ModernPaySystem.Application.Interfaces;

public interface IArchiveRecordService
{
    //Task<Result<IEnumerable<ArchiveRecordDto>>> GetAllAsync();
    //Task<Result<PagedList<ArchiveRecordDto>>> GetPagedAsync(int page, int pageSize);
    Task<Result<ArchiveRecordDto>> GetByIdAsync(Guid id);
    Task<Result<PagedList<ArchiveRecordDto>>> GetByFolderIdAsync(Guid folderId, int page, int pageSize);
    Task<Result<PagedList<ArchiveRecordDto>>> GetByFormIdAsync(Guid formId, int page, int pageSize);
    Task<Result<ArchiveRecordDto>> CreateAsync(CreateArchiveRecordDto dto);
    Task<Result<ArchiveRecordDto>> UpdateAsync(Guid id, UpdateArchiveRecordDto dto);
    Task<Result<ArchiveRecordDto>> AddFilesAsync(Guid id, IFormFileCollection files);
    Task<Result<bool>> RemoveFileAsync(Guid id, Guid fileId);
    Task<Result<ArchivePhysicalFileDownloadDto>> GetPhysicalFileStreamAsync(Guid fileId, Guid? recordId = null);
    Task<Result<ArchiveRecordFilesMetadataPageDto>> GetFilesMetadataByRecordIdAsync(Guid recordId, int page = 1, int pageSize = 10, bool includeDeleted = false);
    Task<Result<ArchiveFileConsistencyDto>> CheckFileConsistencyAsync(Guid id);
    //Task<Result<ArchiveFileCleanupDto>> CleanupOrphanFilesAsync();
    Task<Result<bool>> DeleteAsync(Guid id);
}
