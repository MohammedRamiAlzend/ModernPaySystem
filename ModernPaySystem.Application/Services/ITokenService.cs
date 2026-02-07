namespace ModernPaySystem.Application.Services;

public interface ITokenService
{
    string GenerateAccessToken(User user, List<string> permissions);
    ClaimsPrincipal? GetClaimsFromExpiredToken(string token);
}
