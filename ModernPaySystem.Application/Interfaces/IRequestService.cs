using ModernPaySystem.Domain.Commons;
using ModernPaySystem.Domain.Entities.TransactionSystemEntities;

namespace ModernPaySystem.Application.Interfaces;

/// <summary>
/// Interface for Request service CRUD operations.
/// </summary>
public interface IRequestService
{
    /// <summary>
    /// Get all requests.
    /// </summary>
    Task<Result<IEnumerable<Request>>> GetAllAsync();

    /// <summary>
    /// Get paged requests.
    /// </summary>
    Task<Result<PagedList<Request>>> GetPagedAsync(int page, int pageSize);

    /// <summary>
    /// Get request by id.
    /// </summary>
    Task<Result<Request>> GetByIdAsync(Guid id);

    /// <summary>
    /// Get requests by requester id.
    /// </summary>
    Task<Result<IEnumerable<Request>>> GetByRequesterIdAsync(Guid requesterId);

    /// <summary>
    /// Get requests by approver id.
    /// </summary>
    Task<Result<IEnumerable<Request>>> GetByApproverIdAsync(Guid approverId);

    /// <summary>
    /// Get requests by template id.
    /// </summary>
    Task<Result<IEnumerable<Request>>> GetByTemplateIdAsync(Guid templateId);

    /// <summary>
    /// Create new request.
    /// </summary>
    Task<Result<Request>> CreateAsync(Request request);

    /// <summary>
    /// Update request.
    /// </summary>
    Task<Result<Request>> UpdateAsync(Guid id, Request request);

    /// <summary>
    /// Delete request.
    /// </summary>
    Task<Result<bool>> DeleteAsync(Guid id);
}
