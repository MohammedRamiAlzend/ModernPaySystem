using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ModernPaySystem.Module.Identity.Application.Interfaces;
using ModernPaySystem.SharedKernel.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ModernPaySystem.Module.Identity.Infrastructure.Services;

public class JwtTokenService(IConfiguration configuration) : ITokenService
{
    public string GenerateAccessToken(User user, List<string> permissions)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured")));

        var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        if (!string.IsNullOrEmpty(user.UserName))
            claims.Add(new Claim(ClaimTypes.Name, user.UserName));

        if (user.SubSystemUser?.SubSystem != null)
            claims.Add(new Claim(ClaimTypes.System, user.SubSystemUser.SubSystem.ToString()!));

        claims.Add(new Claim("IsDepartmentHead", user.IsDepartmentHead.ToString()));

        foreach (string permission in (permissions ?? []).Where(x => !string.IsNullOrEmpty(x)))
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
}
