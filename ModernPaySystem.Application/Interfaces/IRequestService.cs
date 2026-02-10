global using ModernPaySystem.Domain.Commons;
global using ModernPaySystem.Domain.Entities.TransactionSystemEntities;
global using System.Collections.Generic;
global using System;
using Microsoft.AspNetCore.Http;

namespace ModernPaySystem.Application.Interfaces;

/// <summary>
/// Interface for Request service CRUD operations.
/// </summary>
public interface IRequestService
{
    /// <summary>
    /// Get all requests.
    /// </summary>
    Task<Result<IEnumerable<RequestDto>>> GetAllAsync();

    /// <summary>
    /// Get paged requests.
    /// </summary>
    Task<Result<PagedList<RequestDto>>> GetPagedAsync(int page, int pageSize);

    /// <summary>
    /// Get request by id.
    /// </summary>
    Task<Result<RequestDto>> GetByIdAsync(Guid id);

    /// <summary>
    /// Get requests by requester id.
    /// </summary>
    Task<Result<IEnumerable<RequestDto>>> GetByRequesterIdAsync(Guid requesterId);

    /// <summary>
    /// Get requests by approver id.
    /// </summary>
    Task<Result<IEnumerable<RequestDto>>> GetByApproverIdAsync(Guid approverId);

    /// <summary>
    /// Get requests by template id.
    /// </summary>
    Task<Result<IEnumerable<RequestDto>>> GetByTemplateIdAsync(Guid templateId);

    /// <summary>
    /// Create new request.
    /// </summary>
    Task<Result<RequestDto>> CreateAsync(CreateRequestDto request, List<IFormFile> files);

    /// <summary>
    /// Update request.
    /// </summary>
    Task<Result<RequestDto>> UpdateAsync(Guid id, UpdateRequestDto request);

    /// <summary>
    /// Delete request.
    /// </summary>
    Task<Result<bool>> DeleteAsync(Guid id);

    /// <summary>
    /// Add files to a request.
    /// </summary>
    Task<Result<RequestDto>> AddFilesToRequestAsync(Guid requestId, List<IFormFile> files);

    /// <summary>
    /// Get requests received by the current user (where the current user is the approver).
    /// </summary>
    Task<Result<IEnumerable<RequestDto>>> GetReceivedRequestsAsync();

    /// <summary>
    /// Get paged requests received by the current user (where the current user is the approver).
    /// </summary>
    Task<Result<PagedList<RequestDto>>> GetReceivedRequestsPagedAsync(int page, int pageSize);
}
