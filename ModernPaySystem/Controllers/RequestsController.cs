using System.Runtime.Versioning;
using ModernPaySystem.Domain.Entities.TransactionSystemEntities;

namespace ModernPaySystem.Controllers;

/// <summary>
/// API controller for Request management
/// Provides CRUD operations and request-specific queries.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RequestsController(IRequestService requestService, ILogger<RequestsController> logger) : ControllerBase
{
    /// <summary>
    /// Get all requests.
    /// </summary>
    [HttpGet]
    [EndpointPermission("requests.get-all", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetAll()
    {
        logger.LogInformation("Getting all requests");
        var result = await requestService.GetAllAsync();
        return result.ToActionResult();
    }


    /// <summary>
    /// Get all Requests need Action.
    /// </summary>
    [HttpGet("GetAllRequestsNeedAction")]
    [EndpointPermission("requests.get-all", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetAllRequestsNeedAction(bool hasResponse)
    {
        logger.LogInformation("Getting all request need action requests");
        var result = await requestService.GetAllAsync(hasResponse);
        return result.ToActionResult();
    }

    /// <summary>
    /// Get request by id.
    /// </summary>
    [HttpGet("{id}")]
    [EndpointPermission("requests.get-by-id", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetById(Guid id)
    {
        logger.LogInformation("Getting request by id: {RequestId}", id);
        var result = await requestService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    /// <summary>
    /// Get requests by requester id.
    /// </summary>
    [HttpGet("by-requester/{requesterId}")]
    [EndpointPermission("requests.get-by-requester-id", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetByRequesterId(Guid requesterId)
    {
        logger.LogInformation("Getting requests for requester: {RequesterId}", requesterId);
        var result = await requestService.GetByRequesterIdAsync(requesterId);
        return result.ToActionResult();
    }

    /// <summary>
    /// Get requests by approver id.
    /// </summary>
    [HttpGet("by-approver/{approverId}")]
    [EndpointPermission("requests.get-by-approver-id", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetByApproverId(Guid approverId)
    {
        logger.LogInformation("Getting requests for approver: {ApproverId}", approverId);
        var result = await requestService.GetByApproverIdAsync(approverId);
        return result.ToActionResult();
    }

    /// <summary>
    /// Get requests by template id.
    /// </summary>
    [HttpGet("by-template/{templateId}")]
    [EndpointPermission("requests.get-by-template-id", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetByTemplateId(Guid templateId)
    {
        logger.LogInformation("Getting requests for template: {TemplateId}", templateId);
        var result = await requestService.GetByTemplateIdAsync(templateId);
        return result.ToActionResult();
    }

    /// <summary>
    /// Create new request.
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [EndpointPermission("requests.create", SubSystem.TransactionSystem, PermissionType.Insert)]
    public async Task<IActionResult> Create([FromForm] CreateRequestDto request)
    {
        logger.LogInformation("Creating new request for requester: {RequesterId}", request?.RequesterId);
        var result = await requestService.CreateAsync(request, request.Files);
        return result.ToActionResult();
    }

    /// <summary>
    /// Update request.
    /// </summary>
    [HttpPut("{id}")]
    [EndpointPermission("requests.update", SubSystem.TransactionSystem, PermissionType.Update)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRequestDto request)
    {
        logger.LogInformation("Updating request: {RequestId}", id);
        var result = await requestService.UpdateAsync(id, request);
        return result.ToActionResult();
    }

    /// <summary>
    /// Delete request.
    /// </summary>
    [HttpDelete("{id}")]
    [EndpointPermission("requests.delete", SubSystem.TransactionSystem, PermissionType.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        logger.LogInformation("Deleting request: {RequestId}", id);
        var result = await requestService.DeleteAsync(id);
        return result.ToActionResult();
    }
}
