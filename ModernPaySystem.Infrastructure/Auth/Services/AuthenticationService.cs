using System.Security.Cryptography;
using System.Text;
using ModernPaySystem.Domain.Commons;
using ModernPaySystem.Domain.Entities.SharedEntities;
using ModernPaySystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Application.Services;

namespace ModernPaySystem.Infrastructure.Auth.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly AppDbContext _dbContext;
    private readonly ITokenService _tokenService;

    public AuthenticationService(AppDbContext dbContext, ITokenService tokenService)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    public async Task<Result<string>> AuthenticateAsync(string username, string password)
    {
        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.UserName == username);

        if (user == null)
            return ApplicationError.InvalidCredentials;

        if (!VerifyPassword(password, user.HashedPassword))
            return ApplicationError.InvalidCredentials;

        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role!.RolePermissions)
            .Select(rp => rp.Permission!.Name)
            .Distinct()
            .ToList();

        var accessToken = _tokenService.GenerateAccessToken(user, permissions);

        return accessToken;
    }

    public async Task<Result<List<string>>> GetUserPermissionsAsync(Guid userId)
    {
        var permissions = await _dbContext.Users
            .Where(u => u.Id == userId)
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .SelectMany(u => u.UserRoles
                .SelectMany(ur => ur.Role!.RolePermissions
                    .Select(rp => rp.Permission!.Name)))
            .Distinct()
            .ToListAsync();

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
