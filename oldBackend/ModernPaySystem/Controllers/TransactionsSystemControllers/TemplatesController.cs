using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernPaySystem.Application.Interfaces;
using ModernPaySystem.Domain.Entities.TransactionSystemEntities;
using ModernPaySystem.Infrastructure.Extensions;

namespace ModernPaySystem.Controllers.TransactionsSystemControllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TemplatesController : ControllerBase
{
    private readonly ITemplateService _templateService;
    private readonly ILogger<TemplatesController> _logger;

    public TemplatesController(ITemplateService templateService, ILogger<TemplatesController> logger)
    {
        _templateService = templateService;
        _logger = logger;
    }

    [HttpGet]
    [EndpointPermission("templates.get-all", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Getting all templates");
        var result = await _templateService.GetAllAsync();
        return result.ToActionResult();
    }

    [HttpGet("{id}")]
    [EndpointPermission("templates.get-by-id", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetById(Guid id)
    {
        _logger.LogInformation("Getting template by id: {TemplateId}", id);
        var result = await _templateService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpGet("by-name/{name}")]
    [EndpointPermission("templates.get-by-name", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetByName(string name)
    {
        _logger.LogInformation("Getting template by name: {Name}", name);
        var result = await _templateService.GetByNameAsync(name);
        return result.ToActionResult();
    }

    [HttpPost]
    [EndpointPermission("templates.create", SubSystem.TransactionSystem, PermissionType.Insert)]
    public async Task<IActionResult> Create([FromBody] CreateTemplateDto template)
    {
        _logger.LogInformation("Creating new template: {TemplateName}", template?.TemplateName);
        var result = await _templateService.CreateAsync(template);
        return result.ToActionResult();
    }

    [HttpPut("{id}")]
    [EndpointPermission("templates.update", SubSystem.TransactionSystem, PermissionType.Update)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTemplateDto template)
    {
        _logger.LogInformation("Updating template: {TemplateId}", id);
        var result = await _templateService.UpdateAsync(id, template);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    [EndpointPermission("templates.delete", SubSystem.TransactionSystem, PermissionType.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogInformation("Deleting template: {TemplateId}", id);
        var result = await _templateService.DeleteAsync(id);
        return result.ToActionResult();
    }

    [HttpGet("{id}/ownerships")]
    [EndpointPermission("templates.ownership.get", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetOwnerships(Guid id)
    {
        _logger.LogInformation("Getting ownerships for template: {TemplateId}", id);
        var result = await _templateService.GetOwnershipsAsync(id);
        return result.ToActionResult();
    }

    [HttpPost("{id}/ownerships")]
    [EndpointPermission("templates.ownership.add", SubSystem.TransactionSystem, PermissionType.Insert)]
    public async Task<IActionResult> AddOwnership(Guid id, [FromBody] CreateTemplateOwnershipDto dto)
    {
        _logger.LogInformation("Adding ownership for template {TemplateId} -> department {DepartmentId}", id, dto?.DepartmentId);
        var result = await _templateService.AddOwnershipAsync(id, dto.DepartmentId);
        return result.ToActionResult();
    }

    [HttpDelete("{id}/ownerships/{departmentId}")]
    [EndpointPermission("templates.ownership.remove", SubSystem.TransactionSystem, PermissionType.Delete)]
    public async Task<IActionResult> RemoveOwnership(Guid id, Guid departmentId)
    {
        _logger.LogInformation("Removing ownership for template {TemplateId} -> department {DepartmentId}", id, departmentId);
        var result = await _templateService.RemoveOwnershipAsync(id, departmentId);
        return result.ToActionResult();
    }

    [HttpGet("{id}/ownerships/user")]
    [EndpointPermission("templates.ownership.user.get", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetUserOwnerships(Guid id)
    {
        _logger.LogInformation("Getting user ownerships for template: {TemplateId}", id);
        var result = await _templateService.GetUserOwnershipsAsync(id);
        return result.ToActionResult();
    }

    [HttpPost("{id}/ownerships/user")]
    [EndpointPermission("templates.ownership.user.add", SubSystem.TransactionSystem, PermissionType.Insert)]
    public async Task<IActionResult> AddUserOwnership(Guid id, [FromBody] CreateUserTemplateOwnershipDto dto)
    {
        _logger.LogInformation("Adding user ownership for template {TemplateId} -> user {UserId}", id, dto?.UserId);
        var result = await _templateService.AddUserOwnershipAsync(id, dto.UserId);
        return result.ToActionResult();
    }

    [HttpDelete("{id}/ownerships/user/{userId}")]
    [EndpointPermission("templates.ownership.user.remove", SubSystem.TransactionSystem, PermissionType.Delete)]
    public async Task<IActionResult> RemoveUserOwnership(Guid id, Guid userId)
    {
        _logger.LogInformation("Removing user ownership for template {TemplateId} -> user {UserId}", id, userId);
        var result = await _templateService.RemoveUserOwnershipAsync(id, userId);
        return result.ToActionResult();
    }

    [HttpGet("department/{departmentId}")]
    [EndpointPermission("templates.department.get", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetByDepartment(Guid departmentId)
    {
        _logger.LogInformation("Getting templates for department: {DepartmentId}", departmentId);
        var result = await _templateService.GetByDepartmentAsync(departmentId);
        return result.ToActionResult();
    }

    [HttpGet("user/{userId}")]
    [EndpointPermission("templates.user.get", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetUserDirect(Guid userId)
    {
        _logger.LogInformation("Getting direct templates for user: {UserId}", userId);
        var result = await _templateService.GetUserDirectAsync(userId);
        return result.ToActionResult();
    }
}
