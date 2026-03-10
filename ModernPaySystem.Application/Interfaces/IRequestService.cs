global using ModernPaySystem.Domain.Entities.TransactionSystemEntities;
using Microsoft.AspNetCore.Http;

namespace ModernPaySystem.Application.Interfaces;

public interface IRequestService
{
    Task<Result<IEnumerable<RequestDto>>> GetAllAsync();

    Task<Result<IEnumerable<RequestDto>>> GetAllAsync(bool hasResponse);

    Task<Result<PagedList<RequestDto>>> GetPagedAsync(int page, int pageSize);

    Task<Result<RequestDto>> GetByIdAsync(Guid id);

    Task<Result<IEnumerable<RequestDto>>> GetByRequesterIdAsync(Guid requesterId);

    Task<Result<IEnumerable<RequestDto>>> GetByApproverIdAsync(Guid approverId);

    Task<Result<IEnumerable<RequestDto>>> GetByTemplateIdAsync(Guid templateId);

    Task<Result<RequestDto>> CreateAsync(CreateRequestDto request, List<IFormFile> files);

    Task<Result<bool>> DeleteAsync(Guid id);

    Task<Result<RequestDto>> AddFilesToRequestAsync(Guid requestId, List<IFormFile> files);

    Task<Result<IEnumerable<RequestDto>>> GetReceivedRequestsAsync();

    Task<Result<PagedList<RequestDto>>> GetReceivedRequestsPagedAsync(int page, int pageSize);

    Task<Result<PagedList<RequestDto>>> GetPagedAsync(int page, int pageSize, bool hasResponse);
}
