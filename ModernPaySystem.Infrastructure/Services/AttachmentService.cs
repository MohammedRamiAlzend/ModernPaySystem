using FileManager.Services.Abstraction;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.IO.Compression;

namespace ModernPaySystem.Infrastructure.Services;

/// <summary>
/// Service for handling file attachments for requests and responses.
/// </summary>
public class AttachmentService(
    IFilesManagerService fileManager,
    IUnitOfWork unitOfWork) : IAttachmentService
{
    /// <summary>
    /// Uploads a file and associates it with a request.
    /// </summary>
    public async Task<Result<Attachment>> UploadFileToRequestAsync(IFormFile file, Guid requestId, string? subDirectory = null)
    {
        // Verify the request exists
        var request = await unitOfWork.Requests.GetByIdAsync(requestId);
        if (request == null)
        {
            return ApplicationErrors.RequestNotFound;
        }

        // Save the file using the file manager
        var fileResult = await fileManager.SaveFileAsync(file, subDirectory);
        if (fileResult.IsError)
        {
            return fileResult.Errors;
        }

        var fileMetadata = fileResult.Value;

        // Create an attachment entity
        var attachment = new Attachment
        {
            FileName = fileMetadata!.OriginalFileName,
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

        var associationResult = await unitOfWork.RequestAttachments.AddAsync(
            new RequestAttachment
            {
                AttachmentId = attachment.Id,
                RequestId = requestId
            });
        if (associationResult.IsError)
        {
            // Clean up: remove the attachment from DB and file system if association fails
            await unitOfWork.Attachments.RemoveAsync(x => x.Id == attachment.Id);
            await fileManager.DeleteFileAsync(fileMetadata.FilePath);
            return associationResult.Errors;
        }

        int result = await unitOfWork.SaveChangesAsync();
        if (result <= 0)
            return ApplicationErrors.DatabaseError;

        return attachment;
    }

    /// <summary>
    /// Uploads a file and associates it with a response.
    /// </summary>
    public async Task<Result<Attachment>> UploadFileToResponseAsync(IFormFile file, Guid responseId, string? subDirectory = null)
    {
        // Verify the response exists
        var response = await unitOfWork.Responses.GetByIdAsync(responseId);
        if (response.IsError)
        {
            return ApplicationErrors.ResponseNotFound;
        }

        // Save the file using the file manager
        var fileResult = await fileManager.SaveFileAsync(file, subDirectory);
        if (fileResult.IsError)
        {
            return fileResult.Errors;
        }

        var fileMetadata = fileResult.Value;

        // Create an attachment entity
        var attachment = new Attachment
        {
            FileName = fileMetadata!.OriginalFileName,
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

        int result = await unitOfWork.SaveChangesAsync();
        if (result <= 0)
            return ApplicationErrors.DatabaseError;

        return attachment;
    }

    /// <summary>
    /// Downloads a file associated with a request.
    /// </summary>
    public async Task<Result<byte[]>> DownloadFileFromRequestAsync(Guid requestId, Guid attachmentId)
    {
        // Verify the request exists
        var request = await unitOfWork.Requests.GetByIdAsync(requestId);
        if (request.IsError)
        {
            return ApplicationErrors.RequestNotFound;
        }

        // Check if the attachment is associated with this request
        var requestAttachment = await unitOfWork.RequestAttachments.GetAsync(
            AttachmentExpressions.RequestAttachmentByRequestIdAndAttachmentId(requestId, attachmentId),
            i => i.Include(x => x.Attachment).Include(x => x.Request));
        if (requestAttachment == null)
        {
            return ApplicationErrors.AttachmentNotFound;
        }

        // Get the attachment details
        var attachment = await unitOfWork.Attachments.GetByIdAsync(attachmentId);
        if (attachment.IsError)
        {
            return ApplicationErrors.AttachmentNotFound;
        }

        // Return the file content
        return await fileManager.GetFileBytesAsync(attachment.Value!.Path);
    }

    /// <summary>
    /// Downloads a file associated with a response.
    /// </summary>
    public async Task<Result<byte[]>> DownloadFileFromResponseAsync(Guid responseId, Guid attachmentId)
    {
        // Verify the response exists
        var response = await unitOfWork.Responses.GetByIdAsync(responseId);
        if (response.IsError)
        {
            return ApplicationErrors.ResponseNotFound;
        }

        // Check if the attachment is associated with this response
        var responseAttachment = await unitOfWork.ResponseAttachments.GetAsync(
            AttachmentExpressions.ResponseAttachmentByResponseIdAndAttachmentId(responseId, attachmentId),
            x => x.Include(x => x.Attachment).Include(x => x.Response));

        if (responseAttachment.IsError)
        {
            return ApplicationErrors.AttachmentNotFound;
        }

        // Get the attachment details
        var attachment = await unitOfWork.Attachments.GetByIdAsync(attachmentId);
        if (attachment.IsError)
        {
            return ApplicationErrors.AttachmentNotFound;
        }

        // Return the file content
        return await fileManager.GetFileBytesAsync(attachment.Value!.Path);
    }

    /// <summary>
    /// Downloads a file associated with a transaction.
    /// </summary>
    public async Task<Result<byte[]>> DownloadFileFromTransactionAsync(Guid transactionId, Guid attachmentId)
    {
        // Verify the transaction exists
        var transaction = await unitOfWork.RequestTransactions.GetByIdAsync(transactionId);
        if (transaction.IsError)
        {
            return ApplicationErrors.RequestTransactionNotFound;
        }

        // Check if the attachment is associated with this transaction
        var transactionAttachment = await unitOfWork.RequestTransactionAttachments.GetAsync(
            AttachmentExpressions.RequestTransactionAttachmentByTransactionIdAndAttachmentId(transactionId, attachmentId),
            x => x.Include(x => x.Attachment).Include(x => x.RequestTransaction));

        if (transactionAttachment.IsError)
        {
            return ApplicationErrors.AttachmentNotFound;
        }

        // Get the attachment details
        var attachment = await unitOfWork.Attachments.GetByIdAsync(attachmentId);
        if (attachment.IsError)
        {
            return ApplicationErrors.AttachmentNotFound;
        }

        // Return the file content
        return await fileManager.GetFileBytesAsync(attachment.Value!.Path);
    }

    /// <summary>
    /// Removes a file attachment from a request.
    /// </summary>
    public async Task<Result<Success>> RemoveFileFromRequestAsync(Guid requestId, Guid attachmentId)
    {
        // Verify the request exists
        var request = await unitOfWork.Requests.GetByIdAsync(requestId);
        if (request.IsError)
        {
            return ApplicationErrors.RequestNotFound;
        }

        // Check if the attachment is associated with this request
        var requestAttachment = await unitOfWork.RequestAttachments.GetAsync(
            AttachmentExpressions.RequestAttachmentByRequestIdAndAttachmentId(requestId, attachmentId),
            x => x.Include(x => x.Attachment).Include(x => x.Request));
        if (requestAttachment.IsError)
        {
            return ApplicationErrors.AttachmentNotFound;
        }

        // Get the attachment details
        var attachment = await unitOfWork.Attachments.GetByIdAsync(attachmentId);
        if (attachment.IsError)
        {
            return ApplicationErrors.AttachmentNotFound;
        }

        // Remove the association
        var removeAssociationResult = await unitOfWork.RequestAttachments.RemoveAsync(x => x.Id == requestAttachment.Value!.Id);
        if (removeAssociationResult.IsError)
        {
            return removeAssociationResult.Errors;
        }

        // If this attachment is not associated with any other requests/responses, delete it
        var isUsedElsewhereResult = await IsAttachmentUsedElsewhere(attachmentId);
        if (isUsedElsewhereResult.IsError)
            return isUsedElsewhereResult.Errors;
        if (!isUsedElsewhereResult.Value)
        {
            // Delete the file from the file system
            var fileDeleteResult = await fileManager.DeleteFileAsync(attachment.Value!.Path);
            if (fileDeleteResult.IsError)
            {
                // Log the error but don't fail the operation as the DB records are cleaned up
                // In a real application, you might want to implement a cleanup job for orphaned files
            }

            // Delete the attachment from the database
            var attachmentDeleteResult = await unitOfWork.Attachments.RemoveAsync(x => x.Id == attachmentId);
            if (attachmentDeleteResult.IsError)
            {
                return attachmentDeleteResult.Errors;
            }
        }

        return Result.Success;
    }

    /// <summary>
    /// Removes a file attachment from a response.
    /// </summary>
    public async Task<Result<Success>> RemoveFileFromResponseAsync(Guid responseId, Guid attachmentId)
    {
        // Verify the response exists
        var response = await unitOfWork.Responses.GetByIdAsync(responseId);
        if (response.IsError)
        {
            return ApplicationErrors.ResponseNotFound;
        }

        // Check if the attachment is associated with this response
        var responseAttachment = await unitOfWork.ResponseAttachments.GetAsync(
            AttachmentExpressions.ResponseAttachmentByResponseIdAndAttachmentId(responseId, attachmentId),
            x => x.Include(x => x.Attachment).Include(x => x.Response));
        if (responseAttachment == null)
        {
            return ApplicationErrors.AttachmentNotFound;
        }

        // Get the attachment details
        var attachment = await unitOfWork.Attachments.GetByIdAsync(attachmentId);
        if (attachment.IsError)
        {
            return ApplicationErrors.AttachmentNotFound;
        }

        // Remove the association
        var removeAssociationResult = await unitOfWork.ResponseAttachments.RemoveAsync(x => x.Id == responseAttachment.Value!.Id);
        if (removeAssociationResult.IsError)
        {
            return removeAssociationResult.Errors;
        }

        // If this attachment is not associated with any other requests/responses, delete it
        var isUsedElsewhereResult = await IsAttachmentUsedElsewhere(attachmentId);
        if (isUsedElsewhereResult.IsError)
            return isUsedElsewhereResult.Errors;
        if (!isUsedElsewhereResult.Value)
        {
            // Delete the file from the file system
            var fileDeleteResult = await fileManager.DeleteFileAsync(attachment.Value!.Path);
            if (fileDeleteResult.IsError)
            {
                // Log the error but don't fail the operation as the DB records are cleaned up
                // In a real application, you might want to implement a cleanup job for orphaned files
            }

            // Delete the attachment from the database
            var attachmentDeleteResult = await unitOfWork.Attachments.RemoveAsync(x => x.Id == attachmentId);
            if (attachmentDeleteResult.IsError)
            {
                return attachmentDeleteResult.Errors;
            }
        }

        return Result.Success;
    }

    public async Task<Result<IEnumerable<AttachmentDto>>> GetAllAsync()
    {
        var attachments = await unitOfWork.Attachments.GetAllAsync();
        if (attachments.IsError)
        {
            return attachments.Errors;
        }

        return attachments.Value!.ConvertAll(a => a.ToDto());
    }

    public async Task<Result<PagedList<AttachmentDto>>> GetPagedAsync(int page, int pageSize)
    {
        // For simplicity, we'll get all attachments and then page them
        // In a real implementation, you'd want to implement proper pagination at the DB level
        var allAttachments = await GetAllAsync();
        if (allAttachments.IsError)
        {
            return allAttachments.Errors;
        }

        var attachmentsList = allAttachments.Value!.ToList();
        var pagedAttachments = attachmentsList.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new PagedList<AttachmentDto>(
            pagedAttachments,
            attachmentsList.Count,
            page,
            pageSize);
    }

    public async Task<Result<AttachmentDto>> GetByIdAsync(Guid id)
    {
        var attachment = await unitOfWork.Attachments.GetByIdAsync(id);
        if (attachment.IsError)
        {
            return attachment.Errors;
        }

        if (attachment.Value == null)
        {
            return ApplicationErrors.AttachmentNotFound;
        }

        return attachment.Value.ToDto();
    }

    public async Task<Result<IEnumerable<AttachmentDto>>> GetByFileTypeAsync(string fileType)
    {
        // Filter attachments by file type in memory
        // In a real implementation, you'd want to implement this at the DB level
        var allAttachments = await GetAllAsync();
        if (allAttachments.IsError)
        {
            return allAttachments.Errors;
        }

        var filteredAttachments = allAttachments.Value!.Where(a => a.Extension?.Equals(fileType, StringComparison.OrdinalIgnoreCase) == true);
        return filteredAttachments.ToList();
    }

    public async Task<Result<AttachmentDto>> GetByFileNameAsync(string fileName)
    {
        // Find attachment by file name in memory
        // In a real implementation, you'd want to implement this at the DB level
        var allAttachments = await GetAllAsync();
        if (allAttachments.IsError)
        {
            return allAttachments.Errors;
        }

        var attachmentDto = allAttachments.Value!.FirstOrDefault(a => a.FileName?.Equals(fileName, StringComparison.OrdinalIgnoreCase) == true);
        return attachmentDto == null ? ApplicationErrors.AttachmentNotFound : attachmentDto;
    }

    public async Task<Result<AttachmentDto>> CreateAsync(CreateAttachmentDto attachment)
    {
        var attachmentEntity = new Attachment
        {
            FileName = attachment.FileName,
            SafeName = attachment.SafeName,
            Extension = attachment.Extension,
            Path = attachment.Path
        };

        var result = await unitOfWork.Attachments.AddAsync(attachmentEntity);
        if (result.IsError)
        {
            return result.Errors;
        }

        return attachmentEntity.ToDto();
    }

    public async Task<Result<AttachmentDto>> UpdateAsync(Guid id, UpdateAttachmentDto attachment)
    {
        var existingAttachment = await unitOfWork.Attachments.GetByIdAsync(id);
        if (existingAttachment.IsError)
        {
            return existingAttachment.Errors;
        }

        if (existingAttachment.Value == null)
        {
            return ApplicationErrors.AttachmentNotFound;
        }

        existingAttachment.Value.FileName = attachment.FileName;
        existingAttachment.Value.SafeName = attachment.SafeName;
        existingAttachment.Value.Extension = attachment.Extension;
        existingAttachment.Value!.Path = attachment.Path;

        var result = await unitOfWork.Attachments.UpdateAsync(existingAttachment.Value);
        if (result.IsError)
        {
            return result.Errors;
        }

        return existingAttachment.Value.ToDto();
    }

    private async Task<Result<bool>> IsAttachmentUsedElsewhere(Guid attachmentId)
    {
        var requestAttachments = await unitOfWork.RequestAttachments.GetAllAsync(AttachmentExpressions.RequestAttachmentByAttachmentId(attachmentId));
        if (requestAttachments.IsError)
            return requestAttachments.Errors;

        if (requestAttachments.Value!.Any())
            return true;

        var responseAttachments = await unitOfWork.ResponseAttachments.GetAllAsync(AttachmentExpressions.ResponseAttachmentByAttachmentId(attachmentId));
        if (responseAttachments.IsError)
            return responseAttachments.Errors;

        return responseAttachments.Value!.Any();
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        var result = await unitOfWork.Attachments.RemoveAsync(x => x.Id == id);

        if (result.IsError)
        {
            return result.Errors;
        }

        return true;
    }
}
