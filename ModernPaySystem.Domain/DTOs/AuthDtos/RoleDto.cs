namespace ModernPaySystem.Domain.DTOs.AuthDtos;

public class RoleDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}
