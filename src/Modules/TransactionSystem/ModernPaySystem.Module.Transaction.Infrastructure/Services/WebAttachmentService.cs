using FileManager.Services.Abstraction;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ModernPaySystem.Module.Transaction.Application;
using ModernPaySystem.Module.Transaction.Application.Interfaces;
using ModernPaySystem.Module.Transaction.Domain;
using ModernPaySystem.Module.Transaction.Domain.Entities;
using ModernPaySystem.SharedKernel.Domain.Commons;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Transaction.Infrastructure.Services;

public class WebAttachmentService(
    IFilesManagerService fileManager,
    ITransactionUnitOfWork unitOfWork,
    ILogger<WebAttachmentService> logger) : IWebAttachmentService
{
    public async Task<Result<Attachment>> UploadFileToRequestAsync(IFormFile file, Guid requestId, string? subDirectory = null)
    {
        var request = await unitOfWork.Requests.GetByIdAsync(requestId);
        if (request.IsError)
            return TransactionErrors.RequestNotFound;

        string requestSubDirectory = Path.Combine("TransactionSystem", "Requests", requestId.ToString());

        if (!string.IsNullOrEmpty(subDirectory))
            requestSubDirectory = Path.Combine(requestSubDirectory, subDirectory);

        var fileResult = await fileManager.SaveFileAsync(file, requestSubDirectory);
        if (fileResult.IsError)
            return fileResult.Errors;

        var fileMetadata = fileResult.Value;

        var attachment = new Attachment
        {
            FileName = fileMetadata!.OriginalFileName,
            SafeName = fileMetadata.StoredFileName,
            Extension = fileMetadata.FileExtension,
            Path = fileMetadata.FilePath,
            Size = fileMetadata.FileSize
        };

        var attachmentResult = await unitOfWork.Attachments.AddAsync(attachment);
        if (attachmentResult.IsError)
        {
            await fileManager.DeleteFileAsync(fileMetadata.FilePath);
            return attachmentResult.Errors;
        }

        var requestAttachment = new RequestAttachment
        {
            RequestId = requestId,
            AttachmentId = attachment.Id
        };

        var associationResult = await unitOfWork.RequestAttachments.AddAsync(requestAttachment);
        if (associationResult.IsError)
        {
            await unitOfWork.Attachments.RemoveAsync(x => x.Id == attachment.Id);
            await fileManager.DeleteFileAsync(fileMetadata.FilePath);
            return associationResult.Errors;
        }

        int result = await unitOfWork.SaveChangesAsync();
        if (result <= 0)
            return TransactionErrors.DatabaseError;

        return attachment;
    }

    public async Task<Result<Attachment>> UploadFileToResponseAsync(IFormFile file, Guid responseId, string? subDirectory = null)
    {
        var response = await unitOfWork.Responses.GetByIdAsync(responseId);
        if (response.IsError)
            return TransactionErrors.ResponseNotFound;

        string responseSubDirectory = Path.Combine("TransactionSystem", "Responses", responseId.ToString());

        if (!string.IsNullOrEmpty(subDirectory))
            responseSubDirectory = Path.Combine(responseSubDirectory, subDirectory);

        var fileResult = await fileManager.SaveFileAsync(file, responseSubDirectory);
        if (fileResult.IsError)
            return fileResult.Errors;

        var fileMetadata = fileResult.Value;

        var attachment = new Attachment
        {
            FileName = fileMetadata!.OriginalFileName,
            SafeName = fileMetadata.StoredFileName,
            Extension = fileMetadata.FileExtension,
            Path = fileMetadata.FilePath,
            Size = fileMetadata.FileSize
        };

        var attachmentResult = await unitOfWork.Attachments.AddAsync(attachment);
        if (attachmentResult.IsError)
        {
            await fileManager.DeleteFileAsync(fileMetadata.FilePath);
            return attachmentResult.Errors;
        }

        var responseAttachment = new ResponseAttachment
        {
            ResponseId = responseId,
            AttachmentId = attachment.Id
        };

        var associationResult = await unitOfWork.ResponseAttachments.AddAsync(responseAttachment);
        if (associationResult.IsError)
        {
            await unitOfWork.Attachments.RemoveAsync(x => x.Id == attachment.Id);
            await fileManager.DeleteFileAsync(fileMetadata.FilePath);
            return associationResult.Errors;
        }

        int result = await unitOfWork.SaveChangesAsync();
        if (result <= 0)
            return TransactionErrors.DatabaseError;

        return attachment;
    }

    public async Task<Result<Attachment>> UploadFileToRequestTransactionAsync(IFormFile file, Guid requestTransactionId, string? subDirectory = null)
    {
        var requestTransaction = await unitOfWork.RequestTransactions.GetByIdAsync(requestTransactionId);
        if (requestTransaction.IsError)
            return TransactionErrors.RequestTransactionNotFound;

        string requestTransactionSubDirectory = Path.Combine("TransactionSystem", "RequestTransactions", requestTransactionId.ToString());

        if (!string.IsNullOrEmpty(subDirectory))
            requestTransactionSubDirectory = Path.Combine(requestTransactionSubDirectory, subDirectory);

        var fileResult = await fileManager.SaveFileAsync(file, requestTransactionSubDirectory);
        if (fileResult.IsError)
            return fileResult.Errors;

        var fileMetadata = fileResult.Value;

        var attachment = new Attachment
        {
            FileName = fileMetadata!.OriginalFileName,
            SafeName = fileMetadata.StoredFileName,
            Extension = fileMetadata.FileExtension,
            Path = fileMetadata.FilePath,
            Size = fileMetadata.FileSize
        };

        var attachmentResult = await unitOfWork.Attachments.AddAsync(attachment);
        if (attachmentResult.IsError)
        {
            await fileManager.DeleteFileAsync(fileMetadata.FilePath);
            return attachmentResult.Errors;
        }

        var requestTransactionAttachment = new RequestTransactionAttachment
        {
            RequestTransactionId = requestTransactionId,
            AttachmentId = attachment.Id
        };

        var associationResult = await unitOfWork.RequestTransactionAttachments.AddAsync(requestTransactionAttachment);
        if (associationResult.IsError)
        {
            await unitOfWork.Attachments.RemoveAsync(x => x.Id == attachment.Id);
            await fileManager.DeleteFileAsync(fileMetadata.FilePath);
            return associationResult.Errors;
        }

        int result = await unitOfWork.SaveChangesAsync();
        if (result <= 0)
            return TransactionErrors.DatabaseError;

        return attachment;
    }
}
