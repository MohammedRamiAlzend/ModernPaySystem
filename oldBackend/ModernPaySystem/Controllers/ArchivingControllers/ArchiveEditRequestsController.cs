using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernPaySystem.Application.Interfaces;
using ModernPaySystem.Domain.Entities.Archiving;
using ModernPaySystem.Infrastructure.Auth;
using ModernPaySystem.Infrastructure.Extensions;

namespace ModernPaySystem.Controllers.ArchivingControllers;

[ApiController]
[Route("api/archive-edit-requests")]
[Authorize]
public class ArchiveEditRequestsController(
    IArchiveEditWorkflowService workflowService,
    IAuthorizationService authorizationService,
    ILogger<ArchiveEditRequestsController> logger) : ControllerBase
{
    [HttpPost]
    [Consumes("multipart/form-data")]
    [DisableRequestSizeLimit]
    [EndpointPermission("archiving.edit-requests.create", SubSystem.Archiving, PermissionType.Insert)]
    public async Task<IActionResult> Submit([FromForm] CreateEditArchiveRequestDto dto)
    {
        logger.LogInformation("Submitting edit archive request for record: {RecordId}", dto.ArchiveRecordId);
        var result = await workflowService.SubmitRequestAsync(dto);
        return result.ToActionResult();
    }

    [HttpGet("{requestId:guid}")]
    [EndpointPermission("archiving.edit-requests.view", SubSystem.Archiving, PermissionType.Read)]
    public async Task<IActionResult> GetById(Guid requestId)
    {
        logger.LogInformation("Getting edit archive request by id: {RequestId}", requestId);
        var result = await workflowService.GetByIdAsync(requestId);
        return result.ToActionResult();
    }

    [HttpGet("department/{departmentId:guid}")]
    [EndpointPermission("archiving.edit-requests.view", SubSystem.Archiving, PermissionType.Read)]
    public async Task<IActionResult> GetPendingForDepartment(Guid departmentId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        logger.LogInformation("Getting pending edit archive requests for department: {DepartmentId}", departmentId);
        var auth = await authorizationService.AuthorizeAsync(User, new ArchiveDepartmentScope(departmentId), ArchiveAuthorizationPolicyExtensions.RequireDepartmentArchiveLeader);
        if (!auth.Succeeded)
        {
            return Forbid();
        }

        var result = await workflowService.GetPendingForDepartmentAsync(departmentId, page, pageSize);
        return result.ToActionResult();
    }

    [HttpGet("my-requests")]
    [EndpointPermission("archiving.edit-requests.view", SubSystem.Archiving, PermissionType.Read)]
    public async Task<IActionResult> GetMyRequests([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (claim is null || !Guid.TryParse(claim, out var userId))
        {
            return Unauthorized();
        }

        logger.LogInformation("Getting edit archive requests for user: {UserId}", userId);
        var result = await workflowService.GetMyRequestsAsync(userId, page, pageSize);
        return result.ToActionResult();
    }

    [HttpPost("{requestId:guid}/approve")]
    [EndpointPermission("archiving.edit-requests.approve", SubSystem.Archiving, PermissionType.Update)]
    public async Task<IActionResult> Approve(Guid requestId, [FromBody] EditArchiveRequestDecisionDto dto)
    {
        logger.LogInformation("Approving edit archive request: {RequestId}", requestId);
        var request = await workflowService.GetByIdAsync(requestId);
        if (request.IsError)
        {
            return request.ToActionResult();
        }

        var auth = await authorizationService.AuthorizeAsync(User, new ArchiveDepartmentScope(request.Value!.DepartmentId), ArchiveAuthorizationPolicyExtensions.RequireDepartmentArchiveLeader);
        if (!auth.Succeeded)
        {
            return Forbid();
        }

        var result = await workflowService.ApproveAsync(requestId, dto.Notes);
        return result.ToActionResult();
    }

    [HttpPost("{requestId:guid}/reject")]
    [EndpointPermission("archiving.edit-requests.reject", SubSystem.Archiving, PermissionType.Update)]
    public async Task<IActionResult> Reject(Guid requestId, [FromBody] EditArchiveRequestRejectDto dto)
    {
        logger.LogInformation("Rejecting edit archive request: {RequestId}", requestId);
        var request = await workflowService.GetByIdAsync(requestId);
        if (request.IsError)
        {
            return request.ToActionResult();
        }

        var auth = await authorizationService.AuthorizeAsync(User, new ArchiveDepartmentScope(request.Value!.DepartmentId), ArchiveAuthorizationPolicyExtensions.RequireDepartmentArchiveLeader);
        if (!auth.Succeeded)
        {
            return Forbid();
        }

        var result = await workflowService.RejectAsync(requestId, dto.Reason);
        return result.ToActionResult();
    }
}
