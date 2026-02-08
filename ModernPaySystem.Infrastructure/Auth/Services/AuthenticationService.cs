using System.Security.Cryptography;
using System.Text;
using ModernPaySystem.Domain.Commons;
using ModernPaySystem.Domain.Entities.SharedEntities;
using ModernPaySystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Application.Services;

namespace ModernPaySystem.Infrastructure.Auth.Services;

public class AuthenticationService(IUnitOfWork uow, ITokenService tokenService) : IAuthenticationService
{
    public async Task<Result<string>> AuthenticateAsync(string username, string password)
    {
        var userResult = await uow.Users.GetAsync(x => x.UserName == username,
            i => i.Include(u => u.Roles)
                  .ThenInclude(rp => rp.Permissions));

        if (userResult.IsError)
            return ApplicationError.InvalidCredentials;

        var user = userResult.Value;

        if (!VerifyPassword(password, user.HashedPassword))
            return ApplicationError.InvalidCredentials;

        var permissions = user.Roles
            .SelectMany(ur => ur.Permissions)
            .Select(rp => rp.Name)
            .Distinct()
            .ToList();

        return tokenService.GenerateAccessToken(user, permissions);
    }

    public async Task<Result<List<string>>> GetUserPermissionsAsync(Guid userId)
    {
        var userResult = await uow.Users.GetAsync(
            x => x.Id == userId,
            i => i.Include(u => u.Roles)
                  .ThenInclude(rp => rp.Permissions));

        if (userResult.IsError)
            return ApplicationError.UserNotFound;

        var user = userResult.Value;

        var permissions = user.Roles
          .SelectMany(ur => ur.Permissions)
          .Select(rp => rp.Name)
          .Distinct()
          .ToList();

        if (permissions.Count == 0)
            return ApplicationError.UserNotFound;

        return permissions;
    }

    public string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    public bool VerifyPassword(string password, string hash)
    {
        var hashOfInput = HashPassword(password);
        return hashOfInput.Equals(hash);
    }
}
