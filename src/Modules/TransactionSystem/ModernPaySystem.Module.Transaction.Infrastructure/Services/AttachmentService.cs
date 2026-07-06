using System.IO.Compression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernPaySystem.Module.Transaction.Application;
using ModernPaySystem.Module.Transaction.Application.Interfaces;
using ModernPaySystem.Module.Transaction.Domain;
using ModernPaySystem.Module.Transaction.Domain.Entities;
using ModernPaySystem.SharedKernel.Domain.Commons;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Transaction.Infrastructure.Services;

public class AttachmentService(
    ITransactionUnitOfWork unitOfWork,
    ILogger<AttachmentService> logger) : IAttachmentService
{
    public async Task<Result<AttachmentDto>> GetByIdAsync(Guid id)
    {
        var attachment = await unitOfWork.Attachments.GetByIdAsync(id);
        if (attachment.IsError)
            return attachment.Errors;

        if (attachment.Value == null)
            return TransactionErrors.AttachmentNotFound;

        return attachment.Value.ToDto();
    }

    public async Task<Result<IEnumerable<AttachmentDto>>> GetByFileTypeAsync(string fileType)
    {
        var attachments = await unitOfWork.Attachments.GetAllAsync();
        if (attachments.IsError)
            return attachments.Errors;

        var filtered = attachments.Value!
            .Where(a => a.Extension?.Equals(fileType, StringComparison.OrdinalIgnoreCase) == true)
            .Select(a => a.ToDto())
            .ToList();

        return filtered;
    }

    public async Task<Result<AttachmentDto>> GetByFileNameAsync(string fileName)
    {
        var attachments = await unitOfWork.Attachments.GetAllAsync();
        if (attachments.IsError)
            return attachments.Errors;

        var attachment = attachments.Value!
            .FirstOrDefault(a => a.FileName?.Equals(fileName, StringComparison.OrdinalIgnoreCase) == true);

        if (attachment == null)
            return TransactionErrors.AttachmentNotFound;

        return attachment.ToDto();
    }

    public async Task<Result<AttachmentDto>> CreateAsync(CreateAttachmentDto attachmentDto)
    {
        var attachment = new Attachment
        {
            FileName = attachmentDto.FileName,
            SafeName = attachmentDto.SafeName,
            Extension = attachmentDto.Extension,
            Path = attachmentDto.Path,
            Size = attachmentDto.Size
        };

        var result = await unitOfWork.Attachments.AddAsync(attachment);
        if (result.IsError)
            return result.Errors;

        return attachment.ToDto();
    }

    public async Task<Result<AttachmentDto>> UpdateAsync(Guid id, UpdateAttachmentDto attachmentDto)
    {
        var existing = await unitOfWork.Attachments.GetByIdAsync(id);
        if (existing.IsError)
            return existing.Errors;

        if (existing.Value == null)
            return TransactionErrors.AttachmentNotFound;

        existing.Value.FileName = attachmentDto.FileName;
        existing.Value.SafeName = attachmentDto.SafeName;
        existing.Value.Extension = attachmentDto.Extension;
        existing.Value.Path = attachmentDto.Path;

        var result = await unitOfWork.Attachments.UpdateAsync(existing.Value);
        if (result.IsError)
            return result.Errors;

        return existing.Value.ToDto();
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        var result = await unitOfWork.Attachments.RemoveAsync(x => x.Id == id);
        if (result.IsError)
            return result.Errors;

        return true;
    }

    public async Task<Result<byte[]>> DownloadFileFromRequestAsync(Guid requestId, Guid attachmentId)
    {
        var request = await unitOfWork.Requests.GetByIdAsync(requestId);
        if (request.IsError)
            return TransactionErrors.RequestNotFound;

        var requestAttachment = await unitOfWork.RequestAttachments.GetAsync(
            AttachmentExpressions.RequestAttachmentByRequestIdAndAttachmentId(requestId, attachmentId),
            q => q.Include(ra => ra.Attachment));

        if (requestAttachment.IsError || requestAttachment.Value?.Attachment == null)
            return TransactionErrors.AttachmentNotFound;

        return await ReadFileBytesAsync(requestAttachment.Value.Attachment.Path);
    }

    public async Task<Result<byte[]>> DownloadFileFromResponseAsync(Guid responseId, Guid attachmentId)
    {
        var response = await unitOfWork.Responses.GetByIdAsync(responseId);
        if (response.IsError)
            return TransactionErrors.ResponseNotFound;

        var responseAttachment = await unitOfWork.ResponseAttachments.GetAsync(
            AttachmentExpressions.ResponseAttachmentByResponseIdAndAttachmentId(responseId, attachmentId),
            q => q.Include(ra => ra.Attachment));

        if (responseAttachment.IsError || responseAttachment.Value?.Attachment == null)
            return TransactionErrors.AttachmentNotFound;

        return await ReadFileBytesAsync(responseAttachment.Value.Attachment.Path);
    }

    public async Task<Result<byte[]>> DownloadFileFromTransactionAsync(Guid transactionId, Guid attachmentId)
    {
        var transaction = await unitOfWork.RequestTransactions.GetByIdAsync(transactionId);
        if (transaction.IsError)
            return TransactionErrors.RequestTransactionNotFound;

        var transactionAttachment = await unitOfWork.RequestTransactionAttachments.GetAsync(
            AttachmentExpressions.RequestTransactionAttachmentByTransactionIdAndAttachmentId(transactionId, attachmentId),
            q => q.Include(rta => rta.Attachment));

        if (transactionAttachment.IsError || transactionAttachment.Value?.Attachment == null)
            return TransactionErrors.AttachmentNotFound;

        return await ReadFileBytesAsync(transactionAttachment.Value.Attachment.Path);
    }

    public async Task<Result<byte[]>> DownloadFilesFromRequestAsync(Guid requestId)
    {
        var request = await unitOfWork.Requests.GetByIdAsync(requestId);
        if (request.IsError)
            return TransactionErrors.RequestNotFound;

        var attachments = await unitOfWork.RequestAttachments.GetAllAsync(
            filter: AttachmentExpressions.RequestAttachmentByRequestId(requestId),
            transform: q => q.Include(ra => ra.Attachment));

        if (attachments.IsError)
            return attachments.Errors;

        return await ZipAttachmentsAsync(attachments.Value!.Select(ra => ra.Attachment).Where(a => a != null)!);
    }

    public async Task<Result<byte[]>> DownloadFilesFromResponseAsync(Guid responseId)
    {
        var response = await unitOfWork.Responses.GetByIdAsync(responseId);
        if (response.IsError)
            return TransactionErrors.ResponseNotFound;

        var attachments = await unitOfWork.ResponseAttachments.GetAllAsync(
            filter: AttachmentExpressions.ResponseAttachmentByResponseId(responseId),
            transform: q => q.Include(ra => ra.Attachment));

        if (attachments.IsError)
            return attachments.Errors;

        return await ZipAttachmentsAsync(attachments.Value!.Select(ra => ra.Attachment).Where(a => a != null)!);
    }

    public async Task<Result<byte[]>> DownloadFilesFromTransactionAsync(Guid transactionId)
    {
        var transaction = await unitOfWork.RequestTransactions.GetByIdAsync(transactionId);
        if (transaction.IsError)
            return TransactionErrors.RequestTransactionNotFound;

        var attachments = await unitOfWork.RequestTransactionAttachments.GetAllAsync(
            filter: AttachmentExpressions.RequestTransactionAttachmentByTransactionId(transactionId),
            transform: q => q.Include(rta => rta.Attachment));

        if (attachments.IsError)
            return attachments.Errors;

        return await ZipAttachmentsAsync(attachments.Value!.Select(rta => rta.Attachment).Where(a => a != null)!);
    }

    public async Task<Result<Success>> RemoveFileFromRequestAsync(Guid requestId, Guid attachmentId)
    {
        var request = await unitOfWork.Requests.GetByIdAsync(requestId);
        if (request.IsError)
            return TransactionErrors.RequestNotFound;

        var requestAttachment = await unitOfWork.RequestAttachments.GetAsync(
            AttachmentExpressions.RequestAttachmentByRequestIdAndAttachmentId(requestId, attachmentId),
            q => q.Include(ra => ra.Attachment));

        if (requestAttachment.IsError || requestAttachment.Value == null)
            return TransactionErrors.AttachmentNotFound;

        var attachment = requestAttachment.Value.Attachment;
        var removeResult = await unitOfWork.RequestAttachments.RemoveAsync(x => x.Id == requestAttachment.Value.Id);
        if (removeResult.IsError)
            return removeResult.Errors;

        return await TryCleanupAttachmentAsync(attachment, attachmentId);
    }

    public async Task<Result<Success>> RemoveFileFromResponseAsync(Guid responseId, Guid attachmentId)
    {
        var response = await unitOfWork.Responses.GetByIdAsync(responseId);
        if (response.IsError)
            return TransactionErrors.ResponseNotFound;

        var responseAttachment = await unitOfWork.ResponseAttachments.GetAsync(
            AttachmentExpressions.ResponseAttachmentByResponseIdAndAttachmentId(responseId, attachmentId),
            q => q.Include(ra => ra.Attachment));

        if (responseAttachment.IsError || responseAttachment.Value == null)
            return TransactionErrors.AttachmentNotFound;

        var attachment = responseAttachment.Value.Attachment;
        var removeResult = await unitOfWork.ResponseAttachments.RemoveAsync(x => x.Id == responseAttachment.Value.Id);
        if (removeResult.IsError)
            return removeResult.Errors;

        return await TryCleanupAttachmentAsync(attachment, attachmentId);
    }

    public async Task<Result<IEnumerable<AttachmentDto>>> GetAttachmentsForRequestAsync(Guid requestId)
    {
        var request = await unitOfWork.Requests.GetByIdAsync(requestId);
        if (request.IsError)
            return TransactionErrors.RequestNotFound;

        var attachments = await unitOfWork.RequestAttachments.GetAllAsync(
            filter: AttachmentExpressions.RequestAttachmentByRequestId(requestId),
            transform: q => q.Include(ra => ra.Attachment));

        if (attachments.IsError)
            return attachments.Errors;

        return attachments.Value!
            .Where(ra => ra.Attachment != null)
            .Select(ra => ra.Attachment!.ToDto())
            .ToList();
    }

    public async Task<Result<IEnumerable<AttachmentDto>>> GetAttachmentsForResponseAsync(Guid responseId)
    {
        var response = await unitOfWork.Responses.GetByIdAsync(responseId);
        if (response.IsError)
            return TransactionErrors.ResponseNotFound;

        var attachments = await unitOfWork.ResponseAttachments.GetAllAsync(
            filter: AttachmentExpressions.ResponseAttachmentByResponseId(responseId),
            transform: q => q.Include(ra => ra.Attachment));

        if (attachments.IsError)
            return attachments.Errors;

        return attachments.Value!
            .Where(ra => ra.Attachment != null)
            .Select(ra => ra.Attachment!.ToDto())
            .ToList();
    }

    public async Task<Result<IEnumerable<AttachmentDto>>> GetAttachmentsForTransactionAsync(Guid transactionId)
    {
        var transaction = await unitOfWork.RequestTransactions.GetByIdAsync(transactionId);
        if (transaction.IsError)
            return TransactionErrors.RequestTransactionNotFound;

        var attachments = await unitOfWork.RequestTransactionAttachments.GetAllAsync(
            filter: AttachmentExpressions.RequestTransactionAttachmentByTransactionId(transactionId),
            transform: q => q.Include(rta => rta.Attachment));

        if (attachments.IsError)
            return attachments.Errors;

        return attachments.Value!
            .Where(rta => rta.Attachment != null)
            .Select(rta => rta.Attachment!.ToDto())
            .ToList();
    }

    private async Task<Result<byte[]>> ReadFileBytesAsync(string path)
    {
        try
        {
            if (!File.Exists(path))
                return TransactionErrors.AttachmentNotFound;

            var bytes = await File.ReadAllBytesAsync(path);
            return bytes;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to read file at {Path}", path);
            return Error.Failure("1000", $"Failed to read file: {ex.Message}");
        }
    }

    private async Task<Result<byte[]>> ZipAttachmentsAsync(IEnumerable<Attachment> attachments)
    {
        using var zipStream = new MemoryStream();
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
        {
            foreach (var attachment in attachments)
            {
                try
                {
                    if (!File.Exists(attachment.Path))
                        continue;

                    var fileBytes = await File.ReadAllBytesAsync(attachment.Path);
                    var entry = archive.CreateEntry(attachment.FileName ?? attachment.SafeName);
                    using var entryStream = entry.Open();
                    await entryStream.WriteAsync(fileBytes);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to include file {Path} in zip", attachment.Path);
                }
            }
        }

        return zipStream.ToArray();
    }

    private async Task<Result<Success>> TryCleanupAttachmentAsync(Attachment? attachment, Guid attachmentId)
    {
        // Check if attachment is still referenced
        var requestAttachments = await unitOfWork.RequestAttachments.GetAllAsync(
            AttachmentExpressions.RequestAttachmentByAttachmentId(attachmentId));
        if (requestAttachments.IsError)
            return requestAttachments.Errors;

        var responseAttachments = await unitOfWork.ResponseAttachments.GetAllAsync(
            AttachmentExpressions.ResponseAttachmentByAttachmentId(attachmentId));
        if (responseAttachments.IsError)
            return responseAttachments.Errors;

        var transactionAttachments = await unitOfWork.RequestTransactionAttachments.GetAllAsync(
            AttachmentExpressions.RequestTransactionAttachmentByAttachmentId(attachmentId));
        if (transactionAttachments.IsError)
            return transactionAttachments.Errors;

        var isUsedElsewhere = requestAttachments.Value!.Any()
            || responseAttachments.Value!.Any()
            || transactionAttachments.Value!.Any();

        if (!isUsedElsewhere && attachment != null)
        {
            try
            {
                if (File.Exists(attachment.Path))
                    File.Delete(attachment.Path);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to delete physical file at {Path}", attachment.Path);
            }

            var deleteResult = await unitOfWork.Attachments.RemoveAsync(x => x.Id == attachmentId);
            if (deleteResult.IsError)
                return deleteResult.Errors;
        }

        return Result.Success;
    }
}
