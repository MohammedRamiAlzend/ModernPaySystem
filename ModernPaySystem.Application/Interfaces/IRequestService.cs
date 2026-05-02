global using ModernPaySystem.Domain.Entities.TransactionSystemEntities;
using Microsoft.AspNetCore.Http;

namespace ModernPaySystem.Application.Interfaces;

public interface IRequestService
{
    //Task<Result<IEnumerable<RequestDto>>> GetAllAsync();

    //Task<Result<IEnumerable<RequestDto>>> GetAllAsync(bool hasResponse);

    Task<Result<PagedList<RequestDto>>> GetPagedAsync(int page, int pageSize);

    Task<Result<RequestDto>> GetByIdAsync(Guid id);

    Task<Result<PagedList<RequestDto>>> GetByRequesterIdAsync(Guid requesterId, int page, int pageSize);

    Task<Result<PagedList<RequestDto>>> GetByApproverIdAsync(Guid approverId, int page, int pageSize);

    Task<Result<PagedList<RequestDto>>> GetByTemplateIdAsync(Guid templateId, int page, int pageSize);

    Task<Result<RequestDto>> CreateAsync(CreateRequestDto request, List<IFormFile> files);

    Task<Result<bool>> DeleteAsync(Guid id);

    Task<Result<RequestDto>> AddFilesToRequestAsync(Guid requestId, List<IFormFile> files);

    Task<Result<PagedList<RequestDto>>> GetAllRequestNeedActionPagedAsync(int page, int pageSize, bool hasResponse);
}
