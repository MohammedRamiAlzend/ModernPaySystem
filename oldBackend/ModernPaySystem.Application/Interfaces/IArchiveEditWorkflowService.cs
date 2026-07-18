using Microsoft.AspNetCore.Http;
using ModernPaySystem.Domain.Commons;
using ModernPaySystem.Domain.Entities.Archiving;

namespace ModernPaySystem.Application.Interfaces;

public interface IArchiveEditWorkflowService
{
    Task<Result<EditArchiveRequestDto>> SubmitRequestAsync(CreateEditArchiveRequestDto dto);
    Task<Result<EditArchiveRequestDto>> GetByIdAsync(Guid requestId);
    Task<Result<PagedList<EditArchiveRequestDto>>> GetPendingForDepartmentAsync(Guid departmentId, int page = 1, int pageSize = 20);
    Task<Result<PagedList<EditArchiveRequestDto>>> GetMyRequestsAsync(Guid requesterId, int page = 1, int pageSize = 20);
    Task<Result<EditArchiveRequestDto>> ApproveAsync(Guid requestId, string? notes = null);
    Task<Result<EditArchiveRequestDto>> RejectAsync(Guid requestId, string reason);
}
