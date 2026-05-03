using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Application.Repos;
using ModernPaySystem.Domain.Commons;
using ModernPaySystem.Domain.Entities.SharedEntities;

namespace ModernPaySystem.Infrastructure.Persistence.Repos;

/// <summary>
/// Extension methods for Department-specific operations
/// These methods are used directly on IRepositoryBase<Department, Guid>
/// </summary>
public static class DepartmentRepositoryExtensions
{
    /// <summary>
    /// Check if a department has any children
    /// </summary>
    public static async Task<bool> HasChildrenAsync(this IRepositoryBase<Department, Guid> repository, Guid departmentId)
    {
        var children = await repository.GetAllAsync(d => d.ParentDepartmentId == departmentId);
        return !children.IsError && children.Value != null && children.Value.Any();
    }

    /// <summary>
    /// Get root departments (those without a parent)
    /// </summary>
    public static async Task<Result<List<Department>>> GetRootDepartmentsAsync(this IRepositoryBase<Department, Guid> repository)
    {
        return await repository.GetAllAsync(d => d.ParentDepartmentId == null);
    }

    /// <summary>
    /// Get direct children of a department
    /// </summary>
    public static async Task<Result<List<Department>>> GetChildrenAsync(this IRepositoryBase<Department, Guid> repository, Guid parentId)
    {
        return await repository.GetAllAsync(d => d.ParentDepartmentId == parentId);
    }

    /// <summary>
    /// Get path from a department to root
    /// </summary>
    public static async Task<Result<List<Department>>> GetPathToRootAsync(this IRepositoryBase<Department, Guid> repository, Guid departmentId)
    {
        var path = new List<Department>();
        var currentId = departmentId;

        while (true)
        {
            var result = await repository.GetByIdAsync(currentId);
            if (result.IsError || result.Value == null)
                break;

            path.Add(result.Value);

            if (!result.Value.ParentDepartmentId.HasValue)
                break;

            currentId = result.Value.ParentDepartmentId.Value;
        }

        path.Reverse();
        return path;
    }

    /// <summary>
    /// Check if assigning a parent would create a circular reference
    /// </summary>
    public static async Task<bool> WouldCreateCircularReferenceAsync(
        this IRepositoryBase<Department, Guid> repository,
        Guid departmentId,
        Guid proposedParentId)
    {
        // Cannot assign self as parent
        if (departmentId == proposedParentId)
            return true;

        // Check if proposed parent is a descendant of the department
        var currentId = proposedParentId;
        var maxDepth = 100; // Prevent infinite loops
        var depth = 0;

        while (currentId != Guid.Empty && depth < maxDepth)
        {
            if (currentId == departmentId)
                return true; // Circular reference detected

            var result = await repository.GetByIdAsync(currentId);
            if (result.IsError || result.Value == null || !result.Value.ParentDepartmentId.HasValue)
                break;

            currentId = result.Value.ParentDepartmentId.Value;
            depth++;
        }

        return false;
    }
}
