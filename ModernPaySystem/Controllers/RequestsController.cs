using System.Runtime.Versioning;
using ModernPaySystem.Domain.Entities.TransactionSystemEntities;
using ModernPaySystem.Infrastructure.Services;

namespace ModernPaySystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RequestsController(IRequestService requestService, ILogger<RequestsController> logger) : ControllerBase
{
    [HttpGet]
    [EndpointPermission("requests.get-all", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetAll()
    {
        logger.LogInformation("Getting all requests");
        var result = await requestService.GetAllAsync();
        return result.ToActionResult();
    }

    [HttpGet("GetAllRequestsNeedAction/{hasResponse}")]
    [EndpointPermission("requests.get-all", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetAllRequestsNeedAction(bool hasResponse)
    {
        logger.LogInformation("Getting all request need action requests");
        var result = await requestService.GetAllAsync(hasResponse);
        return result.ToActionResult();
    }

    [HttpGet("{id}")]
    [EndpointPermission("requests.get-by-id", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetById(Guid id)
    {
        logger.LogInformation("Getting request by id: {RequestId}", id);
        var result = await requestService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpGet("by-requester/{requesterId}")]
    [EndpointPermission("requests.get-by-requester-id", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetByRequesterId(Guid requesterId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        logger.LogInformation("Getting paged requests for requester: {RequesterId}, page: {Page}, size: {PageSize}", requesterId, page, pageSize);
        var result = await requestService.GetByRequesterIdAsync(requesterId, page, pageSize);
        return result.ToActionResult();
    }

    [HttpGet("by-approver/{approverId}")]
    [EndpointPermission("requests.get-by-approver-id", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetByApproverId(Guid approverId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        logger.LogInformation("Getting paged requests for approver: {ApproverId}, page: {Page}, size: {PageSize}", approverId, page, pageSize);
        var result = await requestService.GetByApproverIdAsync(approverId, page, pageSize);
        return result.ToActionResult();
    }

    [HttpGet("by-template/{templateId}")]
    [EndpointPermission("requests.get-by-template-id", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetByTemplateId(Guid templateId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        logger.LogInformation("Getting paged requests for template: {TemplateId}, page: {Page}, size: {PageSize}", templateId, page, pageSize);
        var result = await requestService.GetByTemplateIdAsync(templateId, page, pageSize);
        return result.ToActionResult();
    }


    [HttpPost]
    [Consumes("multipart/form-data")]
    [EndpointPermission("requests.create", SubSystem.TransactionSystem, PermissionType.Insert)]
    public async Task<IActionResult> Create([FromForm] CreateRequestDto request)
    {
        var result = await requestService.CreateAsync(request, request.Files);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    [EndpointPermission("requests.delete", SubSystem.TransactionSystem, PermissionType.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        logger.LogInformation("Deleting request: {RequestId}", id);
        var result = await requestService.DeleteAsync(id);
        return result.ToActionResult();
    }

    [HttpGet("paged")]
    [EndpointPermission("requests.get-paged", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        logger.LogInformation("Getting paged requests, page: {Page}, size: {PageSize}", page, pageSize);
        var result = await requestService.GetPagedAsync(page, pageSize);
        return result.ToActionResult();
    }

    [HttpGet("GetPagedRequestsNeedAction/{hasResponse}")]
    [EndpointPermission("requests.get-paged-need-action", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetPagedRequestsNeedAction(bool hasResponse, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        logger.LogInformation("Getting paged requests need action, hasResponse: {HasResponse}, page: {Page}, size: {PageSize}", hasResponse, page, pageSize);
        var result = await requestService.GetAllRequestNeedActionPagedAsync(page, pageSize, hasResponse);
        return result.ToActionResult();
    }
}
