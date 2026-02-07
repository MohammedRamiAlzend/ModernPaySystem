using ModernPaySystem.Domain.Commons;
using ModernPaySystem.Domain.Entities.SharedEntities;

namespace ModernPaySystem.Application.Interfaces;

/// <summary>
/// Interface for Role service CRUD operations
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// Get all roles
    /// </summary>
    Task<Result<IEnumerable<Role>>> GetAllAsync();

    /// <summary>
    /// Get role by id
    /// </summary>
    Task<Result<Role>> GetByIdAsync(Guid id);

    /// <summary>
    /// Get role by name
    /// </summary>
    Task<Result<Role>> GetByNameAsync(string name);

    /// <summary>
    /// Create new role
    /// </summary>
    Task<Result<Role>> CreateAsync(Role role);

    /// <summary>
    /// Update role
    /// </summary>
    Task<Result<Role>> UpdateAsync(Guid id, Role role);

    /// <summary>
    /// Delete role
    /// </summary>
    Task<Result<bool>> DeleteAsync(Guid id);
}

/// <summary>
/// Interface for User service CRUD operations
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Get all users
    /// </summary>
    Task<Result<IEnumerable<User>>> GetAllAsync();

    /// <summary>
    /// Get user by id
    /// </summary>
    Task<Result<User>> GetByIdAsync(Guid id);

    /// <summary>
    /// Get user by username
    /// </summary>
    Task<Result<User>> GetByUsernameAsync(string username);

    /// <summary>
    /// Check if username exists
    /// </summary>
    Task<bool> UsernameExistsAsync(string username);

    /// <summary>
    /// Create new user
    /// </summary>
    Task<Result<User>> CreateAsync(User user);

    /// <summary>
    /// Update user
    /// </summary>
    Task<Result<User>> UpdateAsync(Guid id, User user);

    /// <summary>
    /// Delete user
    /// </summary>
    Task<Result<bool>> DeleteAsync(Guid id);
}
