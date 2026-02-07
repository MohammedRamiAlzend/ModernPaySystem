using Microsoft.AspNetCore.Mvc;
using ModernPaySystem.Application.DTOs.Auth;
using ModernPaySystem.Domain.Commons;
using ModernPaySystem.Infrastructure.Extensions;
using ModernPaySystem.Application.Services;

namespace ModernPaySystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;

    public AuthController(IAuthenticationService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.AuthenticateAsync(request.Username, request.Password);

        if (result.IsError)
            return result.ToActionResult();

        var accessToken = result.Value;
        return new OkObjectResult(new LoginResponse { AccessToken = accessToken });
    }
}
