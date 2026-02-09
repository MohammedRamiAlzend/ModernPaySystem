using ModernPaySystem.Domain.Commons;
using ModernPaySystem.Domain.Entities.TransactionSystemEntities;
using Microsoft.AspNetCore.Http;

namespace ModernPaySystem.Application.Interfaces;

/// <summary>
/// Interface for Response service CRUD operations.
/// </summary>
public interface IResponseService
{
    /// <summary>
    /// Get all responses.
    /// </summary>
    Task<Result<IEnumerable<ResponseDto>>> GetAllAsync();

    /// <summary>
    /// Get paged responses.
    /// </summary>
    Task<Result<PagedList<ResponseDto>>> GetPagedAsync(int page, int pageSize);

    /// <summary>
    /// Get response by id.
    /// </summary>
    Task<Result<ResponseDto>> GetByIdAsync(Guid id);

    /// <summary>
    /// Get responses by request id.
    /// </summary>
    Task<Result<IEnumerable<ResponseDto>>> GetByRequestIdAsync(Guid requestId);

    /// <summary>
    /// Get responses by responder id.
    /// </summary>
    Task<Result<IEnumerable<ResponseDto>>> GetByResponderIdAsync(Guid responderId);

    /// <summary>
    /// Create new response.
    /// </summary>
    Task<Result<ResponseDto>> CreateAsync(CreateResponseDto response);

    /// <summary>
    /// Update response.
    /// </summary>
    Task<Result<ResponseDto>> UpdateAsync(Guid id, UpdateResponseDto response);

    /// <summary>
    /// Delete response.
    /// </summary>
    Task<Result<bool>> DeleteAsync(Guid id);

    /// <summary>
    /// Add files to a response.
    /// </summary>
    Task<Result<ResponseDto>> AddFilesToResponseAsync(Guid responseId, List<IFormFile> files);
}
