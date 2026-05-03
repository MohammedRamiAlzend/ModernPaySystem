using ModernPaySystem.Domain.DTOs;
using ModernPaySystem.Infrastructure.Persistence.Repos;

namespace ModernPaySystem.Infrastructure.Services;

/// <summary>
/// Service implementation for Department operations
/// Uses IUnitOfWork directly following RequestService pattern
/// </summary>
public class DepartmentService(
    IUnitOfWork unitOfWork,
    ILogger<DepartmentService> logger,
    IHttpContextServiceManager httpContextServiceManager) : IDepartmentService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<DepartmentDto>> CreateAsync(CreateDepartmentDto dto, string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return ApplicationErrors.InvalidInput;

            // Validate parent if provided
            if (dto.ParentDepartmentId.HasValue)
            {
                var parentResult = await _unitOfWork.Departments.GetByIdAsync(dto.ParentDepartmentId.Value);
                if (parentResult.IsError || parentResult.Value == null)
                    return new Error("PARENT_NOT_FOUND", "Parent department not found", ErrorKind.NotFound);

                // Check for circular reference
                if (await _unitOfWork.Departments.WouldCreateCircularReferenceAsync(dto.ParentDepartmentId.Value, dto.ParentDepartmentId.Value))
                    return new Error("CIRCULAR_REFERENCE", "Cannot create circular reference", ErrorKind.Validation);
            }

            // Calculate level and materialized path
            int level = 1;
            string? materializedPath = null;

            if (dto.ParentDepartmentId.HasValue)
            {
                var parentResult = await _unitOfWork.Departments.GetByIdAsync(dto.ParentDepartmentId.Value);
                if (parentResult.Value != null)
                {
                    level = parentResult.Value.Level + 1;
                    materializedPath = $"{parentResult.Value.MaterializedPath}/{GetShortId(Guid.NewGuid())}";
                }
            }
            else
            {
                materializedPath = GetShortId(Guid.NewGuid());
            }

            var department = new Department
            {
                Name = dto.Name,
                Code = dto.Code,
                Description = dto.Description,
                ParentDepartmentId = dto.ParentDepartmentId,
                Level = level,
                MaterializedPath = materializedPath,
                Type = dto.Type,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            var addResult = await _unitOfWork.Departments.AddAsync(department);
            if (addResult.IsError)
                return addResult.Errors;

            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Created department: {DepartmentId} with name: {Name}", department.Id, department.Name);

            return department.MapToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating department");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<DepartmentDto?>> GetByIdAsync(Guid id)
    {
        try
        {
            var result = await _unitOfWork.Departments.GetByIdAsync(id);
            if (result.IsError)
                return result.Errors;

            if (result.Value == null)
                return null;

            return MapToDto(result.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching department by id: {DepartmentId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<DepartmentDto>> UpdateAsync(Guid id, UpdateDepartmentDto dto, string userId)
    {
        try
        {
            var existingResult = await _unitOfWork.Departments.GetByIdAsync(id);
            if (existingResult.IsError)
                return existingResult.Errors;

            if (existingResult.Value == null)
                return new Error("NOT_FOUND", "Department not found", ErrorKind.NotFound);

            var department = existingResult.Value;

            // Validate parent change if provided
            if (dto.ParentDepartmentId.HasValue && dto.ParentDepartmentId != department.ParentDepartmentId)
            {
                // Check for circular reference
                if (await _unitOfWork.Departments.WouldCreateCircularReferenceAsync(id, dto.ParentDepartmentId.Value))
                    return new Error("CIRCULAR_REFERENCE", "Cannot create circular reference", ErrorKind.Validation);

                // Update level and materialized path
                var parentResult = await _unitOfWork.Departments.GetByIdAsync(dto.ParentDepartmentId.Value);
                if (parentResult.Value != null)
                {
                    department.Level = parentResult.Value.Level + 1;
                    department.MaterializedPath = $"{parentResult.Value.MaterializedPath}/{GetShortId(department.Id)}";
                }
            }

            // Update fields
            if (!string.IsNullOrWhiteSpace(dto.Name))
                department.Name = dto.Name;

            department.Code = dto.Code ?? department.Code;
            department.Description = dto.Description ?? department.Description;
            department.ParentDepartmentId = dto.ParentDepartmentId;
            department.Type = dto.Type ?? department.Type;
            department.UpdatedByUserId = userId;
            department.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _unitOfWork.Departments.UpdateAsync(department);
            if (updateResult.IsError)
                return updateResult.Errors;

            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Updated department: {DepartmentId}", id);

            return MapToDto(department);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating department: {DepartmentId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            // Check if department has children
            var hasChildren = await _unitOfWork.Departments.HasChildrenAsync(id);
            if (hasChildren)
                return new Error("HAS_CHILDREN", "Cannot delete department with children", ErrorKind.Validation);

            var deleted = await _unitOfWork.Departments.RemoveAsync(d => d.Id == id);
            if (deleted.IsError)
                return deleted.Errors;

            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Deleted department: {DepartmentId}", id);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting department: {DepartmentId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<List<DepartmentTreeDto>>> GetTreeAsync()
    {
        try
        {
            var roots = await _unitOfWork.Departments.GetRootDepartmentsAsync();
            if (roots.IsError)
                return roots.Errors;

            var tree = new List<DepartmentTreeDto>();
            foreach (var root in roots.Value!)
            {
                var rootNode = MapToTreeDto(root);
                await BuildTreeRecursive(root.Id, rootNode);
                tree.Add(rootNode);
            }

            return tree;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching department tree");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<List<DepartmentTreeDto>>> GetSubTreeAsync(Guid departmentId)
    {
        try
        {
            var deptResult = await _unitOfWork.Departments.GetByIdAsync(departmentId);
            if (deptResult.IsError || deptResult.Value == null)
                return new Error("NOT_FOUND", "Department not found", ErrorKind.NotFound);

            var rootNode = MapToTreeDto(deptResult.Value);
            await BuildTreeRecursive(departmentId, rootNode);

            return new List<DepartmentTreeDto> { rootNode };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching subtree for department: {DepartmentId}", departmentId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<List<DepartmentDto>>> GetChildrenAsync(Guid departmentId)
    {
        try
        {
            var children = await _unitOfWork.Departments.GetChildrenAsync(departmentId);
            if (children.IsError)
                return children.Errors;

            return children.Value!.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching children for department: {DepartmentId}", departmentId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<DepartmentDto?>> GetParentAsync(Guid departmentId)
    {
        try
        {
            var deptResult = await _unitOfWork.Departments.GetByIdAsync(departmentId);
            if (deptResult.IsError || deptResult.Value == null || !deptResult.Value.ParentDepartmentId.HasValue)
                return null;

            var parentResult = await _unitOfWork.Departments.GetByIdAsync(deptResult.Value.ParentDepartmentId.Value);
            if (parentResult.IsError || parentResult.Value == null)
                return null;

            return MapToDto(parentResult.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching parent for department: {DepartmentId}", departmentId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<List<DepartmentDto>>> SearchAsync(string searchTerm, int level = 0)
    {
        try
        {
            var allDepts = await _unitOfWork.Departments.GetAllAsync();
            if (allDepts.IsError)
                return allDepts.Errors;

            var filtered = allDepts.Value!
                .Where(d => d.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                           (d.Code != null && d.Code.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (level > 0)
                filtered = filtered.Where(d => d.Level == level).ToList();

            return filtered.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching departments with term: {SearchTerm}", searchTerm);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<List<DepartmentDto>>> GetByLevelAsync(int level)
    {
        try
        {
            var allDepts = await _unitOfWork.Departments.GetAllAsync();
            if (allDepts.IsError)
                return allDepts.Errors;

            var filtered = allDepts.Value!.Where(d => d.Level == level).ToList();
            return filtered.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching departments at level: {Level}", level);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<List<DepartmentDto>>> GetPathToRootAsync(Guid departmentId)
    {
        try
        {
            var path = await _unitOfWork.Departments.GetPathToRootAsync(departmentId);
            if (path.IsError)
                return path.Errors;

            return path.Value!.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching path to root for department: {DepartmentId}", departmentId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<List<UserDto>>> GetUsersInDepartmentAsync(Guid departmentId, bool includeSubDepartments = false)
    {
        try
        {
            var deptResult = await _unitOfWork.Departments.GetAsync(x => x.Id == departmentId, i => i.Include(x => x.Users).ThenInclude(x => x.SubSystemUser));
            if (deptResult.IsError || deptResult.Value == null)
                return new Error("NOT_FOUND", "Department not found", ErrorKind.NotFound);
            return deptResult.Value.Users.Select(x => x.ToDto()).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching users in department: {DepartmentId}", departmentId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> AssignUserToDepartmentAsync(Guid userId, Guid departmentId)
    {
        try
        {
            var deptResult = await _unitOfWork.Departments.GetByIdAsync(departmentId);
            if (deptResult.IsError || deptResult.Value == null)
                return new Error("NOT_FOUND", "Department not found", ErrorKind.NotFound);

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user.IsError || user.Value == null)
                return new Error("NOT_FOUND", "User not found", ErrorKind.NotFound);

            user.Value.DepartmentId = departmentId;
            var updateResult = await _unitOfWork.Users.UpdateAsync(user.Value);
            if (updateResult.IsError)
                return updateResult.Errors;
            await _unitOfWork.SaveChangesAsync();

            logger.LogInformation("Assigning user: {UserId} to department: {DepartmentId}", userId, departmentId);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error assigning user to department");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> RemoveUserFromDepartmentAsync(Guid userId)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user.IsError || user.Value == null)
                return new Error("NOT_FOUND", "User not found", ErrorKind.NotFound);

            user.Value.DepartmentId = null;
            var updateResult = await _unitOfWork.Users.UpdateAsync(user.Value);
            if (updateResult.IsError)
                return updateResult.Errors;

            await _unitOfWork.SaveChangesAsync();

            logger.LogInformation("Removed user: {UserId} from department", userId);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing user from department");
            return ApplicationErrors.InternalServerError;
        }
    }

    public bool CanAssignParent(Guid departmentId, Guid parentDepartmentId)
    {
        // Basic validation - cannot assign self as parent
        if (departmentId == parentDepartmentId)
            return false;

        // Check for circular references synchronously
        // We cannot call async repository methods directly, so we implement a basic check
        // For a production system, this should ideally be done async in the service layer
        try
        {
            // Cannot assign self as parent (already checked above)
            // Check if proposed parent is a descendant of the department
            var currentId = parentDepartmentId;
            var maxDepth = 100; // Prevent infinite loops
            var depth = 0;

            while (currentId != Guid.Empty && depth < maxDepth)
            {
                // Get the department synchronously - we'll use a blocking call for this check
                // In a real implementation, this check should be done async in the service layer
                var result = _unitOfWork.Departments.GetByIdAsync(currentId).GetAwaiter().GetResult();
                if (result.IsError || result.Value == null || !result.Value.ParentDepartmentId.HasValue)
                    break;

                if (result.Value.Id == departmentId)
                    return false; // Circular reference detected

                currentId = result.Value.ParentDepartmentId.Value;
                depth++;
            }

            return true;
        }
        catch
        {
            // If there's an error in our check, we'll be conservative and allow the assignment
            // The actual validation will happen when we try to make the change
            return true;
        }
    }

    private async Task BuildTreeRecursive(Guid parentId, DepartmentTreeDto parentNode)
    {
        var children = await _unitOfWork.Departments.GetChildrenAsync(parentId);
        if (children.IsError || children.Value == null || !children.Value.Any())
            return;

        foreach (var child in children.Value)
        {
            var childNode = MapToTreeDto(child);
            await BuildTreeRecursive(child.Id, childNode);
            parentNode.Children.Add(childNode);
        }
    }

    private DepartmentDto MapToDto(Department department)
    {
        return new DepartmentDto
        {
            Id = department.Id,
            Name = department.Name,
            Code = department.Code,
            Description = department.Description,
            ParentDepartmentId = department.ParentDepartmentId,
            Level = department.Level,
            MaterializedPath = department.MaterializedPath,
            Type = department.Type,
            ChildrenCount = 0, // Would need to query for this
            UsersCount = department.Users?.Count ?? 0,
            CreatedAt = department.CreatedAt
        };
    }

    private DepartmentTreeDto MapToTreeDto(Department department)
    {
        return new DepartmentTreeDto
        {
            Id = department.Id,
            Name = department.Name,
            Code = department.Code,
            Level = department.Level,
            Type = department.Type,
            Children = new List<DepartmentTreeDto>()
        };
    }

    private static string GetShortId(Guid id)
    {
        return id.ToString("N")[..8];
    }

}
