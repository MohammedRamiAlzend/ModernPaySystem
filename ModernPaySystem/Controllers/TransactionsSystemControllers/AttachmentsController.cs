namespace ModernPaySystem.Controllers.TransactionsSystemControllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttachmentsController(IAttachmentService attachmentService, ILogger<AttachmentsController> logger) : ControllerBase
{
    [HttpGet("{id}")]
    [EndpointPermission("attachments.get-by-id", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetById(Guid id)
    {
        logger.LogInformation("Getting attachment by id: {AttachmentId}", id);
        var result = await attachmentService.GetByIdAsync(id);
        return result.ToActionResult();
    }

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

    [HttpGet("transaction/{transactionId}/download-all")]
    [EndpointPermission("attachments.download-all-from-transaction", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> DownloadAllFilesFromTransaction(Guid transactionId)
    {
        logger.LogInformation("Downloading all files from transaction: {TransactionId}", transactionId);
        var result = await attachmentService.DownloadFilesFromTransactionAsync(transactionId);
        if (result.IsError)
        {
            return result.ToActionResult();
        }

        return File(result.Value!, "application/zip", $"Transaction_{transactionId}_Attachments.zip");
    }
}
