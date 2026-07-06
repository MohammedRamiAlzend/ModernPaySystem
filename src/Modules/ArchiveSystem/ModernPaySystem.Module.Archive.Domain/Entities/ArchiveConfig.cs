using System.Linq;
using ModernPaySystem.SharedKernel.Domain.Entities.Abstraction;

namespace ModernPaySystem.Module.Archive.Domain.Entities;

public class ArchiveConfig : Entity<Guid>
{
    public string DefaultPath { get; set; } = "Uploads";
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public string? AllowedFileExtensions { get; set; }

    public ArchiveConfigDto ToDto()
    {
        return new ArchiveConfigDto
        {
            Id = Id,
            DefaultPath = DefaultPath,
            Description = Description,
            IsActive = IsActive,
            AllowedFileExtensions = AllowedFileExtensions
        };
    }

    public string[] GetAllowedExtensionsArray()
    {
        if (string.IsNullOrWhiteSpace(AllowedFileExtensions))
            return [];

        return AllowedFileExtensions
            .Split(',')
            .Select(e => e.Trim().ToLowerInvariant())
            .Where(e => !string.IsNullOrEmpty(e))
            .ToArray();
    }
}

public class ArchiveConfigDto
{
    public Guid Id { get; set; }
    public string DefaultPath { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public string? AllowedFileExtensions { get; set; }
}

public class UpdateArchiveConfigDto
{
    public string DefaultPath { get; set; } = "Uploads";
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public string? AllowedFileExtensions { get; set; }
}
