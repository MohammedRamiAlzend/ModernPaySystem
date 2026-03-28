using Microsoft.AspNetCore.Http;

namespace ModernPaySystem.Application.Interfaces;

public interface IResponseService
{

    Task<Result<PagedList<ResponseDto>>> GetPagedAsync(int page, int pageSize);

    Task<Result<ResponseDto>> GetByIdAsync(Guid id);

    Task<Result<IEnumerable<ResponseDto>>> GetByRequestIdAsync(Guid requestId);

    Task<Result<IEnumerable<ResponseDto>>> GetByResponderIdAsync(Guid responderId);

    Task<Result<IEnumerable<ResponseDto>>> GetResponsesByRequesterIdAsync(Guid requesterId, int page = 1, int pageSize = 10);

    Task<Result<ResponseDto>> CreateAsync(CreateResponseDto response);

    Task<Result<ResponseDto>> UpdateAsync(Guid id, UpdateResponseDto response);

    Task<Result<bool>> DeleteAsync(Guid id);

    Task<Result<ResponseDto>> AddFilesToResponseAsync(Guid responseId, List<IFormFile> files);
}
