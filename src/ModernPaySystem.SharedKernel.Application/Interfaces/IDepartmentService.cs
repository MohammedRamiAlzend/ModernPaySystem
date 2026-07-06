using ModernPaySystem.SharedKernel.Domain.Commons;
using ModernPaySystem.SharedKernel.Domain.DTOs;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.SharedKernel.Application.Interfaces;

public interface IDepartmentService
{
    Task<Result<DepartmentDto>> CreateAsync(CreateDepartmentDto dto, string userId);

    Task<Result<DepartmentDto?>> GetByIdAsync(Guid id);
    Task<Result<DepartmentDto?>> GetByUserIdAsync(Guid id);

    Task<Result<DepartmentDto>> UpdateAsync(Guid id, UpdateDepartmentDto dto, string userId);

    Task<Result<bool>> DeleteAsync(Guid id);

    Task<Result<List<DepartmentTreeDto>>> GetTreeAsync();

    Task<Result<List<DepartmentTreeDto>>> GetSubTreeAsync(Guid departmentId);

    Task<Result<List<DepartmentDto>>> GetChildrenAsync(Guid departmentId);

    Task<Result<DepartmentDto?>> GetParentAsync(Guid departmentId);

    Task<Result<List<DepartmentDto>>> SearchAsync(string? searchTerm = null, int level = 0);

    Task<Result<List<DepartmentDto>>> GetByLevelAsync(int level);

    Task<Result<List<DepartmentDto>>> GetPathToRootAsync(Guid departmentId);

    Task<Result<List<UserDto>>> GetUsersInDepartmentAsync(Guid departmentId, bool includeSubDepartments = false);

    Task<Result<bool>> AssignUserToDepartmentAsync(Guid userId, Guid departmentId);

    Task<Result<bool>> AssignDepartmentHeadAsync(Guid departmentId, Guid userId);

    Task<Result<bool>> RemoveUserFromDepartmentAsync(Guid userId);

    Task<Result<bool>> CanAssignParentAsync(Guid departmentId, Guid parentDepartmentId);
}
