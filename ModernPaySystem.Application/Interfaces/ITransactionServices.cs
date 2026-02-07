using ModernPaySystem.Domain.Commons;
using ModernPaySystem.Domain.Entities.TransactionSystemEntities;
using ModernPaySystem.Domain.Entities.SharedEntities;

namespace ModernPaySystem.Application.Interfaces;

/// <summary>
/// Interface for Template service CRUD operations
/// </summary>
public interface ITemplateService
{
    /// <summary>
    /// Get all templates
    /// </summary>
    Task<Result<IEnumerable<Template>>> GetAllAsync();

    /// <summary>
    /// Get template by id
    /// </summary>
    Task<Result<Template>> GetByIdAsync(Guid id);

    /// <summary>
    /// Get template by name
    /// </summary>
    Task<Result<Template>> GetByNameAsync(string name);

    /// <summary>
    /// Create new template
    /// </summary>
    Task<Result<Template>> CreateAsync(Template template);

    /// <summary>
    /// Update template
    /// </summary>
    Task<Result<Template>> UpdateAsync(Guid id, Template template);

    /// <summary>
    /// Delete template
    /// </summary>
    Task<Result<bool>> DeleteAsync(Guid id);
}

/// <summary>
/// Interface for Request service CRUD operations
/// </summary>
public interface IRequestService
{
    /// <summary>
    /// Get all requests
    /// </summary>
    Task<Result<IEnumerable<Request>>> GetAllAsync();

    /// <summary>
    /// Get request by id
    /// </summary>
    Task<Result<Request>> GetByIdAsync(Guid id);

    /// <summary>
    /// Get requests by requester id
    /// </summary>
    Task<Result<IEnumerable<Request>>> GetByRequesterIdAsync(Guid requesterId);

    /// <summary>
    /// Get requests by approver id
    /// </summary>
    Task<Result<IEnumerable<Request>>> GetByApproverIdAsync(Guid approverId);

    /// <summary>
    /// Get requests by template id
    /// </summary>
    Task<Result<IEnumerable<Request>>> GetByTemplateIdAsync(Guid templateId);

    /// <summary>
    /// Create new request
    /// </summary>
    Task<Result<Request>> CreateAsync(Request request);

    /// <summary>
    /// Update request
    /// </summary>
    Task<Result<Request>> UpdateAsync(Guid id, Request request);

    /// <summary>
    /// Delete request
    /// </summary>
    Task<Result<bool>> DeleteAsync(Guid id);
}

/// <summary>
/// Interface for Response service CRUD operations
/// </summary>
public interface IResponseService
{
    /// <summary>
    /// Get all responses
    /// </summary>
    Task<Result<IEnumerable<Response>>> GetAllAsync();

    /// <summary>
    /// Get response by id
    /// </summary>
    Task<Result<Response>> GetByIdAsync(Guid id);

    /// <summary>
    /// Get responses by request id
    /// </summary>
    Task<Result<IEnumerable<Response>>> GetByRequestIdAsync(Guid requestId);

    /// <summary>
    /// Get responses by responder id
    /// </summary>
    Task<Result<IEnumerable<Response>>> GetByResponderIdAsync(Guid responderId);

    /// <summary>
    /// Create new response
    /// </summary>
    Task<Result<Response>> CreateAsync(Response response);

    /// <summary>
    /// Update response
    /// </summary>
    Task<Result<Response>> UpdateAsync(Guid id, Response response);

    /// <summary>
    /// Delete response
    /// </summary>
    Task<Result<bool>> DeleteAsync(Guid id);
}

/// <summary>
/// Interface for Attachment service CRUD operations
/// </summary>
public interface IAttachmentService
{
    /// <summary>
    /// Get all attachments
    /// </summary>
    Task<Result<IEnumerable<Attachment>>> GetAllAsync();

    /// <summary>
    /// Get attachment by id
    /// </summary>
    Task<Result<Attachment>> GetByIdAsync(Guid id);

    /// <summary>
    /// Get attachments by file type
    /// </summary>
    Task<Result<IEnumerable<Attachment>>> GetByFileTypeAsync(string fileType);

    /// <summary>
    /// Get attachment by file name
    /// </summary>
    Task<Result<Attachment>> GetByFileNameAsync(string fileName);

    /// <summary>
    /// Create new attachment
    /// </summary>
    Task<Result<Attachment>> CreateAsync(Attachment attachment);

    /// <summary>
    /// Update attachment
    /// </summary>
    Task<Result<Attachment>> UpdateAsync(Guid id, Attachment attachment);

    /// <summary>
    /// Delete attachment
    /// </summary>
    Task<Result<bool>> DeleteAsync(Guid id);
}
