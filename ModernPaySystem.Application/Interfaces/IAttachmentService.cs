namespace ModernPaySystem.Application.Interfaces;

/// <summary>
/// Interface for Attachment service CRUD operations.
/// </summary>
public interface IAttachmentService
{
    /// <summary>
    /// Get all attachments.
    /// </summary>
    Task<Result<IEnumerable<Attachment>>> GetAllAsync();

    /// <summary>
    /// Get paged attachments.
    /// </summary>
    Task<Result<PagedList<Attachment>>> GetPagedAsync(int page, int pageSize);

    /// <summary>
    /// Get attachment by id.
    /// </summary>
    Task<Result<Attachment>> GetByIdAsync(Guid id);

    /// <summary>
    /// Get attachments by file type.
    /// </summary>
    Task<Result<IEnumerable<Attachment>>> GetByFileTypeAsync(string fileType);

    /// <summary>
    /// Get attachment by file name.
    /// </summary>
    Task<Result<Attachment>> GetByFileNameAsync(string fileName);

    /// <summary>
    /// Create new attachment.
    /// </summary>
    Task<Result<Attachment>> CreateAsync(Attachment attachment);

    /// <summary>
    /// Update attachment.
    /// </summary>
    Task<Result<Attachment>> UpdateAsync(Guid id, Attachment attachment);

    /// <summary>
    /// Delete attachment.
    /// </summary>
    Task<Result<bool>> DeleteAsync(Guid id);

    /// <summary>
    /// Downloads a file associated with a request.
    /// </summary>
    Task<Result<byte[]>> DownloadFileFromRequestAsync(Guid requestId, Guid attachmentId);

    /// <summary>
    /// Downloads a file associated with a response.
    /// </summary>
    Task<Result<byte[]>> DownloadFileFromResponseAsync(Guid responseId, Guid attachmentId);

    /// <summary>
    /// Removes a file attachment from a request.
    /// </summary>
    Task<Result<Success>> RemoveFileFromRequestAsync(Guid requestId, Guid attachmentId);

    /// <summary>
    /// Removes a file attachment from a response.
    /// </summary>
    Task<Result<Success>> RemoveFileFromResponseAsync(Guid responseId, Guid attachmentId);

    /// <summary>
    /// Gets all attachments for a request.
    /// </summary>
    Task<Result<IEnumerable<Attachment>>> GetAttachmentsForRequestAsync(Guid requestId);

    /// <summary>
    /// Gets all attachments for a response.
    /// </summary>
    Task<Result<IEnumerable<Attachment>>> GetAttachmentsForResponseAsync(Guid responseId);
}
