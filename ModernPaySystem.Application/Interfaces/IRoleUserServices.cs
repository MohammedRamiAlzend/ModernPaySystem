using ModernPaySystem.Domain.Commons;
using ModernPaySystem.Domain.Entities.SharedEntities;

namespace ModernPaySystem.Application.Interfaces;

/// <summary>
/// Interface for Role service CRUD operations.
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// Get all roles.
    /// </summary>
    Task<Result<IEnumerable<Role>>> GetAllAsync();

    /// <summary>
    /// Get paged roles.
    /// </summary>
    Task<Result<PagedList<Role>>> GetPagedAsync(int page, int pageSize);

    /// <summary>
    /// Get role by id.
    /// </summary>
    Task<Result<Role>> GetByIdAsync(Guid id);

    /// <summary>
    /// Get role by name.
    /// </summary>
    Task<Result<Role>> GetByNameAsync(string name);

    /// <summary>
    /// Create new role.
    /// </summary>
    Task<Result<Role>> CreateAsync(Role role);

    /// <summary>
    /// Update role.
    /// </summary>
    Task<Result<Role>> UpdateAsync(Guid id, Role role);

    /// <summary>
    /// Delete role.
    /// </summary>
    Task<Result<bool>> DeleteAsync(Guid id);
}
