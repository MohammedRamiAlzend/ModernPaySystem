using ModernPaySystem.Domain.Entities.TransactionSystemEntities;

namespace ModernPaySystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ResponsesController(IResponseService responseService, ILogger<ResponsesController> logger) : ControllerBase
{
    [HttpGet]
    [EndpointPermission("responses.get-all", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetAll()
    {
        logger.LogInformation("Getting all responses");
        var result = await responseService.GetAllAsync();
        return result.ToActionResult();
    }

    [HttpGet("{id}")]
    [EndpointPermission("responses.get-by-id", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetById(Guid id)
    {
        logger.LogInformation("Getting response by id: {ResponseId}", id);
        var result = await responseService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpGet("by-request/{requestId}")]
    [EndpointPermission("responses.get-by-request-id", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetByRequestId(Guid requestId)
    {
        logger.LogInformation("Getting responses for request: {RequestId}", requestId);
        var result = await responseService.GetByRequestIdAsync(requestId);
        return result.ToActionResult();
    }

    [HttpGet("by-responder/{responderId}")]
    [EndpointPermission("responses.get-by-responder-id", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetByResponderId(Guid responderId)
    {
        logger.LogInformation("Getting responses for responder: {ResponderId}", responderId);
        var result = await responseService.GetByResponderIdAsync(responderId);
        return result.ToActionResult();
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [EndpointPermission("responses.create", SubSystem.TransactionSystem, PermissionType.Insert)]
    public async Task<IActionResult> Create([FromForm] CreateResponseDto response)
    {
        logger.LogInformation("Creating new response for request: {RequestId}", response?.RequestId);
        ArgumentNullException.ThrowIfNull(response);
        var result = await responseService.CreateAsync(response);
        return result.ToActionResult();
    }

    [HttpPut("{id}")]
    [EndpointPermission("responses.update", SubSystem.TransactionSystem, PermissionType.Update)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateResponseDto response)
    {
        logger.LogInformation("Updating response: {ResponseId}", id);
        var result = await responseService.UpdateAsync(id, response);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    [EndpointPermission("responses.delete", SubSystem.TransactionSystem, PermissionType.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        logger.LogInformation("Deleting response: {ResponseId}", id);
        var result = await responseService.DeleteAsync(id);
        return result.ToActionResult();
    }

    [HttpPost("AddFilesToResponse")]
    [Consumes("multipart/form-data")]
    [EndpointPermission("responses.add-Files", SubSystem.TransactionSystem, PermissionType.Insert)]
    public async Task<IActionResult> AddFilesToResponse([FromForm] Guid responseId, [FromForm] List<IFormFile> files)
    {
        logger.LogInformation("Adding {FileCount} Files to response: {ResponseId}", files?.Count, responseId);
        ArgumentNullException.ThrowIfNull(files);
        var result = await responseService.AddFilesToResponseAsync(responseId, files);
        return result.ToActionResult();
    }

    [HttpGet("paged")]
    [EndpointPermission("responses.get-paged", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        logger.LogInformation("Getting paged responses, page: {Page}, size: {PageSize}", page, pageSize);
        var result = await responseService.GetPagedAsync(page, pageSize);
        return result.ToActionResult();
    }
}
