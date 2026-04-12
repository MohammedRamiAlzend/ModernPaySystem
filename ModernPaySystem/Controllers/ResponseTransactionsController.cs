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
    public async Task<IActionResult> Create([FromForm] AddInitialResponseTransactionDto dto)
    {
        logger.LogInformation("Creating new response transaction for response: {ResponseId}", dto?.ResponseId);
        ArgumentNullException.ThrowIfNull(dto);
        var result = await responseTransactionService.AddInitialResponseTransaction(dto);
        return result.ToActionResult();
    }

    [HttpPost("AddTransactionChildren")]
    [Consumes("multipart/form-data")]
    [EndpointPermission("response-transactions.add-child", SubSystem.TransactionSystem, PermissionType.Insert)]
    public async Task<IActionResult> AddChildTransaction([FromForm] CreateResponseTransactionDto dto)
    {
        logger.LogInformation("Adding child transaction to parent: {ParentTransactionId}", dto.ParentTransactionId);
        ArgumentNullException.ThrowIfNull(dto);
        var result = await responseTransactionService.AddChildTransactionAsync(dto);
        return result.ToActionResult();
    }

    [HttpGet]
    [EndpointPermission("response-transactions.get-paged", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] TransactionStatus? status = null)
    {
        logger.LogInformation("Getting paged response transactions, page: {Page}, size: {PageSize}, status: {Status}", page, pageSize, status);
        var result = await responseTransactionService.GetPagedAsync(page, pageSize, status);
        return result.ToActionResult();
    }

    [HttpPost("{responseId}/mark-as-managed")]
    [EndpointPermission("response-transactions.mark-as-managed", SubSystem.TransactionSystem, PermissionType.Update)]
    public async Task<IActionResult> MarkAsManaged(Guid responseId)
    {
        logger.LogInformation("Marking response as managed: {ResponseId}", responseId);
        var result = await responseTransactionService.MarkAsManagedAsync(responseId);
        return result.ToActionResult();
    }
}
