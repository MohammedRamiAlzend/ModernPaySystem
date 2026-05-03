using ModernPaySystem.Domain.DTOs;

namespace ModernPaySystem.Domain.Entities.SharedEntities;

/// <summary>
/// Represents a department in the organizational hierarchy
/// </summary>
public class Department : Entity<Guid>, IAuditableEntity
{
    // Basic Information
    public required string Name { get; set; }           // Department name (e.g., "Technical Office Diwan")
    public string? Code { get; set; }                   // Department code for unique identification
    public string? Description { get; set; }            // Department description

    // Tree Relationships
    public Guid? ParentDepartmentId { get; set; }       // Parent department ID
    public Department? ParentDepartment { get; set; }   // Reference to parent department
    public ICollection<Department> ChildDepartments { get; set; } = new List<Department>(); // Child departments

    // Hierarchy Level (1: Country, 2: Governorate, 3: District, 4: Municipality, 5: Office, ...)
    public int Level { get; set; }

    // Full materialized path for hierarchical sequence (e.g., "1/5/12/45/78")
    public string? MaterializedPath { get; set; }

    // Department type (for validation)
    public DepartmentType Type { get; set; }

    // Users belonging to this department
    public ICollection<User> Users { get; set; } = new List<User>();

    // Audit Data
    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Maps the department entity to a DTO
    /// </summary>
    public DepartmentDto MapToDto()
    {
        return new DepartmentDto
        {
            Id = this.Id,
            Name = this.Name,
            Code = this.Code,
            Description = this.Description,
            ParentDepartmentId = this.ParentDepartmentId,
            Level = this.Level,
            MaterializedPath = this.MaterializedPath,
            Type = this.Type,
            ChildrenCount = 0, // Would need to query for this
            UsersCount = this.Users?.Count ?? 0,
            CreatedAt = this.CreatedAt
        };
    }
}

/// <summary>
/// Department types representing organizational levels
/// </summary>
public enum DepartmentType
{
    Country = 1,        // State (Syria)
    Governorate = 2,    // Governorate (Rif Dimashq)
    District = 3,       // District (Ghouta Eastern)
    Municipality = 4,   // Municipality (Douma)
    Office = 5,         // Office (Technical Office Diwan)
    Unit = 6            // Other administrative unit
}
