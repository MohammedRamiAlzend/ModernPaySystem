using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Module.Identity.Application;
using ModernPaySystem.Module.Identity.Application.Interfaces;
using ModernPaySystem.Module.Identity.Infrastructure.Persistence;
using ModernPaySystem.SharedKernel.Domain.Commons;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Identity.Infrastructure.Services;

public class AuthenticationService(
    IIdentityUnitOfWork unitOfWork,
    ITokenService tokenService,
    IPasswordHasher passwordHasher) : IAuthenticationService
{
    public async Task<Result<string>> AuthenticateAsync(string username, string password)
    {
        var userResult = await unitOfWork.Users.GetAsync(
            x => x.UserName == username,
            i => i.Include(u => u.Roles)
                  .ThenInclude(rp => rp.Permissions)
                  .Include(x => x.SubSystemUser),
            bypassAuth: true);

        if (userResult.IsError)
            return Error.Unauthorized("InvalidCredentials", "Invalid username or password.");

        var user = userResult.Value;

        if (!passwordHasher.VerifyPassword(password, user!.HashedPassword))
            return Error.Unauthorized("InvalidCredentials", "Invalid username or password.");

        var permissions = user.Roles
            .SelectMany(ur => ur.Permissions)
            .Select(rp => rp.Name)
            .Distinct()
            .ToList();

        return tokenService.GenerateAccessToken(user, permissions!);
    }

    public async Task<Result<List<string>>> GetUserPermissionsAsync(Guid userId)
    {
        var userResult = await unitOfWork.Users.GetAsync(
            x => x.Id == userId,
            i => i.Include(u => u.Roles)
                  .ThenInclude(rp => rp.Permissions),
            bypassAuth: true);

        if (userResult.IsError)
            return Error.NotFound("UserNotFound", "User not found.");

        var user = userResult.Value;

        var permissions = user!.Roles
            .SelectMany(ur => ur.Permissions)
            .Select(rp => rp.Name)
            .Distinct()
            .ToList();

        if (permissions.Count == 0)
            return Error.NotFound("UserNotFound", "User not found.");

        return permissions!;
    }
}
