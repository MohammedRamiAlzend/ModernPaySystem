namespace ModernPaySystem.Module.Identity.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentsController(
    IDepartmentService departmentService,
    ILogger<DepartmentsController> logger) : ControllerBase
{
    [HttpGet("tree")]
    [EndpointPermission("departments.view_tree", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetTree()
    {
        logger.LogInformation("Fetching full department tree");
        var result = await departmentService.GetTreeAsync();
        return result.ToActionResult();
    }

    [HttpGet("{id:guid}/subtree")]
    [EndpointPermission("departments.view_tree", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetSubTree(Guid id)
    {
        logger.LogInformation("Fetching subtree for department: {DepartmentId}", id);
        var result = await departmentService.GetSubTreeAsync(id);
        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    [EndpointPermission("departments.view", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetById(Guid id)
    {
        logger.LogInformation("Fetching department by id: {DepartmentId}", id);
        var result = await departmentService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpGet("{id:guid}/children")]
    [EndpointPermission("departments.view", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetChildren(Guid id)
    {
        logger.LogInformation("Fetching children for department: {DepartmentId}", id);
        var result = await departmentService.GetChildrenAsync(id);
        return result.ToActionResult();
    }

    [HttpGet("{id:guid}/path")]
    [EndpointPermission("departments.view", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetPathToRoot(Guid id)
    {
        logger.LogInformation("Fetching path to root for department: {DepartmentId}", id);
        var result = await departmentService.GetPathToRootAsync(id);
        return result.ToActionResult();
    }

    [HttpGet("{id:guid}/parent")]
    [EndpointPermission("departments.view", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetParent(Guid id)
    {
        logger.LogInformation("Fetching parent for department: {DepartmentId}", id);
        var result = await departmentService.GetParentAsync(id);
        return result.ToActionResult();
    }

    [HttpGet("search")]
    [EndpointPermission("departments.view", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> Search([FromQuery] string? searchTerm = null, [FromQuery] int level = 0)
    {
        logger.LogInformation("Searching departments with term: {SearchTerm}, level: {Level}", searchTerm, level);
        var result = await departmentService.SearchAsync(searchTerm, level);
        return result.ToActionResult();
    }

    [HttpGet("level/{level:int}")]
    [EndpointPermission("departments.view", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetByLevel(int level)
    {
        logger.LogInformation("Fetching departments at level: {Level}", level);
        var result = await departmentService.GetByLevelAsync(level);
        return result.ToActionResult();
    }

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
            return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);

        return result.ToActionResult();
    }

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

    [HttpDelete("{id:guid}")]
    [EndpointPermission("departments.delete", SubSystem.TransactionSystem, PermissionType.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        logger.LogInformation("Deleting department: {DepartmentId}", id);
        var result = await departmentService.DeleteAsync(id);
        return result.ToActionResult();
    }

    [HttpGet("{id:guid}/users")]
    [EndpointPermission("departments.view", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetUsersInDepartment(Guid id, [FromQuery] bool includeSubDepartments = false)
    {
        logger.LogInformation("Fetching users in department: {DepartmentId}", id);
        var result = await departmentService.GetUsersInDepartmentAsync(id, includeSubDepartments);
        return result.ToActionResult();
    }
}
