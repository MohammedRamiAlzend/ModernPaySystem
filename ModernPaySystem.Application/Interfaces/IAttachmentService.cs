global using System;
global using System.Collections.Generic;

namespace ModernPaySystem.Application.Interfaces;

/// <summary>
/// Interface for Attachment service CRUD operations.
/// </summary>
public interface IAttachmentService
{

    /// <summary>
    /// Get attachment by id.
    /// </summary>
    Task<Result<AttachmentDto>> GetByIdAsync(Guid id);

    /// <summary>
    /// Get attachments by file type.
    /// </summary>
    Task<Result<IEnumerable<AttachmentDto>>> GetByFileTypeAsync(string fileType);

    /// <summary>
    /// Get attachment by file name.
    /// </summary>
    Task<Result<AttachmentDto>> GetByFileNameAsync(string fileName);

    /// <summary>
    /// Create new attachment.
    /// </summary>
    Task<Result<AttachmentDto>> CreateAsync(CreateAttachmentDto attachment);

    /// <summary>
    /// Update attachment.
    /// </summary>
    Task<Result<AttachmentDto>> UpdateAsync(Guid id, UpdateAttachmentDto attachment);

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
    /// Downloads a files associated with a request.
    /// </summary>
    Task<Result<byte[]>> DownloadFilesFromRequestAsync(Guid requestId);

    /// <summary>
    /// Downloads files associated with a response.
    /// </summary>
    Task<Result<byte[]>> DownloadFilesFromResponseAsync(Guid responseId);

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
    Task<Result<IEnumerable<AttachmentDto>>> GetAttachmentsForRequestAsync(Guid requestId);

    /// <summary>
    /// Gets all attachments for a response.
    /// </summary>
    Task<Result<IEnumerable<AttachmentDto>>> GetAttachmentsForResponseAsync(Guid responseId);
}
