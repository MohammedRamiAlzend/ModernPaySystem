using ModernPaySystem.Domain.DTOs;
using ModernPaySystem.Infrastructure.Persistence.Repos;
using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Domain.Entities.SharedEntities;


namespace ModernPaySystem.Infrastructure.Services;

public class DepartmentService(
    IUnitOfWork unitOfWork,
    IArchiveLeaderService archiveLeaderService,
    ILogger<DepartmentService> logger) : IDepartmentService
{

    public async Task<Result<DepartmentDto>> CreateAsync(CreateDepartmentDto dto, string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return ApplicationErrors.InvalidInput;

            if (dto.ParentDepartmentId.HasValue)
            {
                var parentResult = await unitOfWork.Departments.GetByIdAsync(dto.ParentDepartmentId.Value);
                if (parentResult.IsError || parentResult.Value == null)
                    return new Error("PARENT_NOT_FOUND", "Parent department not found", ErrorKind.NotFound);
            }

            int level = 1;
            string? materializedPath = null;

            if (dto.ParentDepartmentId.HasValue)
            {
                var parentResult = await unitOfWork.Departments.GetByIdAsync(dto.ParentDepartmentId.Value);
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

            var userResult = await unitOfWork.Users.GetAsync(x => x.Id == dto.HeadedUserId, i => i.Include(x => x.HeadedDepartment));
            if (userResult.IsError)
                return userResult.Errors;
            if (userResult.Value!.HeadedDepartmentId.HasValue)
            {
                return ApplicationErrors.UserAlreadyDepartmentHeader(userResult.Value.HeadedDepartmentId.Value, userResult.Value.HeadedDepartment.Name);
            }
            userResult.Value.IsDepartmentHead = true;
            var department = new Department
            {
                Name = dto.Name,
                Code = dto.Code,
                Description = dto.Description,
                ParentDepartmentId = dto.ParentDepartmentId,
                Level = level,
                MaterializedPath = materializedPath,
                Type = dto.Type,
                DepartmentHeadId = dto.HeadedUserId,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            var addResult = await unitOfWork.Departments.AddAsync(department);
            if (addResult.IsError)
                return addResult.Errors;

            userResult.Value.DepartmentId = department.Id;
            var userUpdateResult = await unitOfWork.Users.UpdateAsync(userResult.Value);

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
            var result = await unitOfWork.Departments.GetAsync(x => x.Id == id, i => i.Include(x => x.DepartmentHead).Include(x => x.Users).Include(x => x.ChildDepartments));
            if (result.IsError)
                return result.Errors;

            if (result.Value == null)
                return null!;

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
            var existingResult = await unitOfWork.Departments.GetByIdAsync(id);
            if (existingResult.IsError)
                return existingResult.Errors;

            if (existingResult.Value == null)
                return new Error("NOT_FOUND", "القسم المطلوب غير موجود", ErrorKind.NotFound, "القسم المطلوب غير موجود");

            var department = existingResult.Value;

            if (dto.ParentDepartmentId.HasValue && dto.ParentDepartmentId != department.ParentDepartmentId)
            {
                if (dto.ParentDepartmentId.Value == id)
                    return new Error("SELF_PARENT", "لا يمكن تعيين القسم كأب لنفسه", ErrorKind.Validation, "لا يمكن تعيين القسم كأب لنفسه");

                var parentExists = await unitOfWork.Departments.GetByIdAsync(dto.ParentDepartmentId.Value);
                if (parentExists.IsError || parentExists.Value == null)
                    return new Error("PARENT_NOT_FOUND", "القسم الأب المحدد غير موجود", ErrorKind.Validation, "القسم الأب المحدد غير موجود");

                if (await unitOfWork.Departments.WouldCreateCircularReferenceAsync(id, dto.ParentDepartmentId.Value))
                    return new Error("CIRCULAR_REFERENCE", "لا يمكن نقل القسم تحت أحد أبنائه (مرجعية دائرية)", ErrorKind.Validation, "لا يمكن نقل القسم تحت أحد أبنائه لأن ذلك يُنشئ علاقة دائرية");

                department.Level = parentExists.Value.Level + 1;
                department.MaterializedPath = $"{parentExists.Value.MaterializedPath}/{GetShortId(department.Id)}";
                department.ParentDepartmentId = dto.ParentDepartmentId.Value;

                logger.LogInformation("Moving department {DeptId} under new parent {ParentId}", id, dto.ParentDepartmentId.Value);
            }

            if (!string.IsNullOrWhiteSpace(dto.Name))
                department.Name = dto.Name;

            department.Code = dto.Code ?? department.Code;
            department.Description = dto.Description ?? department.Description;
            department.Type = dto.Type ?? department.Type;
            if (dto.HeadedUserId.HasValue)
                department.DepartmentHeadId = dto.HeadedUserId.Value;
            department.UpdatedByUserId = userId;
            department.UpdatedAt = DateTime.UtcNow;

            var updateResult = await unitOfWork.Departments.UpdateAsync(department);
            if (updateResult.IsError)
            {
                logger.LogError("Repository UpdateAsync failed for department {DeptId}: {Errors}", id, string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                return new Error("UPDATE_FAILED", "فشل تحديث القسم في قاعدة البيانات", ErrorKind.Failure, "فشل تحديث بيانات القسم، يرجى التحقق من البيانات المدخلة");
            }

            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Updated department: {DepartmentId}", id);

            return MapToDto(department);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
        {
            logger.LogError(dbEx, "Database error updating department: {DepartmentId}. Inner: {Inner}", id, dbEx.InnerException?.Message);
            
            var innerMsg = dbEx.InnerException?.Message ?? "";
            
            if (innerMsg.Contains("UNIQUE") || innerMsg.Contains("duplicate"))
                return new Error("DUPLICATE", "يوجد قسم آخر بنفس الاسم أو الكود", ErrorKind.Validation, "يوجد قسم آخر بنفس الاسم أو الكود، يرجى اختيار اسم مختلف");
            
            if (innerMsg.Contains("FOREIGN KEY") || innerMsg.Contains("REFERENCE"))
                return new Error("FK_VIOLATION", "القسم الأب المحدد غير صالح أو تم حذفه", ErrorKind.Validation, "القسم الأب المحدد غير صالح أو تم حذفه، يرجى تحديث الصفحة والمحاولة مرة أخرى");
            
            return new Error("DB_ERROR", $"خطأ في قاعدة البيانات: {innerMsg}", ErrorKind.Failure, $"حدث خطأ في قاعدة البيانات أثناء تحديث القسم: {innerMsg}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating department: {DepartmentId}. Exception: {Message}. StackTrace: {Stack}", id, ex.Message, ex.StackTrace);
            return new Error("INTERNAL_ERROR", $"خطأ غير متوقع: {ex.Message}", ErrorKind.Failure, $"حدث خطأ غير متوقع أثناء تحديث القسم: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            var hasChildren = await unitOfWork.Departments.HasChildrenAsync(id);
            if (hasChildren)
                return new Error("HAS_CHILDREN", "Cannot delete department with children", ErrorKind.Validation);

            var deleted = await unitOfWork.Departments.RemoveAsync(d => d.Id == id);
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
            var roots = await unitOfWork.Departments.GetAllAsync(d => d.ParentDepartmentId == null, i => i.Include(x => x.DepartmentHead).Include(x => x.ChildDepartments));
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
            var deptResult = await unitOfWork.Departments.GetAsync(x => x.Id == departmentId, i => i.Include(x => x.DepartmentHead).Include(x => x.ChildDepartments));
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
            var children = await unitOfWork.Departments.GetChildrenAsync(departmentId);
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
            var deptResult = await unitOfWork.Departments.GetByIdAsync(departmentId);
            if (deptResult.IsError || deptResult.Value == null || !deptResult.Value.ParentDepartmentId.HasValue)
                return null!;

            var parentResult = await unitOfWork.Departments.GetByIdAsync(deptResult.Value.ParentDepartmentId.Value);
            if (parentResult.IsError || parentResult.Value == null)
                return null!;

            return MapToDto(parentResult.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching parent for department: {DepartmentId}", departmentId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<List<DepartmentDto>>> SearchAsync(string? searchTerm = null, int level = 0)
    {
        try
        {
            List<Expression<Func<Department, bool>>> filters = [];

            if (level > 0)
                filters.Add(d => d.Level == level);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm;
                filters.Add(d => d.Name.Contains(term) || (d.Code != null && d.Code.Contains(term)));
            }

            var result = await unitOfWork.Departments.GetAllAsync(
                additionalFilters: filters.Count > 0 ? filters : null,
                transform: q => q.Include(d => d.DepartmentHead)
            );

            if (result.IsError)
                return result.Errors;

            return result.Value!.Select(MapToDto).ToList();
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
            var allDepts = await unitOfWork.Departments.GetAllAsync(d => d.Level == level, i => i.Include(x => x.DepartmentHead));
            if (allDepts.IsError)
                return allDepts.Errors;

            var filtered = allDepts.Value!.ToList();


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
            var path = await unitOfWork.Departments.GetPathToRootAsync(departmentId);
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
            var deptResult = await unitOfWork.Departments.GetAsync(x => x.Id == departmentId, i => i.Include(x => x.Users).ThenInclude(x => x.SubSystemUser));
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
            var deptResult = await unitOfWork.Departments.GetByIdAsync(departmentId);
            if (deptResult.IsError || deptResult.Value == null)
                return new Error("NOT_FOUND", "Department not found", ErrorKind.NotFound);

            var user = await unitOfWork.Users.GetByIdAsync(userId);
            if (user.IsError || user.Value == null)
                return new Error("NOT_FOUND", "User not found", ErrorKind.NotFound);

            var previousDepartmentId = user.Value.DepartmentId;
            user.Value.DepartmentId = departmentId;
            var updateResult = await unitOfWork.Users.UpdateAsync(user.Value);
            if (updateResult.IsError)
                return updateResult.Errors;
            await unitOfWork.SaveChangesAsync();

            if (previousDepartmentId.HasValue && previousDepartmentId.Value != departmentId)
            {
                var revokeResult = await archiveLeaderService.RevokeAssignmentsForUserAsync(userId, previousDepartmentId.Value);
                if (revokeResult.IsError)
                {
                    return revokeResult.Errors;
                }
            }

            logger.LogInformation("Assigning user: {UserId} to department: {DepartmentId}", userId, departmentId);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error assigning user to department");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> AssignDepartmentHeadAsync(Guid departmentId, Guid userId)
    {
        try
        {
            var departmentResult = await unitOfWork.Departments.GetByIdAsync(departmentId);
            if (departmentResult.IsError || departmentResult.Value == null)
                return new Error("NOT_FOUND", "Department not found", ErrorKind.NotFound);

            var userResult = await unitOfWork.Users.GetByIdAsync(userId);
            if (userResult.IsError || userResult.Value == null)
                return new Error("NOT_FOUND", "User not found", ErrorKind.NotFound);

            var department = departmentResult.Value;
            var user = userResult.Value;

            if (user.IsDepartmentHead && user.HeadedDepartmentId.HasValue && user.HeadedDepartmentId.Value != departmentId)
                return new Error("USER_ALREADY_DEPARTMENT_HEAD", "User is already assigned as head of another department", ErrorKind.Validation);

            if (department.DepartmentHeadId is Guid previousHeadId && previousHeadId != userId)
            {
                var previousHeadResult = await unitOfWork.Users.GetByIdAsync(previousHeadId);
                if (!previousHeadResult.IsError && previousHeadResult.Value != null)
                {
                    previousHeadResult.Value.IsDepartmentHead = false;
                    previousHeadResult.Value.HeadedDepartmentId = null;
                    var previousHeadUpdate = await unitOfWork.Users.UpdateAsync(previousHeadResult.Value);
                    if (previousHeadUpdate.IsError)
                        return previousHeadUpdate.Errors;
                }
            }

            department.DepartmentHeadId = userId;
            user.IsDepartmentHead = true;
            user.HeadedDepartmentId = departmentId;
            if (!user.DepartmentId.HasValue)
                user.DepartmentId = departmentId;

            var departmentUpdate = await unitOfWork.Departments.UpdateAsync(department);
            if (departmentUpdate.IsError)
                return departmentUpdate.Errors;

            var userUpdate = await unitOfWork.Users.UpdateAsync(user);
            if (userUpdate.IsError)
                return userUpdate.Errors;

            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Assigned user: {UserId} as head of department: {DepartmentId}", userId, departmentId);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error assigning department head: {UserId} to department: {DepartmentId}", userId, departmentId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> RemoveUserFromDepartmentAsync(Guid userId)
    {
        try
        {
            var user = await unitOfWork.Users.GetByIdAsync(userId);
            if (user.IsError || user.Value == null)
                return new Error("NOT_FOUND", "User not found", ErrorKind.NotFound);

            var previousDepartmentId = user.Value.DepartmentId;
            user.Value.DepartmentId = null;
            var updateResult = await unitOfWork.Users.UpdateAsync(user.Value);
            if (updateResult.IsError)
                return updateResult.Errors;

            await unitOfWork.SaveChangesAsync();
            if (previousDepartmentId.HasValue)
            {
                var revokeResult = await archiveLeaderService.RevokeAssignmentsForUserAsync(userId, previousDepartmentId.Value);
                if (revokeResult.IsError)
                {
                    return revokeResult.Errors;
                }
            }

            logger.LogInformation("Removed user: {UserId} from department", userId);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing user from department");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> CanAssignParentAsync(Guid departmentId, Guid parentDepartmentId)
    {
        if (departmentId == parentDepartmentId)
            return false;

        try
        {
            var currentId = parentDepartmentId;
            const int maxDepth = 100;
            var depth = 0;

            while (currentId != Guid.Empty && depth < maxDepth)
            {
                var result = await unitOfWork.Departments.GetByIdAsync(currentId);
                if (result.IsError || result.Value == null || !result.Value.ParentDepartmentId.HasValue)
                    break;

                if (result.Value.Id == departmentId)
                    return false;

                currentId = result.Value.ParentDepartmentId.Value;
                depth++;
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking parent assignment for department {DepartmentId}", departmentId);
            return true;
        }
    }

    private async Task BuildTreeRecursive(Guid parentId, DepartmentTreeDto parentNode)
    {
        var children = await unitOfWork.Departments.GetAllAsync(d => d.ParentDepartmentId == parentId, i => i.Include(x => x.DepartmentHead).Include(x => x.ChildDepartments));
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
            DepartmentHeadId = department.DepartmentHeadId ?? Guid.Empty,
            DepartmentHeadName = department.DepartmentHead?.UserName,

            Level = department.Level,
            MaterializedPath = department.MaterializedPath,
            Type = department.Type,
            ChildrenCount = department.ChildDepartments?.Count ?? 0,
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
            DepartmentHeadName = department.DepartmentHead?.UserName,
            Level = department.Level,
            Type = department.Type,
            ChildrenCount = department.ChildDepartments?.Count ?? 0,
            Children = new List<DepartmentTreeDto>()
        };
    }


    private static string GetShortId(Guid id)
    {
        return id.ToString("N")[..8];
    }

}
