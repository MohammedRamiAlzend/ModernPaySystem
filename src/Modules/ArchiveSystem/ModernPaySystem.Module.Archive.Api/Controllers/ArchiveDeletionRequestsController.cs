using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernPaySystem.Module.Archive.Api.Extensions;
using ModernPaySystem.Module.Archive.Application.Interfaces;
using ModernPaySystem.Module.Archive.Domain.Entities;
using ModernPaySystem.SharedKernel.Domain.Attrs;
using ModernPaySystem.SharedKernel.Domain.Entities;
using ModernPaySystem.Module.Archive.Infrastructure.Auth;

namespace ModernPaySystem.Module.Archive.Api.Controllers;

[ApiController]
[Route("api/ArchiveSystem/[controller]")]
[Authorize]
public class ArchiveDeletionRequestsController(
    IArchiveDeletionWorkflowService workflowService,
    IAuthorizationService authorizationService,
    ILogger<ArchiveDeletionRequestsController> logger) : ControllerBase
{
    [HttpPost]
    [EndpointPermission("archiving.delete-requests.create", SubSystem.Archiving, PermissionType.Insert)]
    public async Task<IActionResult> Submit([FromBody] CreateDeleteArchiveRequestDto dto)
    {
        var result = await workflowService.SubmitRequestAsync(dto);
        return result.ToActionResult();
    }

    [HttpGet("{requestId:guid}")]
    [EndpointPermission("archiving.delete-requests.view", SubSystem.Archiving, PermissionType.Read)]
    public async Task<IActionResult> GetById(Guid requestId)
    {
        var result = await workflowService.GetByIdAsync(requestId);
        return result.ToActionResult();
    }

    [HttpGet("department/{departmentId:guid}")]
    [EndpointPermission("archiving.delete-requests.view", SubSystem.Archiving, PermissionType.Read)]
    public async Task<IActionResult> GetPendingForDepartment(Guid departmentId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var auth = await authorizationService.AuthorizeAsync(User, new ArchiveDepartmentScope(departmentId), ArchiveAuthorizationPolicyExtensions.RequireDepartmentHead);
        if (!auth.Succeeded)
        {
            return Forbid();
        }

        var result = await workflowService.GetPendingForDepartmentAsync(departmentId, page, pageSize);
        return result.ToActionResult();
    }

    [HttpPost("{requestId:guid}/approve")]
    [EndpointPermission("archiving.delete-requests.approve", SubSystem.Archiving, PermissionType.Update)]
    public async Task<IActionResult> Approve(Guid requestId, [FromBody] DeleteArchiveRequestDecisionDto dto)
    {
        var request = await workflowService.GetByIdAsync(requestId);
        if (request.IsError)
        {
            return request.ToActionResult();
        }

        var auth = await authorizationService.AuthorizeAsync(User, new ArchiveDepartmentScope(request.Value!.DepartmentId), ArchiveAuthorizationPolicyExtensions.RequireDepartmentHead);
        if (!auth.Succeeded)
        {
            return Forbid();
        }

        var result = await workflowService.ApproveAsync(requestId, dto.Notes);
        return result.ToActionResult();
    }

    [HttpPost("{requestId:guid}/reject")]
    [EndpointPermission("archiving.delete-requests.reject", SubSystem.Archiving, PermissionType.Update)]
    public async Task<IActionResult> Reject(Guid requestId, [FromBody] DeleteArchiveRequestRejectDto dto)
    {
        var request = await workflowService.GetByIdAsync(requestId);
        if (request.IsError)
        {
            return request.ToActionResult();
        }

        var auth = await authorizationService.AuthorizeAsync(User, new ArchiveDepartmentScope(request.Value!.DepartmentId), ArchiveAuthorizationPolicyExtensions.RequireDepartmentHead);
        if (!auth.Succeeded)
        {
            return Forbid();
        }

        var result = await workflowService.RejectAsync(requestId, dto.Reason);
        return result.ToActionResult();
    }
}
