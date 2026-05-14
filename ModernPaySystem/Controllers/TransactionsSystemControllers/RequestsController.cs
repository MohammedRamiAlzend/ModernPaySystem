using ModernPaySystem.Domain.DTOs;
using ModernPaySystem.Domain.Entities.TransactionSystemEntities;
using ModernPaySystem.Infrastructure.Services;
using System.Runtime.Versioning;

namespace ModernPaySystem.Controllers.TransactionsSystemControllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RequestsController(IRequestService requestService, ILogger<RequestsController> logger) : ControllerBase
{
    [HttpGet("{id}")]
    [EndpointPermission("requests.get-by-id", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetById(Guid id)
    {
        logger.LogInformation("Getting request by id: {RequestId}", id);
        var result = await requestService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpPost("by-requester/{requesterId}")]
    [EndpointPermission("requests.get-by-requester-id", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetByRequesterId(Guid requesterId, [FromBody] RequestPagedFilterDto filterDto)
    {
        logger.LogInformation("Getting paged requests for requester: {RequesterId}, page: {Page}, size: {PageSize}", requesterId, filterDto.Page, filterDto.PageSize);
        var result = await requestService.GetByRequesterIdAsync(requesterId, filterDto);
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
        var result = await requestService.CreateAsync(request, request.Files!);
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

    [HttpPost("paged")]
    [EndpointPermission("requests.get-paged", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetPaged(RequestPagedFilterDto filterDto)
    {
        logger.LogInformation("Getting paged requests, page: {Page}, size: {PageSize}", filterDto.Page, filterDto.PageSize);
        var result = await requestService.GetPagedAsync(filterDto);
        return result.ToActionResult();
    }

    [HttpPost("GetPagedRequestsNeedAction/{hasResponse}")]
    [EndpointPermission("requests.get-paged-need-action", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetPagedRequestsNeedAction(bool hasResponse, [FromBody] RequestPagedFilterDto? filterDto)
    {
        logger.LogInformation("Getting paged requests need action, hasResponse: {HasResponse}, page: {Page}, size: {PageSize}", hasResponse, filterDto.Page, filterDto.PageSize);
        var result = await requestService.GetAllRequestNeedActionPagedAsync(filterDto, hasResponse);
        return result.ToActionResult();
    }

    [HttpGet("my-pending/paged")]
    [EndpointPermission("requests.get-my-pending-paged", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetMyPendingPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        logger.LogInformation("Getting paged pending requests for current requester, page: {Page}, size: {PageSize}", page, pageSize);
        var result = await requestService.GetPendingByCurrentRequesterPagedAsync(page, pageSize);
        return result.ToActionResult();
    }
}
