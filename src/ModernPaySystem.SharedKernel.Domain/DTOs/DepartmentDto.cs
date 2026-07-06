using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.SharedKernel.Domain.DTOs;

public class CreateDepartmentDto
{
    public required string Name { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public Guid HeadedUserId { get; set; }
    public DepartmentType Type { get; set; }
}

public class UpdateDepartmentDto
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public Guid? HeadedUserId { get; set; }
    public DepartmentType? Type { get; set; }
}

public class DepartmentDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public string? ParentDepartmentName { get; set; }
    public Guid DepartmentHeadId { get; set; }
    public string? DepartmentHeadName { get; set; }
    public int Level { get; set; }
    public string? MaterializedPath { get; set; }
    public DepartmentType Type { get; set; }
    public int ChildrenCount { get; set; }
    public int UsersCount { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class DepartmentTreeDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Code { get; set; }
    public string? DepartmentHeadName { get; set; }
    public int Level { get; set; }
    public DepartmentType Type { get; set; }
    public int ChildrenCount { get; set; }
    public List<DepartmentTreeDto> Children { get; set; } = [];
}
