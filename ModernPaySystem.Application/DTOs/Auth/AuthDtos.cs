namespace ModernPaySystem.Application.DTOs.Auth;

public class LoginRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}

public class LoginResponse
{
    public required string AccessToken { get; set; }
}
