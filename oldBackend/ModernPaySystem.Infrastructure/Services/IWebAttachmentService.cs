using Microsoft.AspNetCore.Http;

namespace ModernPaySystem.Infrastructure.Services;

/// <summary>
/// Interface for web-specific attachment operations that handle file uploads/downloads.
/// </summary>
public interface IWebAttachmentService
{
    /// <summary>
    /// Uploads a file and associates it with a request.
    /// </summary>
    Task<Result<Attachment>> UploadFileToRequestAsync(IFormFile file, Guid requestId, string? subDirectory = null);

    /// <summary>
    /// Uploads a file and associates it with a response.
    /// </summary>
    Task<Result<Attachment>> UploadFileToResponseAsync(IFormFile file, Guid responseId, string? subDirectory = null);

    /// <summary>
    /// Uploads a file and associates it with a request transaction.
    /// </summary>
    Task<Result<Attachment>> UploadFileToRequestTransactionAsync(IFormFile file, Guid requestTransactionId, string? subDirectory = null);
}
