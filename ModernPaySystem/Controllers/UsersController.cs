namespace ModernPaySystem.Controllers;

/// <summary>
/// API controller for User management
/// Provides CRUD operations for users.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Get all users.
    /// </summary>
    [HttpGet]
    [EndpointPermission("users.get-all", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Getting all users");
        var result = await _userService.GetAllAsync();
        return result.ToActionResult();
    }

    /// <summary>
    /// Get user by id
    /// </summary>
    [HttpGet("{id}")]
    [EndpointPermission("users.get-by-id", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetById(Guid id)
    {
        _logger.LogInformation("Getting user by id: {UserId}", id);
        var result = await _userService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    /// <summary>
    /// Get user by username
    /// </summary>
    [HttpGet("by-username/{username}")]
    [EndpointPermission("users.get-by-username", SubSystem.TransactionSystem, PermissionType.Read)]
    [AllowAnonymous]
    public async Task<IActionResult> GetByUsername(string username)
    {
        _logger.LogInformation("Getting user by username: {Username}", username);
        var result = await _userService.GetByUsernameAsync(username);
        return result.ToActionResult();
    }

    /// <summary>
    /// Check if username exists
    /// </summary>
    [HttpGet("exists/{username}")]
    [EndpointPermission("users.username-exists", SubSystem.TransactionSystem, PermissionType.Read)]
    [AllowAnonymous]
    public async Task<IActionResult> UsernameExists(string username)
    {
        _logger.LogInformation("Checking if username exists: {Username}", username);
        bool exists = await _userService.UsernameExistsAsync(username);
        return Ok(new { exists });
    }

    /// <summary>
    /// Create new user.
    /// </summary>
    [HttpPost]
    [EndpointPermission("users.create", SubSystem.TransactionSystem, PermissionType.Insert)]
    public async Task<IActionResult> Create([FromBody] User user)
    {
        _logger.LogInformation("Creating new user: {Username}", user?.UserName);
        var result = await _userService.CreateAsync(user);
        return result.ToActionResult();
    }

    /// <summary>
    /// Update user.
    /// </summary>
    [HttpPut("{id}")]
    [EndpointPermission("users.update", SubSystem.TransactionSystem, PermissionType.Update)]
    public async Task<IActionResult> Update(Guid id, [FromBody] User user)
    {
        _logger.LogInformation("Updating user: {UserId}", id);
        var result = await _userService.UpdateAsync(id, user);
        return result.ToActionResult();
    }

    /// <summary>
    /// Delete user.
    /// </summary>
    [HttpDelete("{id}")]
    [EndpointPermission("users.delete", SubSystem.TransactionSystem, PermissionType.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogInformation("Deleting user: {UserId}", id);
        var result = await _userService.DeleteAsync(id);
        return result.ToActionResult();
    }
}
