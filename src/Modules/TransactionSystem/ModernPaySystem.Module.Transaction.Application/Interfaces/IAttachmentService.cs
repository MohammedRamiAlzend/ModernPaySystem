using ModernPaySystem.SharedKernel.Domain.Commons;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Transaction.Application.Interfaces;

public interface IAttachmentService
{
    Task<Result<AttachmentDto>> GetByIdAsync(Guid id);

    Task<Result<IEnumerable<AttachmentDto>>> GetByFileTypeAsync(string fileType);

    Task<Result<AttachmentDto>> GetByFileNameAsync(string fileName);

    Task<Result<AttachmentDto>> CreateAsync(CreateAttachmentDto attachment);

    Task<Result<AttachmentDto>> UpdateAsync(Guid id, UpdateAttachmentDto attachment);

    Task<Result<bool>> DeleteAsync(Guid id);

    Task<Result<byte[]>> DownloadFileFromRequestAsync(Guid requestId, Guid attachmentId);

    Task<Result<byte[]>> DownloadFileFromResponseAsync(Guid responseId, Guid attachmentId);

    Task<Result<byte[]>> DownloadFilesFromRequestAsync(Guid requestId);

    Task<Result<byte[]>> DownloadFilesFromResponseAsync(Guid responseId);

    Task<Result<byte[]>> DownloadFileFromTransactionAsync(Guid transactionId, Guid attachmentId);

    Task<Result<byte[]>> DownloadFilesFromTransactionAsync(Guid transactionId);

    Task<Result<Success>> RemoveFileFromRequestAsync(Guid requestId, Guid attachmentId);

    Task<Result<Success>> RemoveFileFromResponseAsync(Guid responseId, Guid attachmentId);

    Task<Result<IEnumerable<AttachmentDto>>> GetAttachmentsForRequestAsync(Guid requestId);

    Task<Result<IEnumerable<AttachmentDto>>> GetAttachmentsForResponseAsync(Guid responseId);

    Task<Result<IEnumerable<AttachmentDto>>> GetAttachmentsForTransactionAsync(Guid transactionId);
}
