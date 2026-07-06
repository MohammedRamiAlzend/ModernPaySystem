using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Identity.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user, List<string> permissions);
}
