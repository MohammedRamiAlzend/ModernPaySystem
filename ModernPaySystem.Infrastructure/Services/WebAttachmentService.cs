using FileManager.Services.Abstraction;
using Microsoft.AspNetCore.Http;
using ModernPaySystem.Domain.Entities.TransactionSystemEntities;
using ModernPaySystem.Domain.Commons;
using System.IO;

namespace ModernPaySystem.Infrastructure.Services;

public class WebAttachmentService(
    IFilesManagerService fileManager,
    IUnitOfWork unitOfWork) : IWebAttachmentService
{
    public async Task<Result<Attachment>> UploadFileToRequestAsync(IFormFile file, Guid requestId, string? subDirectory = null)
    {
        // Verify the request exists
        var request = await unitOfWork.Requests.GetByIdAsync(requestId);
        if (request.IsError)
        {
            return ApplicationErrors.RequestNotFound;
        }

        // Create a custom subdirectory for this request
        string requestSubDirectory = Path.Combine("TransactionSystem", "Requests", requestId.ToString());

        // If a subdirectory was provided, append it to the request directory
        if (!string.IsNullOrEmpty(subDirectory))
        {
            requestSubDirectory = Path.Combine(requestSubDirectory, subDirectory);
        }

        // Save the file using the file manager
        var fileResult = await fileManager.SaveFileAsync(file, requestSubDirectory);
        if (fileResult.IsError)
        {
            return fileResult.Errors;
        }

        var fileMetadata = fileResult.Value;

        // Create an attachment entity
        var attachment = new Attachment
        {
            FileName = fileMetadata.OriginalFileName,
            SafeName = fileMetadata.StoredFileName,
            Extension = fileMetadata.FileExtension,
            Path = fileMetadata.FilePath
        };

        // Save the attachment to the database
        var attachmentResult = await unitOfWork.Attachments.AddAsync(attachment);
        if (attachmentResult.IsError)
        {
            // Clean up the uploaded file if DB operation fails
            await fileManager.DeleteFileAsync(fileMetadata.FilePath);
            return attachmentResult.Errors;
        }

        // Associate the attachment with the request
        var requestAttachment = new RequestAttachment
        {
            RequestId = requestId,
            AttachmentId = attachment.Id
        };

        var associationResult = await unitOfWork.RequestAttachments.AddAsync(requestAttachment);
        if (associationResult.IsError)
        {
            // Clean up: remove the attachment from DB and file system if association fails
            await unitOfWork.Attachments.RemoveAsync(x => x.Id == attachment.Id);
            await fileManager.DeleteFileAsync(fileMetadata.FilePath);
            return associationResult.Errors;
        }

        return attachment;
    }

    public async Task<Result<Attachment>> UploadFileToResponseAsync(IFormFile file, Guid responseId, string? subDirectory = null)
    {
        // Verify the response exists
        var response = await unitOfWork.Responses.GetByIdAsync(responseId);
        if (response.IsError)
        {
            return ApplicationErrors.ResponseNotFound;
        }

        // Create a custom subdirectory for this response
        string responseSubDirectory = Path.Combine("TransactionSystem", "Responses", responseId.ToString());

        // If a subdirectory was provided, append it to the response directory
        if (!string.IsNullOrEmpty(subDirectory))
        {
            responseSubDirectory = Path.Combine(responseSubDirectory, subDirectory);
        }

        // Save the file using the file manager
        var fileResult = await fileManager.SaveFileAsync(file, responseSubDirectory);
        if (fileResult.IsError)
        {
            return fileResult.Errors;
        }

        var fileMetadata = fileResult.Value;

        // Create an attachment entity
        var attachment = new Attachment
        {
            FileName = fileMetadata.OriginalFileName,
            SafeName = fileMetadata.StoredFileName,
            Extension = fileMetadata.FileExtension,
            Path = fileMetadata.FilePath
        };

        // Save the attachment to the database
        var attachmentResult = await unitOfWork.Attachments.AddAsync(attachment);
        if (attachmentResult.IsError)
        {
            // Clean up the uploaded file if DB operation fails
            await fileManager.DeleteFileAsync(fileMetadata.FilePath);
            return attachmentResult.Errors;
        }

        // Associate the attachment with the response
        var responseAttachment = new ResponseAttachment
        {
            ResponseId = responseId,
            AttachmentId = attachment.Id
        };

        var associationResult = await unitOfWork.ResponseAttachments.AddAsync(responseAttachment);
        if (associationResult.IsError)
        {
            // Clean up: remove the attachment from DB and file system if association fails
            await unitOfWork.Attachments.RemoveAsync(x => x.Id == attachment.Id);
            await fileManager.DeleteFileAsync(fileMetadata.FilePath);
            return associationResult.Errors;
        }

        return attachment;
    }
}