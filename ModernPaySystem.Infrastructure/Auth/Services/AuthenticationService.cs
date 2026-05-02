global using System.Security.Cryptography;
global using System.Text;
global using ModernPaySystem.Domain.Commons;
global using ModernPaySystem.Domain.Entities.SharedEntities;
global using ModernPaySystem.Infrastructure.Persistence;
global using Microsoft.EntityFrameworkCore;
global using ModernPaySystem.Application.Services;
global using System.Linq;
global using System.Collections.Generic;

namespace ModernPaySystem.Infrastructure.Auth.Services;

public class AuthenticationService(IUnitOfWork uow,
    ITokenService tokenService,
    IPasswordHasher passwordHasher) : IAuthenticationService
{
    public async Task<Result<string>> AuthenticateAsync(string username, string password)
    {
        var userResult = await uow.Users.GetAsync(
            x => x.UserName == username,
            i => i.Include(u => u.Roles)
                  .ThenInclude(rp => rp.Permissions)
                  .Include(x => x.SubSystemUser),
            bypassAuth: true);

        if (userResult.IsError)
            return ApplicationErrors.InvalidCredentials;

        var user = userResult.Value;

        if (!passwordHasher.VerifyPassword(password, user.HashedPassword))
            return ApplicationErrors.InvalidCredentials;

        var permissions = user.Roles
            .SelectMany(ur => ur.Permissions)
            .Select(rp => rp.Name)
            .Distinct()
            .ToList();

        return tokenService.GenerateAccessToken(user, permissions!);
    }

    public async Task<Result<List<string>>> GetUserPermissionsAsync(Guid userId)
    {
        var userResult = await uow.Users.GetAsync(
            x => x.Id == userId,
            i => i.Include(u => u.Roles)
                  .ThenInclude(rp => rp.Permissions),
            bypassAuth: true);

        if (userResult.IsError)
            return ApplicationErrors.UserNotFound;

        var user = userResult.Value;

        var permissions = user.Roles
          .SelectMany(ur => ur.Permissions)
          .Select(rp => rp.Name)
          .Distinct()
          .ToList();

        if (permissions.Count == 0)
            return ApplicationErrors.UserNotFound;

        return permissions!;
    }
}