using ModernPaySystem.Domain.Entities.TransactionSystemEntities;

namespace ModernPaySystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ResponseTransactionsController(IResponseTransactionService responseTransactionService, ILogger<ResponseTransactionsController> logger) : ControllerBase
{
    [HttpGet("{id}")]
    [EndpointPermission("response-transactions.get-by-id", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetById(Guid id)
    {
        logger.LogInformation("Getting response transaction by id: {TransactionId}", id);
        var result = await responseTransactionService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpGet("by-response/{responseId}")]
    [EndpointPermission("response-transactions.get-by-response-id", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetByResponseId(Guid responseId)
    {
        logger.LogInformation("Getting response transactions for response: {ResponseId}", responseId);
        var result = await responseTransactionService.GetByResponseIdAsync(responseId);
        return result.ToActionResult();
    }

    [HttpGet("{parentTransactionId}/children")]
    [EndpointPermission("response-transactions.get-children", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetChildTransactions(Guid parentTransactionId)
    {
        logger.LogInformation("Getting child transactions for parent: {ParentTransactionId}", parentTransactionId);
        var result = await responseTransactionService.GetChildTransactionsAsync(parentTransactionId);
        return result.ToActionResult();
    }

    [HttpGet("root/{responseId}")]
    [EndpointPermission("response-transactions.get-root", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetRootTransaction(Guid responseId)
    {
        logger.LogInformation("Getting root transaction for response: {ResponseId}", responseId);
        var result = await responseTransactionService.GetRootTransactionAsync(responseId);
        return result.ToActionResult();
    }

    [HttpGet("{transactionId}/tree")]
    [EndpointPermission("response-transactions.get-tree", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetTransactionTree(Guid transactionId)
    {
        logger.LogInformation("Getting transaction tree for transaction: {TransactionId}", transactionId);
        var result = await responseTransactionService.GetTransactionTreeAsync(transactionId);
        return result.ToActionResult();
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [EndpointPermission("response-transactions.create", SubSystem.TransactionSystem, PermissionType.Insert)]
    public async Task<IActionResult> Create([FromForm] CreateResponseTransactionDto dto)
    {
        logger.LogInformation("Creating new response transaction for response: {ResponseId}", dto?.ResponseId);
        ArgumentNullException.ThrowIfNull(dto);
        var result = await responseTransactionService.CreateAsync(dto);
        return result.ToActionResult();
    }

    //[HttpPut("{id}")]
    //[EndpointPermission("response-transactions.update", SubSystem.TransactionSystem, PermissionType.Update)]
    //public async Task<IActionResult> Update(Guid id, [FromBody] UpdateResponseTransactionDto dto)
    //{
    //    logger.LogInformation("Updating response transaction: {TransactionId}", id);
    //    var result = await responseTransactionService.UpdateAsync(id, dto);
    //    return result.ToActionResult();
    //}

    //[HttpDelete("{id}")]
    //[EndpointPermission("response-transactions.delete", SubSystem.TransactionSystem, PermissionType.Delete)]
    //public async Task<IActionResult> Delete(Guid id)
    //{
    //    logger.LogInformation("Deleting response transaction: {TransactionId}", id);
    //    var result = await responseTransactionService.DeleteAsync(id);
    //    return result.ToActionResult();
    //}

    //[HttpPost("{transactionId}/files")]
    //[Consumes("multipart/form-data")]
    //[EndpointPermission("response-transactions.add-files", SubSystem.TransactionSystem, PermissionType.Insert)]
    //public async Task<IActionResult> AddFiles(Guid transactionId, [FromForm] List<IFormFile> files)
    //{
    //    logger.LogInformation("Adding {FileCount} files to transaction: {TransactionId}", files?.Count, transactionId);
    //    ArgumentNullException.ThrowIfNull(files);
    //    var result = await responseTransactionService.AddFilesAsync(transactionId, files);
    //    return result.ToActionResult();
    //}

    //[HttpDelete("{transactionId}/attachments/{attachmentId}")]
    //[EndpointPermission("response-transactions.remove-attachment", SubSystem.TransactionSystem, PermissionType.Delete)]
    //public async Task<IActionResult> RemoveAttachment(Guid transactionId, Guid attachmentId)
    //{
    //    logger.LogInformation("Removing attachment: {AttachmentId} from transaction: {TransactionId}", attachmentId, transactionId);
    //    var result = await responseTransactionService.RemoveAttachmentAsync(transactionId, attachmentId);
    //    return result.ToActionResult();
    //}

    [HttpPost("{parentTransactionId}/children")]
    [Consumes("multipart/form-data")]
    [EndpointPermission("response-transactions.add-child", SubSystem.TransactionSystem, PermissionType.Insert)]
    public async Task<IActionResult> AddChildTransaction(Guid parentTransactionId, [FromForm] CreateResponseTransactionDto dto)
    {
        logger.LogInformation("Adding child transaction to parent: {ParentTransactionId}", parentTransactionId);
        ArgumentNullException.ThrowIfNull(dto);
        var result = await responseTransactionService.AddChildTransactionAsync(parentTransactionId, dto);
        return result.ToActionResult();
    }

    //[HttpDelete("{parentTransactionId}/children/{childTransactionId}")]
    //[EndpointPermission("response-transactions.remove-child", SubSystem.TransactionSystem, PermissionType.Delete)]
    //public async Task<IActionResult> RemoveChildTransaction(Guid parentTransactionId, Guid childTransactionId)
    //{
    //    logger.LogInformation("Removing child transaction: {ChildTransactionId} from parent: {ParentTransactionId}", childTransactionId, parentTransactionId);
    //    var result = await responseTransactionService.RemoveChildTransactionAsync(parentTransactionId, childTransactionId);
    //    return result.ToActionResult();
    //}

    //[HttpGet]
    //[EndpointPermission("response-transactions.get-paged", SubSystem.TransactionSystem, PermissionType.Read)]
    //public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    //{
    //    logger.LogInformation("Getting paged response transactions, page: {Page}, size: {PageSize}", page, pageSize);
    //    var result = await responseTransactionService.GetPagedAsync(page, pageSize);
    //    return result.ToActionResult();
    //}
}
