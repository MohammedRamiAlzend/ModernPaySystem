using ModernPaySystem.Domain.Commons;
using ModernPaySystem.Domain.DTOs;
using ModernPaySystem.Domain.Entities.SharedEntities;

namespace ModernPaySystem.Application.Interfaces;

/// <summary>
/// Interface for User service CRUD operations.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Get all users.
    /// </summary>
    Task<Result<IEnumerable<UserDto>>> GetAllAsync();

    /// <summary>
    /// Get paged users.
    /// </summary>
    Task<Result<PagedList<UserDto>>> GetPagedAsync(int page, int pageSize);

    /// <summary>
    /// Get user by id.
    /// </summary>
    Task<Result<UserDto>> GetByIdAsync(Guid id);

    /// <summary>
    /// Get user by username.
    /// </summary>
    Task<Result<UserDto>> GetByUsernameAsync(string username);

    /// <summary>
    /// Create new user.
    /// </summary>
    Task<Result<UserDto>> CreateAsync(CreateUserDto user);

    /// <summary>
    /// Delete user.
    /// </summary>
    Task<Result<bool>> DeleteAsync(Guid id);
    Task<Result<List<SubSystemDto>>> GetSubSystemsAsync();
}