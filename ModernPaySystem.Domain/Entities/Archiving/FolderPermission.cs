using ModernPaySystem.Domain.Entities.Abstraction;

namespace ModernPaySystem.Domain.Entities.Archiving;

public class FolderPermission : Entity<Guid>, IAuditableEntity
{
    public Guid FolderId { get; set; }
    public Folder Folder { get; set; } = default!;

    public string UserId { get; set; } = string.Empty;
    public AccessLevel AccessLevel { get; set; }
    public bool IsInherited { get; set; }

    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
