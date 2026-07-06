using ModernPaySystem.SharedKernel.Domain.Entities.Abstraction;

namespace ModernPaySystem.Module.Archive.Domain.Entities;

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

    public FolderPermissionDto ToDto()
    {
        return new FolderPermissionDto
        {
            Id = Id,
            FolderId = FolderId,
            UserId = UserId,
            AccessLevel = AccessLevel,
            IsInherited = IsInherited,
            CreatedByUserId = CreatedByUserId,
            CreatedAt = CreatedAt,
            UpdatedByUserId = UpdatedByUserId,
            UpdatedAt = UpdatedAt
        };
    }
}

public class FolderPermissionDto
{
    public Guid Id { get; set; }
    public Guid FolderId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public AccessLevel AccessLevel { get; set; }
    public bool IsInherited { get; set; }
    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateFolderPermissionDto
{
    public Guid FolderId { get; set; }
    public Guid UserId { get; set; }
    public AccessLevel AccessLevel { get; set; }
    public bool IsInherited { get; set; } = true;
}

public class UpdateFolderPermissionDto
{
    public AccessLevel AccessLevel { get; set; }
    public bool IsInherited { get; set; } = true;
}
