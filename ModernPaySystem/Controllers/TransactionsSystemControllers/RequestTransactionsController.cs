using ModernPaySystem.Domain.DTOs;
using ModernPaySystem.Domain.Entities.TransactionSystemEntities;

namespace ModernPaySystem.Controllers.TransactionsSystemControllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RequestTransactionsController(IRequestTransactionService requestTransactionService, ILogger<RequestTransactionsController> logger) : ControllerBase
{
    [HttpGet("{id}")]
    [EndpointPermission("request-transactions.get-by-id", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetById(Guid id)
    {
        logger.LogInformation("Getting request transaction by id: {TransactionId}", id);
        var result = await requestTransactionService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpGet("by-request/{requestId}")]
    [EndpointPermission("request-transactions.get-by-request-id", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetByRequestId(Guid requestId)
    {
        logger.LogInformation("Getting request transactions for request: {RequestId}", requestId);
        var result = await requestTransactionService.GetByRequestIdAsync(requestId);
        return result.ToActionResult();
    }

    [HttpGet("{parentTransactionId}/children")]
    [EndpointPermission("request-transactions.get-children", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetChildTransactions(Guid parentTransactionId)
    {
        logger.LogInformation("Getting child transactions for parent: {ParentTransactionId}", parentTransactionId);
        var result = await requestTransactionService.GetChildTransactionsAsync(parentTransactionId);
        return result.ToActionResult();
    }

    [HttpGet("root/{requestId}")]
    [EndpointPermission("request-transactions.get-root", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetRootTransaction(Guid requestId)
    {
        logger.LogInformation("Getting root transaction for request: {RequestId}", requestId);
        var result = await requestTransactionService.GetRootTransactionAsync(requestId);
        return result.ToActionResult();
    }

    [HttpGet("{transactionId}/tree")]
    [EndpointPermission("request-transactions.get-tree", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetTransactionTree(Guid transactionId)
    {
        logger.LogInformation("Getting transaction tree for transaction: {TransactionId}", transactionId);
        var result = await requestTransactionService.GetTransactionTreeAsync(transactionId);
        return result.ToActionResult();
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [EndpointPermission("request-transactions.create", SubSystem.TransactionSystem, PermissionType.Insert)]
    public async Task<IActionResult> Create([FromForm] AddInitialRequestTransactionDto dto)
    {
        logger.LogInformation("Creating new request transaction for request: {RequestId}", dto?.RequestId);
        ArgumentNullException.ThrowIfNull(dto);
        var result = await requestTransactionService.AddInitialRequestTransaction(dto);
        return result.ToActionResult();
    }

    [HttpPost("AddTransactionChildren")]
    [Consumes("multipart/form-data")]
    [EndpointPermission("request-transactions.add-child", SubSystem.TransactionSystem, PermissionType.Insert)]
    public async Task<IActionResult> AddChildTransaction([FromForm] CreateRequestTransactionDto dto)
    {
        logger.LogInformation("Adding child transaction to parent: {ParentTransactionId}", dto.ParentTransactionId);
        ArgumentNullException.ThrowIfNull(dto);
        var result = await requestTransactionService.AddChildTransactionAsync(dto);
        return result.ToActionResult();
    }

    [HttpPost("paged")]
    [EndpointPermission("request-transactions.get-paged", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetPaged([FromQuery] TransactionStatus status, [FromBody] RequestPagedFilterDto? filterDto)
    {
        filterDto ??= new RequestPagedFilterDto();
        logger.LogInformation("Getting paged request transactions, page: {Page}, size: {PageSize}, status: {Status}", filterDto.Page, filterDto.PageSize, status);
        var result = await requestTransactionService.GetPagedAsync(filterDto, status);
        return result.ToActionResult();
    }

    [HttpPost("{requestId}/mark-as-managed")]
    [EndpointPermission("request-transactions.mark-as-managed", SubSystem.TransactionSystem, PermissionType.Update)]
    public async Task<IActionResult> MarkAsManaged(Guid requestId)
    {
        logger.LogInformation("Marking request as managed: {RequestId}", requestId);
        var result = await requestTransactionService.MarkAsManagedAsync(requestId);
        return result.ToActionResult();
    }
}
