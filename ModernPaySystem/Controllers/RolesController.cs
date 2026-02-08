using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernPaySystem.Application.Services;
using ModernPaySystem.Domain.Entities.SharedEntities;
using ModernPaySystem.Infrastructure.Extensions;

namespace ModernPaySystem.Controllers;

/// <summary>
/// API controller for Role management
/// Provides CRUD operations for roles
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;
    private readonly ILogger<RolesController> _logger;

    public RolesController(IRoleService roleService, ILogger<RolesController> logger)
    {
        _roleService = roleService;
        _logger = logger;
    }

    /// <summary>
    /// Get all roles
    /// </summary>
    [HttpGet]
    [EndpointPermission("roles.get-all", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Getting all roles");
        var result = await _roleService.GetAllAsync();
        return result.ToActionResult();
    }

    /// <summary>
    /// Get role by id
    /// </summary>
    [HttpGet("{id}")]
    [EndpointPermission("roles.get-by-id", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetById(Guid id)
    {
        _logger.LogInformation("Getting role by id: {RoleId}", id);
        var result = await _roleService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    /// <summary>
    /// Get role by name
    /// </summary>
    [HttpGet("by-name/{name}")]
    [EndpointPermission("roles.get-by-name", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetByName(string name)
    {
        _logger.LogInformation("Getting role by name: {Name}", name);
        var result = await _roleService.GetByNameAsync(name);
        return result.ToActionResult();
    }

    /// <summary>
    /// Create new role
    /// </summary>
    [HttpPost]
    [EndpointPermission("roles.create", SubSystem.TransactionSystem, PermissionType.Insert)]
    public async Task<IActionResult> Create([FromBody] Role role)
    {
        _logger.LogInformation("Creating new role: {RoleName}", role?.Name);
        var result = await _roleService.CreateAsync(role);
        return result.ToActionResult();
    }

    /// <summary>
    /// Update role
    /// </summary>
    [HttpPut("{id}")]
    [EndpointPermission("roles.update", SubSystem.TransactionSystem, PermissionType.Update)]
    public async Task<IActionResult> Update(Guid id, [FromBody] Role role)
    {
        _logger.LogInformation("Updating role: {RoleId}", id);
        var result = await _roleService.UpdateAsync(id, role);
        return result.ToActionResult();
    }

    /// <summary>
    /// Delete role
    /// </summary>
    [HttpDelete("{id}")]
    [EndpointPermission("roles.delete", SubSystem.TransactionSystem, PermissionType.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogInformation("Deleting role: {RoleId}", id);
        var result = await _roleService.DeleteAsync(id);
        return result.ToActionResult();
    }
}
