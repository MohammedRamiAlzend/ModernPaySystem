namespace ModernPaySystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthenticationService authService) : ControllerBase
{
    [HttpPost("login")]
    [EndpointPermission("auth.login", SubSystem.None, PermissionType.Read)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await authService.AuthenticateAsync(request.Username, request.Password);

        if (result.IsError)
            return result.ToActionResult();

        var accessToken = result.Value;
        return new OkObjectResult(new LoginResponse { AccessToken = accessToken });
    }
}
