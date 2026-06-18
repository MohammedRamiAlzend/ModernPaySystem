using ModernPaySystem.Domain.Entities.Abstraction;

namespace ModernPaySystem.Domain.Entities.Archiving;

public class FolderIcon : Entity<Guid>, IAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string SvgContent { get; set; } = string.Empty;
    public bool IsDefault { get; set; }

    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Folder> Folders { get; set; } = [];
}

public class FolderIconDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SvgContent { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CreateFolderIconDto
{
    public string Name { get; set; } = string.Empty;
    public string SvgContent { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}

public class UpdateFolderIconDto
{
    public string? Name { get; set; }
    public string? SvgContent { get; set; }
    public bool? IsDefault { get; set; }
}

public class AssignFolderIconDto
{
    public Guid FolderId { get; set; }
    public Guid? IconId { get; set; }
}
