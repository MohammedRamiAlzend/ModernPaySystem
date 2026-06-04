using ModernPaySystem.Domain.Entities.Archiving;

namespace ModernPaySystem.Application.Interfaces;

public interface IArchiveDeletionWorkflowService
{
    Task<Result<DeleteArchiveRequestDto>> SubmitRequestAsync(CreateDeleteArchiveRequestDto dto);
    Task<Result<DeleteArchiveRequestDto>> GetByIdAsync(Guid requestId);
    Task<Result<PagedList<DeleteArchiveRequestDto>>> GetPendingForDepartmentAsync(Guid departmentId, int page = 1, int pageSize = 20);
    Task<Result<DeleteArchiveRequestDto>> ApproveAsync(Guid requestId, string? notes = null);
    Task<Result<DeleteArchiveRequestDto>> RejectAsync(Guid requestId, string reason);
    Task<Result<bool>> DeleteFolderAsync(Guid folderId, Guid? requestId = null);
    Task<Result<bool>> DeleteArchiveRecordAsync(Guid recordId, Guid? requestId = null);
}
