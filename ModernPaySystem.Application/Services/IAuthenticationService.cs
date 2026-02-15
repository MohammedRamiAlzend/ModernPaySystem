namespace ModernPaySystem.Application.Services;

public interface IAuthenticationService
{
    Task<Result<string>> AuthenticateAsync(string username, string password);
    Task<Result<List<string>>> GetUserPermissionsAsync(Guid userId);
}
