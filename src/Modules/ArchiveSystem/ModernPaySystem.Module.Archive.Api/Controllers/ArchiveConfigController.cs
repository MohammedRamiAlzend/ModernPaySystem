using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernPaySystem.Module.Archive.Api.Extensions;
using ModernPaySystem.Module.Archive.Application.Interfaces;
using ModernPaySystem.Module.Archive.Domain.Entities;
using ModernPaySystem.SharedKernel.Application.Services;
using ModernPaySystem.SharedKernel.Domain.Attrs;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Archive.Api.Controllers;

[ApiController]
[Route("api/archive/config")]
[Authorize]
public class ArchiveConfigController(
    IArchiveConfigService archiveConfigService,
    IArchiveAuthorizationService archiveAuthorizationService,
    IHttpContextServiceManager httpContextServiceManager,
    ILogger<ArchiveConfigController> logger) : ControllerBase
{
    [HttpGet]
    [EndpointPermission("archiving.config.get", SubSystem.Archiving, PermissionType.Read)]
    public async Task<IActionResult> Get()
    {
        logger.LogInformation("Fetching archive configuration");
        var result = await archiveConfigService.GetAsync();
        return result.ToActionResult();
    }

    [HttpPut]
    [EndpointPermission("archiving.config.update", SubSystem.Archiving, PermissionType.Update)]
    public async Task<IActionResult> Update([FromBody] UpdateArchiveConfigDto dto)
    {
        logger.LogInformation("Updating archive configuration");

        var userId = httpContextServiceManager.GetCurrentUserId();
        var leaderDepartmentsResult = await archiveAuthorizationService.GetUserArchiveLeaderDepartmentsAsync(userId);
        if (leaderDepartmentsResult.IsError)
            return leaderDepartmentsResult.ToActionResult();

        var isArchiveLeader = leaderDepartmentsResult.Value?.Count > 0;
        if (!isArchiveLeader)
            return Forbid();

        var result = await archiveConfigService.UpdateAsync(dto);
        return result.ToActionResult();
    }
}
