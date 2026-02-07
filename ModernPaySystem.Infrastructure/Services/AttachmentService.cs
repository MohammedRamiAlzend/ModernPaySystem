using Microsoft.Extensions.Logging;
using ModernPaySystem.Application.Interfaces;
using ModernPaySystem.Domain.Commons;
using ModernPaySystem.Domain.Entities.SharedEntities;
using ModernPaySystem.Infrastructure.Persistence;

namespace ModernPaySystem.Infrastructure.Services;

/// <summary>
/// Implementation of Attachment service CRUD operations
/// </summary>
public class AttachmentService : IAttachmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AttachmentService> _logger;

    public AttachmentService(IUnitOfWork unitOfWork, ILogger<AttachmentService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<Attachment>>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all attachments");
            var attachments = await _unitOfWork.Attachments.GetAllAsync();
            if (attachments.IsError)
                return attachments.Errors;

            return attachments.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all attachments");
            return ApplicationError.InternalServerError;
        }
    }

    public async Task<Result<Attachment>> GetByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Fetching attachment by id: {AttachmentId}", id);
            var attachment = await _unitOfWork.Attachments.GetByIdAsync(id);

            if (attachment.IsError)
                return attachment.Errors;

            if (attachment.Value == null)
                return ApplicationError.AttachmentNotFound;

            return attachment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching attachment by id: {AttachmentId}", id);
            return ApplicationError.InternalServerError;
        }
    }

    public async Task<Result<IEnumerable<Attachment>>> GetByFileTypeAsync(string fileType)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileType))
                return ApplicationError.InvalidInput;

            _logger.LogInformation("Fetching attachments by file type: {FileType}", fileType);
            var attachments = await _unitOfWork.Attachments.GetAllAsync();
            if (attachments.IsError)
                return attachments.Errors;

            return attachments.Value.Where(a => a.Extension == fileType).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching attachments by file type: {FileType}", fileType);
            return ApplicationError.InternalServerError;
        }
    }

    public async Task<Result<Attachment>> GetByFileNameAsync(string fileName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return ApplicationError.InvalidInput;

            _logger.LogInformation("Fetching attachment by file name: {FileName}", fileName);
            var attachments = await _unitOfWork.Attachments.GetAllAsync();
            if (attachments.IsError)
                return attachments.Errors;

            var attachment = attachments.Value.FirstOrDefault(a => a.FileName == fileName);

            if (attachment == null)
                return ApplicationError.AttachmentNotFound;

            return attachment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching attachment by file name: {FileName}", fileName);
            return ApplicationError.InternalServerError;
        }
    }

    public async Task<Result<Attachment>> CreateAsync(Attachment attachment)
    {
        try
        {
            if (attachment == null)
                return ApplicationError.InvalidInput;

            if (string.IsNullOrWhiteSpace(attachment.FileName))
                return ApplicationError.MissingRequiredField;

            _logger.LogInformation("Creating new attachment: {FileName}", attachment.FileName);

            await _unitOfWork.Attachments.AddAsync(attachment);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully created attachment: {FileName}", attachment.FileName);
            return attachment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating attachment");
            return ApplicationError.InternalServerError;
        }
    }

    public async Task<Result<Attachment>> UpdateAsync(Guid id, Attachment attachment)
    {
        try
        {
            if (id == Guid.Empty || attachment == null)
                return ApplicationError.InvalidInput;

            var existingAttachment = await _unitOfWork.Attachments.GetByIdAsync(id);
            if (existingAttachment.IsError)
                return existingAttachment.Errors;

            if (existingAttachment.Value == null)
                return ApplicationError.AttachmentNotFound;

            _logger.LogInformation("Updating attachment: {AttachmentId}", id);

            existingAttachment.Value.FileName = attachment.FileName;
            existingAttachment.Value.SafeName = attachment.SafeName;
            existingAttachment.Value.Extension = attachment.Extension;
            existingAttachment.Value.Path = attachment.Path;

            await _unitOfWork.Attachments.UpdateAsync(existingAttachment.Value);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully updated attachment: {AttachmentId}", id);
            return existingAttachment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating attachment: {AttachmentId}", id);
            return ApplicationError.InternalServerError;
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
                return ApplicationError.InvalidInput;

            var attachment = await _unitOfWork.Attachments.GetByIdAsync(id);
            if (attachment.IsError)
                return attachment.Errors;

            if (attachment.Value == null)
                return ApplicationError.AttachmentNotFound;

            _logger.LogInformation("Deleting attachment: {AttachmentId}", id);

            await _unitOfWork.Attachments.RemoveAsync(x => x.Id == attachment.Value.Id);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully deleted attachment: {AttachmentId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting attachment: {AttachmentId}", id);
            return ApplicationError.InternalServerError;
        }
    }
}
