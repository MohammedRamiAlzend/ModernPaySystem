using ModernPaySystem.Domain.DTOs;

namespace ModernPaySystem.Application.Interfaces;

/// <summary>
/// Interface for Department service operations
/// </summary>
public interface IDepartmentService
{
    // CRUD Operations
    
    /// <summary>
    /// Create a new department
    /// </summary>
    Task<Result<DepartmentDto>> CreateAsync(CreateDepartmentDto dto, string userId);
    
    /// <summary>
    /// Get department by ID
    /// </summary>
    Task<Result<DepartmentDto?>> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Update an existing department
    /// </summary>
    Task<Result<DepartmentDto>> UpdateAsync(Guid id, UpdateDepartmentDto dto, string userId);
    
    /// <summary>
    /// Delete a department
    /// </summary>
    Task<Result<bool>> DeleteAsync(Guid id);
    
    // Tree Operations
    
    /// <summary>
    /// Get the full department tree
    /// </summary>
    Task<Result<List<DepartmentTreeDto>>> GetTreeAsync();
    
    /// <summary>
    /// Get a subtree starting from a specific department
    /// </summary>
    Task<Result<List<DepartmentTreeDto>>> GetSubTreeAsync(Guid departmentId);
    
    /// <summary>
    /// Get direct children of a department
    /// </summary>
    Task<Result<List<DepartmentDto>>> GetChildrenAsync(Guid departmentId);
    
    /// <summary>
    /// Get parent department
    /// </summary>
    Task<Result<DepartmentDto?>> GetParentAsync(Guid departmentId);
    
    // Search and Query
    
    /// <summary>
    /// Search departments by name or code
    /// </summary>
    Task<Result<List<DepartmentDto>>> SearchAsync(string searchTerm, int level = 0);
    
    /// <summary>
    /// Get departments by level
    /// </summary>
    Task<Result<List<DepartmentDto>>> GetByLevelAsync(int level);
    
    /// <summary>
    /// Get the path from a department to root
    /// </summary>
    Task<Result<List<DepartmentDto>>> GetPathToRootAsync(Guid departmentId);
    
    // User Management
    
    /// <summary>
    /// Get users in a department
    /// </summary>
    Task<Result<List<UserDto>>> GetUsersInDepartmentAsync(Guid departmentId, bool includeSubDepartments = false);
    
    /// <summary>
    /// Assign a user to a department
    /// </summary>
    Task<Result<bool>> AssignUserToDepartmentAsync(Guid userId, Guid departmentId);
    
    /// <summary>
    /// Remove a user from a department
    /// </summary>
    Task<Result<bool>> RemoveUserFromDepartmentAsync(Guid userId);
    
    // Validation
    
    /// <summary>
    /// Check if a parent assignment is valid (prevents circular references)
    /// </summary>
    bool CanAssignParent(Guid departmentId, Guid parentDepartmentId);
}
