namespace ModernPaySystem.Application.Interfaces;

/// <summary>
/// Interface for User service CRUD operations.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Get all users.
    /// </summary>
    Task<Result<IEnumerable<User>>> GetAllAsync();

    /// <summary>
    /// Get user by id.
    /// </summary>
    Task<Result<User>> GetByIdAsync(Guid id);

    /// <summary>
    /// Get user by username.
    /// </summary>
    Task<Result<User>> GetByUsernameAsync(string username);

    /// <summary>
    /// Check if username exists.
    /// </summary>
    Task<bool> UsernameExistsAsync(string username);

    /// <summary>
    /// Create new user.
    /// </summary>
    Task<Result<User>> CreateAsync(User user);

    /// <summary>
    /// Update user.
    /// </summary>
    Task<Result<User>> UpdateAsync(Guid id, User user);

    /// <summary>
    /// Delete user.
    /// </summary>
    Task<Result<bool>> DeleteAsync(Guid id);
}
