using System.Security.Cryptography;
using System.Text;
using ModernPaySystem.Module.Identity.Application.Interfaces;

namespace ModernPaySystem.Module.Identity.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        byte[] hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password).Equals(hash);
    }
}
