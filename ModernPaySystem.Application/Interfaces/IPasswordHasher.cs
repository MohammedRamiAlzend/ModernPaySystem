namespace ModernPaySystem.Application.Interfaces;
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a plain text password.
    /// </summary>
    string HashPassword(string password);
}
