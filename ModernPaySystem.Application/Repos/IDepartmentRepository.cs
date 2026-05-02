using ModernPaySystem.Domain.Entities.SharedEntities;

namespace ModernPaySystem.Application.Repos;

/// <summary>
/// Repository interface for Department entity
/// </summary>
public interface IDepartmentRepository : IRepositoryBase<Department, Guid>
{
    /// <summary>
    /// Get all root departments (no parent)
    /// </summary>
    Task<Result<List<Department>>> GetRootDepartmentsAsync();
    
    /// <summary>
    /// Get children of a specific department
    /// </summary>
    Task<Result<List<Department>>> GetChildrenAsync(Guid parentId);
    
    /// <summary>
    /// Get department with its full path to root
    /// </summary>
    Task<Result<List<Department>>> GetPathToRootAsync(Guid departmentId);
    
    /// <summary>
    /// Get all descendants of a department (recursive)
    /// </summary>
    Task<Result<List<Department>>> GetAllDescendantsAsync(Guid departmentId);
    
    /// <summary>
    /// Check if a department has any children
    /// </summary>
    Task<bool> HasChildrenAsync(Guid departmentId);
    
    /// <summary>
    /// Check if assigning a parent would create a circular reference
    /// </summary>
    Task<bool> WouldCreateCircularReferenceAsync(Guid departmentId, Guid parentDepartmentId);
}
