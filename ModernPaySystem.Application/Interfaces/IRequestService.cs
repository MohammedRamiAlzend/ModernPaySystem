global using ModernPaySystem.Domain.Entities.TransactionSystemEntities;
using Microsoft.AspNetCore.Http;
using ModernPaySystem.Domain.DTOs;

namespace ModernPaySystem.Application.Interfaces;

public interface IRequestService
{
    //Task<Result<IEnumerable<RequestDto>>> GetAllAsync();

    //Task<Result<IEnumerable<RequestDto>>> GetAllAsync(bool hasResponse);

    Task<Result<PagedList<RequestDto>>> GetPagedAsync(RequestPagedFilterDto filterDto);

    Task<Result<RequestDto>> GetByIdAsync(Guid id);

    Task<Result<PagedList<RequestDto>>> GetByRequesterIdAsync(Guid requesterId, RequestPagedFilterDto filterDto);

    Task<Result<PagedList<RequestDto>>> GetByApproverIdAsync(Guid approverId, int page, int pageSize);

    Task<Result<PagedList<RequestDto>>> GetByTemplateIdAsync(Guid templateId, int page, int pageSize);

    Task<Result<RequestDto>> CreateAsync(CreateRequestDto request, List<IFormFile> files);

    Task<Result<bool>> DeleteAsync(Guid id);

    Task<Result<RequestDto>> AddFilesToRequestAsync(Guid requestId, List<IFormFile> files);

    Task<Result<PagedList<RequestDto>>> GetAllRequestNeedActionPagedAsync(RequestPagedFilterDto? filterDto, bool hasResponse);

    Task<Result<PagedList<RequestDto>>> GetPendingByCurrentRequesterPagedAsync(int page, int pageSize);
}
