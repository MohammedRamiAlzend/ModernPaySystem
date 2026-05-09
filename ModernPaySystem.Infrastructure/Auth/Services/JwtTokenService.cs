global using Microsoft.Extensions.Configuration;
global using Microsoft.IdentityModel.Tokens;
global using ModernPaySystem.Application.Services;
global using ModernPaySystem.Domain.Entities.SharedEntities;
global using System.IdentityModel.Tokens.Jwt;
global using System.Linq;
global using System.Security.Claims;
global using System.Text;

namespace ModernPaySystem.Infrastructure.Auth.Services;

public class JwtTokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(User user, List<string> permissions)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured")));

        var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        // Defensive null checks to avoid NullReferenceException when parts of `user` are missing.
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user != null ? user.Id.ToString() : string.Empty)
        };

        if (!string.IsNullOrEmpty(user?.UserName))
            claims.Add(new Claim(ClaimTypes.Name, user.UserName));

        // Only add subsystem claim when SubSystemUser and SubSystem are present.
        if (user?.SubSystemUser?.SubSystem != null)
            claims.Add(new Claim(ClaimTypes.System, user.SubSystemUser.SubSystem.ToString()!));

        // Guard against a null permissions list and skip null/empty permission entries.
        foreach (string permission in (permissions ?? Enumerable.Empty<string>()).Where(x => !string.IsNullOrEmpty(x)))
        {
            claims.Add(new Claim("permission", permission));
        }

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpirationMinutes"] ?? "15")),
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal? GetClaimsFromExpiredToken(string token)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured")));

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = secretKey,
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

        if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
            !jwtSecurityToken.Header.Alg.Equals(
                SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
            return null;

        return principal;
    }
}
