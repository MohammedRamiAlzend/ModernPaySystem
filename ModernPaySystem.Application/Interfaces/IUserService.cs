using ModernPaySystem.Domain.DTOs;

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

    /// <summary>
    /// Get users by subsystem.
    /// </summary>
    Task<Result<IEnumerable<UserDto>>> GetBySubSystemAsync(SubSystem subSystem);

    /// <summary>
    /// Update existing user.
    /// </summary>
    Task<Result<UserDto>> UpdateAsync(Guid id, CreateUserDto user);

    Task<Result<IEnumerable<TemplateDto>>> GetVisitedTemplatesAsync(Guid userId);

    Task<Result<List<SubSystemDto>>> GetSubSystemsAsync();
}
