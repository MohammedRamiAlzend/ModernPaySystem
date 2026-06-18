using ModernPaySystem.Domain.Entities.Abstraction;

namespace ModernPaySystem.Domain.Entities.Archiving;

public class ArchiveConfig : Entity<Guid>
{
    public string DefaultPath { get; set; } = "Uploads";
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ArchiveConfigDto ToDto()
    {
        return new ArchiveConfigDto
        {
            Id = Id,
            DefaultPath = DefaultPath,
            Description = Description,
            IsActive = IsActive
        };
    }
}

public class ArchiveConfigDto
{
    public Guid Id { get; set; }
    public string DefaultPath { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateArchiveConfigDto
{
    public string DefaultPath { get; set; } = "Uploads";
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}