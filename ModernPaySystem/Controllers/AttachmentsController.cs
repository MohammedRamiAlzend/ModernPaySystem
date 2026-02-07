using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernPaySystem.Application.Services;
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
public class AttachmentsController : ControllerBase
{
    private readonly IAttachmentService _attachmentService;
    private readonly ILogger<AttachmentsController> _logger;

    public AttachmentsController(IAttachmentService attachmentService, ILogger<AttachmentsController> logger)
    {
        _attachmentService = attachmentService;
        _logger = logger;
    }

    /// <summary>
    /// Get all attachments
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Getting all attachments");
        var result = await _attachmentService.GetAllAsync();
        return result.ToActionResult();
    }

    /// <summary>
    /// Get attachment by id
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        _logger.LogInformation("Getting attachment by id: {AttachmentId}", id);
        var result = await _attachmentService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    /// <summary>
    /// Get attachments by file type
    /// </summary>
    [HttpGet("by-type/{fileType}")]
    public async Task<IActionResult> GetByFileType(string fileType)
    {
        _logger.LogInformation("Getting attachments by file type: {FileType}", fileType);
        var result = await _attachmentService.GetByFileTypeAsync(fileType);
        return result.ToActionResult();
    }

    /// <summary>
    /// Get attachment by file name
    /// </summary>
    [HttpGet("by-name/{fileName}")]
    public async Task<IActionResult> GetByFileName(string fileName)
    {
        _logger.LogInformation("Getting attachment by file name: {FileName}", fileName);
        var result = await _attachmentService.GetByFileNameAsync(fileName);
        return result.ToActionResult();
    }

    /// <summary>
    /// Create new attachment
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Attachment attachment)
    {
        _logger.LogInformation("Creating new attachment: {FileName}", attachment?.FileName);
        var result = await _attachmentService.CreateAsync(attachment);
        return result.ToActionResult();
    }

    /// <summary>
    /// Update attachment
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Attachment attachment)
    {
        _logger.LogInformation("Updating attachment: {AttachmentId}", id);
        var result = await _attachmentService.UpdateAsync(id, attachment);
        return result.ToActionResult();
    }

    /// <summary>
    /// Delete attachment
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogInformation("Deleting attachment: {AttachmentId}", id);
        var result = await _attachmentService.DeleteAsync(id);
        return result.ToActionResult();
    }
}
