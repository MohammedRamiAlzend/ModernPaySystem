using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernPaySystem.Application.Services;
using ModernPaySystem.Domain.Attrs;
using ModernPaySystem.Domain.Entities.SharedEntities;
using ModernPaySystem.Infrastructure.Extensions;

namespace ModernPaySystem.Controllers;

/// <summary>
/// API controller for Attachment management
/// Provides CRUD operations and attachment-specific queries
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttachmentsController(IAttachmentService attachmentService, ILogger<AttachmentsController> logger) : ControllerBase
{
    /// <summary>
    /// Get all attachments.
    /// </summary>
    [HttpGet]
    [EndpointPermission("attachments.get-all", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetAll()
    {
        logger.LogInformation("Getting all attachments");
        var result = await attachmentService.GetAllAsync();
        return result.ToActionResult();
    }

    /// <summary>
    /// Get attachment by id.
    /// </summary>
    [HttpGet("{id}")]
    [EndpointPermission("attachments.get-by-id", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetById(Guid id)
    {
        logger.LogInformation("Getting attachment by id: {AttachmentId}", id);
        var result = await attachmentService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    /// <summary>
    /// Get attachments by file type.
    /// </summary>
    [HttpGet("by-type/{fileType}")]
    [EndpointPermission("attachments.get-by-file-type", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetByFileType(string fileType)
    {
        logger.LogInformation("Getting attachments by file type: {FileType}", fileType);
        var result = await attachmentService.GetByFileTypeAsync(fileType);
        return result.ToActionResult();
    }

    /// <summary>
    /// Get attachment by file name.
    /// </summary>
    [HttpGet("by-name/{fileName}")]
    [EndpointPermission("attachments.get-by-file-name", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetByFileName(string fileName)
    {
        logger.LogInformation("Getting attachment by file name: {FileName}", fileName);
        var result = await attachmentService.GetByFileNameAsync(fileName);
        return result.ToActionResult();
    }

    /// <summary>
    /// Create new attachment.
    /// </summary>
    [HttpPost]
    [EndpointPermission("attachments.create", SubSystem.TransactionSystem, PermissionType.Insert)]
    public async Task<IActionResult> Create([FromBody] Attachment attachment)
    {
        logger.LogInformation("Creating new attachment: {FileName}", attachment?.FileName);
        var result = await attachmentService.CreateAsync(attachment);
        return result.ToActionResult();
    }

    /// <summary>
    /// Update attachment.
    /// </summary>
    [HttpPut("{id}")]
    [EndpointPermission("attachments.update", SubSystem.TransactionSystem, PermissionType.Update)]
    public async Task<IActionResult> Update(Guid id, [FromBody] Attachment attachment)
    {
        logger.LogInformation("Updating attachment: {AttachmentId}", id);
        var result = await attachmentService.UpdateAsync(id, attachment);
        return result.ToActionResult();
    }

    /// <summary>
    /// Delete attachment.
    /// </summary>
    [HttpDelete("{id}")]
    [EndpointPermission("attachments.delete", SubSystem.TransactionSystem, PermissionType.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        logger.LogInformation("Deleting attachment: {AttachmentId}", id);
        var result = await attachmentService.DeleteAsync(id);
        return result.ToActionResult();
    }
}
