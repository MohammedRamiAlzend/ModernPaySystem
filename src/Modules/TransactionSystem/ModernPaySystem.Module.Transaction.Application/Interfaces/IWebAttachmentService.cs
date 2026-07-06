using Microsoft.AspNetCore.Http;
using ModernPaySystem.SharedKernel.Domain.Commons;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Transaction.Application.Interfaces;

public interface IWebAttachmentService
{
    Task<Result<Attachment>> UploadFileToRequestAsync(IFormFile file, Guid requestId, string? subDirectory = null);

    Task<Result<Attachment>> UploadFileToResponseAsync(IFormFile file, Guid responseId, string? subDirectory = null);

    Task<Result<Attachment>> UploadFileToRequestTransactionAsync(IFormFile file, Guid requestTransactionId, string? subDirectory = null);
}
