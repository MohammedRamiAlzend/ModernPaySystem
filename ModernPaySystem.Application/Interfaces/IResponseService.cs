using Microsoft.AspNetCore.Http;
using ModernPaySystem.Domain.DTOs;

namespace ModernPaySystem.Application.Interfaces;

public interface IResponseService
{
    Task<Result<PagedList<ResponseDto>>> GetPagedAsync(int page, int pageSize);

    Task<Result<ResponseDto>> GetByIdAsync(Guid id);

    Task<Result<PagedList<ResponseDto>>> GetByRequestIdAsync(Guid requestId, RequestPagedFilterDto filterDto);

    Task<Result<PagedList<ResponseDto>>> GetByResponderIdAsync(Guid responderId, int page, int pageSize);

    Task<Result<PagedList<ResponseDto>>> GetResponsesByRequesterIdAsync(Guid requesterId, int page, int pageSize);

    Task<Result<Success>> CreateAsync(CreateResponseDto response);

    Task<Result<ResponseDto>> UpdateAsync(Guid id, UpdateResponseDto response);

    Task<Result<bool>> DeleteAsync(Guid id);

    Task<Result<ResponseDto>> AddFilesToResponseAsync(Guid responseId, List<IFormFile> files);
}
