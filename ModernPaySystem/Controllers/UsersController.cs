namespace ModernPaySystem.Controllers;

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

    [HttpGet]
    [EndpointPermission("users.get-all", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Getting all users");
        var result = await _userService.GetAllAsync();
        return result.ToActionResult();
    }

    [HttpGet("{id}")]
    [EndpointPermission("users.get-by-id", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetById(Guid id)
    {
        _logger.LogInformation("Getting user by id: {UserId}", id);
        var result = await _userService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpGet("by-username/{username}")]
    [EndpointPermission("users.get-by-username", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetByUsername(string username)
    {
        _logger.LogInformation("Getting user by username: {Username}", username);
        var result = await _userService.GetByUsernameAsync(username);
        return result.ToActionResult();
    }

    [HttpPost]
    [EndpointPermission("users.create", SubSystem.TransactionSystem, PermissionType.Insert)]
    public async Task<IActionResult> Create([FromBody] CreateUserDto user)
    {
        ArgumentNullException.ThrowIfNull(user);
        _logger.LogInformation("Creating new user: {Username}", user?.UserName);
        var result = await _userService.CreateAsync(user);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    [EndpointPermission("users.delete", SubSystem.TransactionSystem, PermissionType.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogInformation("Deleting user: {UserId}", id);
        var result = await _userService.DeleteAsync(id);
        return result.ToActionResult();
    }
    [HttpGet("subsystems")]
    public async Task<IActionResult> GetSubSystems()
    {
        _logger.LogInformation("Getting all subsystems");
        var result = await _userService.GetSubSystemsAsync();
        return result.ToActionResult();
    }

    [HttpGet("by-subsystem/{subSystemId:int}")]
    [EndpointPermission("users.get-by-subsystem", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetBySubSystem([FromRoute] int subSystemId)
    {
        _logger.LogInformation("Getting users by subsystem ID: {SubSystemId}", subSystemId);

        if (!Enum.IsDefined(typeof(SubSystem), subSystemId))
        {
            return BadRequest($"Invalid subsystem ID: {subSystemId}");
        }

        var subSystem = (SubSystem)subSystemId;
        var result = await _userService.GetBySubSystemAsync(subSystem);
        return result.ToActionResult();
    }
}
