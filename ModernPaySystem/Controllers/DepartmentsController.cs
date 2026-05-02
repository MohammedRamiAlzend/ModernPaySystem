using ModernPaySystem.Application.Interfaces;
using ModernPaySystem.Domain.DTOs;

namespace ModernPaySystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentsController(IDepartmentService departmentService, ILogger<DepartmentsController> logger) : ControllerBase
{
    /// <summary>
    /// Get the full department tree
    /// </summary>
    [HttpGet("tree")]
    [EndpointPermission("departments.view_tree", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetTree()
    {
        logger.LogInformation("Fetching full department tree");
        var result = await departmentService.GetTreeAsync();
        return result.ToActionResult();
    }

    /// <summary>
    /// Get a subtree starting from a specific department
    /// </summary>
    [HttpGet("{id:guid}/subtree")]
    [EndpointPermission("departments.view_tree", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetSubTree(Guid id)
    {
        logger.LogInformation("Fetching subtree for department: {DepartmentId}", id);
        var result = await departmentService.GetSubTreeAsync(id);
        return result.ToActionResult();
    }

    /// <summary>
    /// Get department by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [EndpointPermission("departments.view", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetById(Guid id)
    {
        logger.LogInformation("Fetching department by id: {DepartmentId}", id);
        var result = await departmentService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    /// <summary>
    /// Get direct children of a department
    /// </summary>
    [HttpGet("{id:guid}/children")]
    [EndpointPermission("departments.view", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetChildren(Guid id)
    {
        logger.LogInformation("Fetching children for department: {DepartmentId}", id);
        var result = await departmentService.GetChildrenAsync(id);
        return result.ToActionResult();
    }

    /// <summary>
    /// Get the path from a department to root
    /// </summary>
    [HttpGet("{id:guid}/path")]
    [EndpointPermission("departments.view", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetPathToRoot(Guid id)
    {
        logger.LogInformation("Fetching path to root for department: {DepartmentId}", id);
        var result = await departmentService.GetPathToRootAsync(id);
        return result.ToActionResult();
    }

    /// <summary>
    /// Get parent department
    /// </summary>
    [HttpGet("{id:guid}/parent")]
    [EndpointPermission("departments.view", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetParent(Guid id)
    {
        logger.LogInformation("Fetching parent for department: {DepartmentId}", id);
        var result = await departmentService.GetParentAsync(id);
        return result.ToActionResult();
    }

    /// <summary>
    /// Search departments by name or code
    /// </summary>
    [HttpGet("search")]
    [EndpointPermission("departments.view", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> Search([FromQuery] string searchTerm, [FromQuery] int level = 0)
    {
        logger.LogInformation("Searching departments with term: {SearchTerm}, level: {Level}", searchTerm, level);
        var result = await departmentService.SearchAsync(searchTerm, level);
        return result.ToActionResult();
    }

    /// <summary>
    /// Get departments by level
    /// </summary>
    [HttpGet("level/{level:int}")]
    [EndpointPermission("departments.view", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetByLevel(int level)
    {
        logger.LogInformation("Fetching departments at level: {Level}", level);
        var result = await departmentService.GetByLevelAsync(level);
        return result.ToActionResult();
    }

    /// <summary>
    /// Create a new department
    /// </summary>
    [HttpPost]
    [EndpointPermission("departments.create", SubSystem.TransactionSystem, PermissionType.Insert)]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentDto dto)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            logger.LogWarning("Create department attempt without valid user ID");
            return Unauthorized();
        }

        logger.LogInformation("Creating new department: {DepartmentName}", dto.Name);
        var result = await departmentService.CreateAsync(dto, userId);
        
        if (!result.IsError && result.Value != null)
        {
            return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
        }

        return result.ToActionResult();
    }

    /// <summary>
    /// Update an existing department
    /// </summary>
    [HttpPut("{id:guid}")]
    [EndpointPermission("departments.edit", SubSystem.TransactionSystem, PermissionType.Update)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDepartmentDto dto)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            logger.LogWarning("Update department attempt without valid user ID");
            return Unauthorized();
        }

        logger.LogInformation("Updating department: {DepartmentId}", id);
        var result = await departmentService.UpdateAsync(id, dto, userId);
        return result.ToActionResult();
    }

    /// <summary>
    /// Delete a department
    /// </summary>
    [HttpDelete("{id:guid}")]
    [EndpointPermission("departments.delete", SubSystem.TransactionSystem, PermissionType.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        logger.LogInformation("Deleting department: {DepartmentId}", id);
        var result = await departmentService.DeleteAsync(id);
        return result.ToActionResult();
    }

    /// <summary>
    /// Get users in a department
    /// </summary>
    [HttpGet("{id:guid}/users")]
    [EndpointPermission("departments.view", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetUsersInDepartment(Guid id, [FromQuery] bool includeSubDepartments = false)
    {
        logger.LogInformation("Fetching users in department: {DepartmentId}", id);
        var result = await departmentService.GetUsersInDepartmentAsync(id, includeSubDepartments);
        return result.ToActionResult();
    }

    /// <summary>
    /// Assign a user to a department
    /// </summary>
    [HttpPost("{id:guid}/assign-user")]
    [EndpointPermission("departments.assign_user", SubSystem.TransactionSystem, PermissionType.Update)]
    public async Task<IActionResult> AssignUser(Guid id, [FromBody] AssignUserDto dto)
    {
        logger.LogInformation("Assigning user: {UserId} to department: {DepartmentId}", dto.UserId, id);
        var result = await departmentService.AssignUserToDepartmentAsync(dto.UserId, id);
        return result.ToActionResult();
    }

    /// <summary>
    /// Remove a user from a department
    /// </summary>
    [HttpDelete("{id:guid}/remove-user/{userId:guid}")]
    [EndpointPermission("departments.assign_user", SubSystem.TransactionSystem, PermissionType.Update)]
    public async Task<IActionResult> RemoveUser(Guid id, Guid userId)
    {
        logger.LogInformation("Removing user: {UserId} from department: {DepartmentId}", userId, id);
        var result = await departmentService.RemoveUserFromDepartmentAsync(userId);
        return result.ToActionResult();
    }
}

/// <summary>
/// DTO for assigning a user to a department
/// </summary>
public class AssignUserDto
{
    public Guid UserId { get; set; }
}
