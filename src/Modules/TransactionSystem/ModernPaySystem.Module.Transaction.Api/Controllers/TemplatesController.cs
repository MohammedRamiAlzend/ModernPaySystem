using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernPaySystem.Module.Transaction.Api.Extensions;
using ModernPaySystem.Module.Transaction.Application.Interfaces;
using ModernPaySystem.Module.Transaction.Domain.Entities;
using ModernPaySystem.SharedKernel.Domain.Attrs;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Transaction.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TemplatesController(ITemplateService templateService, ILogger<TemplatesController> logger) : ControllerBase
{
    [HttpGet]
    [EndpointPermission("templates.get-all", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetAll()
    {
        logger.LogInformation("Getting all templates");
        var result = await templateService.GetAllAsync();
        return result.ToActionResult();
    }

    [HttpGet("{id}")]
    [EndpointPermission("templates.get-by-id", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetById(Guid id)
    {
        logger.LogInformation("Getting template by id: {TemplateId}", id);
        var result = await templateService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpGet("by-name/{name}")]
    [EndpointPermission("templates.get-by-name", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetByName(string name)
    {
        logger.LogInformation("Getting template by name: {Name}", name);
        var result = await templateService.GetByNameAsync(name);
        return result.ToActionResult();
    }

    [HttpPost]
    [EndpointPermission("templates.create", SubSystem.TransactionSystem, PermissionType.Insert)]
    public async Task<IActionResult> Create([FromBody] CreateTemplateDto template)
    {
        logger.LogInformation("Creating new template: {TemplateName}", template?.TemplateName);
        var result = await templateService.CreateAsync(template);
        return result.ToActionResult();
    }

    [HttpPut("{id}")]
    [EndpointPermission("templates.update", SubSystem.TransactionSystem, PermissionType.Update)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTemplateDto template)
    {
        logger.LogInformation("Updating template: {TemplateId}", id);
        var result = await templateService.UpdateAsync(id, template);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    [EndpointPermission("templates.delete", SubSystem.TransactionSystem, PermissionType.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        logger.LogInformation("Deleting template: {TemplateId}", id);
        var result = await templateService.DeleteAsync(id);
        return result.ToActionResult();
    }

    [HttpGet("{id}/ownerships")]
    [EndpointPermission("templates.ownership.get", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetOwnerships(Guid id)
    {
        logger.LogInformation("Getting ownerships for template: {TemplateId}", id);
        var result = await templateService.GetOwnershipsAsync(id);
        return result.ToActionResult();
    }

    [HttpPost("{id}/ownerships")]
    [EndpointPermission("templates.ownership.add", SubSystem.TransactionSystem, PermissionType.Insert)]
    public async Task<IActionResult> AddOwnership(Guid id, [FromBody] CreateTemplateOwnershipDto dto)
    {
        logger.LogInformation("Adding ownership for template {TemplateId} -> department {DepartmentId}", id, dto?.DepartmentId);
        var result = await templateService.AddOwnershipAsync(id, dto.DepartmentId);
        return result.ToActionResult();
    }

    [HttpDelete("{id}/ownerships/{departmentId}")]
    [EndpointPermission("templates.ownership.remove", SubSystem.TransactionSystem, PermissionType.Delete)]
    public async Task<IActionResult> RemoveOwnership(Guid id, Guid departmentId)
    {
        logger.LogInformation("Removing ownership for template {TemplateId} -> department {DepartmentId}", id, departmentId);
        var result = await templateService.RemoveOwnershipAsync(id, departmentId);
        return result.ToActionResult();
    }

    [HttpGet("{id}/ownerships/user")]
    [EndpointPermission("templates.ownership.user.get", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetUserOwnerships(Guid id)
    {
        logger.LogInformation("Getting user ownerships for template: {TemplateId}", id);
        var result = await templateService.GetUserOwnershipsAsync(id);
        return result.ToActionResult();
    }

    [HttpPost("{id}/ownerships/user")]
    [EndpointPermission("templates.ownership.user.add", SubSystem.TransactionSystem, PermissionType.Insert)]
    public async Task<IActionResult> AddUserOwnership(Guid id, [FromBody] CreateUserTemplateOwnershipDto dto)
    {
        logger.LogInformation("Adding user ownership for template {TemplateId} -> user {UserId}", id, dto?.UserId);
        var result = await templateService.AddUserOwnershipAsync(id, dto.UserId);
        return result.ToActionResult();
    }

    [HttpDelete("{id}/ownerships/user/{userId}")]
    [EndpointPermission("templates.ownership.user.remove", SubSystem.TransactionSystem, PermissionType.Delete)]
    public async Task<IActionResult> RemoveUserOwnership(Guid id, Guid userId)
    {
        logger.LogInformation("Removing user ownership for template {TemplateId} -> user {UserId}", id, userId);
        var result = await templateService.RemoveUserOwnershipAsync(id, userId);
        return result.ToActionResult();
    }

    [HttpGet("department/{departmentId}")]
    [EndpointPermission("templates.department.get", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetByDepartment(Guid departmentId)
    {
        logger.LogInformation("Getting templates for department: {DepartmentId}", departmentId);
        var result = await templateService.GetByDepartmentAsync(departmentId);
        return result.ToActionResult();
    }

    [HttpGet("user/{userId}")]
    [EndpointPermission("templates.user.get", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetUserDirect(Guid userId)
    {
        logger.LogInformation("Getting direct templates for user: {UserId}", userId);
        var result = await templateService.GetUserDirectAsync(userId);
        return result.ToActionResult();
    }
}
