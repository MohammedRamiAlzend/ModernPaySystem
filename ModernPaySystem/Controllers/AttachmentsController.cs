namespace ModernPaySystem.Controllers;

/// <summary>
/// API controller for Attachment management
/// Provides CRUD operations and attachment-specific queries.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttachmentsController(IAttachmentService attachmentService, ILogger<AttachmentsController> logger) : ControllerBase
{

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
    /// Downloads all files associated with a request as a ZIP archive.
    /// </summary>
    [HttpGet("request/{requestId}/download-all")]
    [EndpointPermission("attachments.download-all-from-request", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> DownloadAllFilesFromRequest(Guid requestId)
    {
        logger.LogInformation("Downloading all files from request: {RequestId}", requestId);
        var result = await attachmentService.DownloadFilesFromRequestAsync(requestId);
        if (result.IsError)
        {
            return result.ToActionResult();
        }

        return File(result.Value!, "application/zip", $"Request_{requestId}_Attachments.zip");
    }

    /// <summary>
    /// Downloads all files associated with a response as a ZIP archive.
    /// </summary>
    [HttpGet("response/{responseId}/download-all")]
    [EndpointPermission("attachments.download-all-from-response", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> DownloadAllFilesFromResponse(Guid responseId)
    {
        logger.LogInformation("Downloading all files from response: {ResponseId}", responseId);
        var result = await attachmentService.DownloadFilesFromResponseAsync(responseId);
        if (result.IsError)
        {
            return result.ToActionResult();
        }

        return File(result.Value!, "application/zip", $"Response_{responseId}_Attachments.zip");
    }
}
