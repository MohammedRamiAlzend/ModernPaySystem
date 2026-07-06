using Microsoft.AspNetCore.Http;
using ModernPaySystem.Module.Transaction.Domain.DTOs;
using ModernPaySystem.Module.Transaction.Domain.Entities;
using ModernPaySystem.SharedKernel.Domain.Commons;

namespace ModernPaySystem.Module.Transaction.Application.Interfaces;

public interface IResponseService
{
    Task<Result<PagedList<ResponseDto>>> GetPagedAsync(int page, int pageSize);

    Task<Result<ResponseDto>> GetByIdAsync(Guid id);

    Task<Result<PagedList<ResponseDto>>> GetByRequestIdAsync(Guid requestId, RequestPagedFilterDto filterDto);

    Task<Result<PagedList<ResponseDto>>> GetByResponderIdAsync(Guid responderId, RequestPagedFilterDto filterDto);

    Task<Result<PagedList<ResponseDto>>> GetResponsesByRequesterIdAsync(Guid requesterId, RequestPagedFilterDto filterDto);

    Task<Result<Success>> CreateAsync(CreateResponseDto response);

    Task<Result<ResponseDto>> UpdateAsync(Guid id, UpdateResponseDto response);

    Task<Result<bool>> DeleteAsync(Guid id);

    Task<Result<ResponseDto>> AddFilesToResponseAsync(Guid responseId, List<IFormFile> files);
}
