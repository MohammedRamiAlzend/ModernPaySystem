using ModernPaySystem.SharedKernel.Domain.Commons;

namespace ModernPaySystem.Module.Identity.Application.Interfaces;

public interface IAuthenticationService
{
    Task<Result<string>> AuthenticateAsync(string username, string password);
    Task<Result<List<string>>> GetUserPermissionsAsync(Guid userId);
}
