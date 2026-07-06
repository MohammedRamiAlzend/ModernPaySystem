using ModernPaySystem.SharedKernel.Domain.Commons;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Identity.Application.Interfaces;

public interface IRoleService
{
    Task<Result<IEnumerable<RoleDto>>> GetAllAsync();

    Task<Result<PagedList<RoleDto>>> GetPagedAsync(int page, int pageSize);

    Task<Result<RoleDto>> GetByIdAsync(Guid id);

    Task<Result<RoleDto>> GetByNameAsync(string name);

    Task<Result<RoleDto>> CreateAsync(CreateRoleDto role);

    Task<Result<RoleDto>> UpdateAsync(Guid id, UpdateRoleDto role);

    Task<Result<bool>> DeleteAsync(Guid id);
}
