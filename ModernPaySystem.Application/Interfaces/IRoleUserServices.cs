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
    Task<Result<IEnumerable<RoleDto>>> GetAllAsync();

    /// <summary>
    /// Get paged roles.
    /// </summary>
    Task<Result<PagedList<RoleDto>>> GetPagedAsync(int page, int pageSize);

    /// <summary>
    /// Get role by id.
    /// </summary>
    Task<Result<RoleDto>> GetByIdAsync(Guid id);

    /// <summary>
    /// Get role by name.
    /// </summary>
    Task<Result<RoleDto>> GetByNameAsync(string name);

    /// <summary>
    /// Create new role.
    /// </summary>
    Task<Result<RoleDto>> CreateAsync(CreateRoleDto role);

    /// <summary>
    /// Update role.
    /// </summary>
    Task<Result<RoleDto>> UpdateAsync(Guid id, UpdateRoleDto role);

    /// <summary>
    /// Delete role.
    /// </summary>
    Task<Result<bool>> DeleteAsync(Guid id);
}
