namespace ModernPaySystem.Domain.DTOs;

/// <summary>
/// DTO for creating a new department
/// </summary>
public class CreateDepartmentDto
{
    public required string Name { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public DepartmentType Type { get; set; }
}

/// <summary>
/// DTO for updating an existing department
/// </summary>
public class UpdateDepartmentDto
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public DepartmentType? Type { get; set; }
}

/// <summary>
/// DTO for displaying department information
/// </summary>
public class DepartmentDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public string? ParentDepartmentName { get; set; }
    public int Level { get; set; }
    public string? MaterializedPath { get; set; }
    public DepartmentType Type { get; set; }
    public int ChildrenCount { get; set; }
    public int UsersCount { get; set; }
    public DateTime? CreatedAt { get; set; }
}

/// <summary>
/// DTO for displaying department tree structure
/// </summary>
public class DepartmentTreeDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Code { get; set; }
    public int Level { get; set; }
    public DepartmentType Type { get; set; }
    public List<DepartmentTreeDto> Children { get; set; } = new();
}
