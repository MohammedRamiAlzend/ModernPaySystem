using ModernPaySystem.Domain.Commons;
using ModernPaySystem.Domain.Entities.TransactionSystemEntities;

namespace ModernPaySystem.Application.Interfaces;

/// <summary>
/// Interface for Response service CRUD operations.
/// </summary>
public interface IResponseService
{
    /// <summary>
    /// Get all responses.
    /// </summary>
    Task<Result<IEnumerable<Response>>> GetAllAsync();

    /// <summary>
    /// Get paged responses.
    /// </summary>
    Task<Result<PagedList<Response>>> GetPagedAsync(int page, int pageSize);

    /// <summary>
    /// Get response by id.
    /// </summary>
    Task<Result<Response>> GetByIdAsync(Guid id);

    /// <summary>
    /// Get responses by request id.
    /// </summary>
    Task<Result<IEnumerable<Response>>> GetByRequestIdAsync(Guid requestId);

    /// <summary>
    /// Get responses by responder id.
    /// </summary>
    Task<Result<IEnumerable<Response>>> GetByResponderIdAsync(Guid responderId);

    /// <summary>
    /// Create new response.
    /// </summary>
    Task<Result<Response>> CreateAsync(Response response);

    /// <summary>
    /// Update response.
    /// </summary>
    Task<Result<Response>> UpdateAsync(Guid id, Response response);

    /// <summary>
    /// Delete response.
    /// </summary>
    Task<Result<bool>> DeleteAsync(Guid id);
}
