using ModernPaySystem.Domain.Entities.Abstraction;
using System.Linq;

namespace ModernPaySystem.Domain.Entities.Archiving;

public class Folder : Entity<Guid>, IAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }

    public Guid? ParentId { get; set; }
    public Folder? Parent { get; set; }

    public ICollection<Folder> SubFolders { get; set; } = [];
    public ICollection<ArchiveRecord> ArchiveRecords { get; set; } = [];
    public ICollection<FolderPermission> Permissions { get; set; } = [];

    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public FolderDto ToDto()
    {
        return new FolderDto
        {
            Id = Id,
            Name = Name,
            Level = Level,
            ParentId = ParentId,
            FolderDtos = SubFolders is not null && SubFolders.Count != 0 ? [.. SubFolders.Where(x => x.Level == Level + 1).Select(f => f.ToDto())] : [],
            CreatedByUserId = CreatedByUserId,
            CreatedAt = CreatedAt,
            UpdatedByUserId = UpdatedByUserId,
            UpdatedAt = UpdatedAt
        };
    }
}

public class FolderDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public Guid? ParentId { get; set; }

    public required List<FolderDto> FolderDtos { get; set; } = [];

    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateFolderDto
{
    public string Name { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
}

public class UpdateFolderDto
{
    public string Name { get; set; } = string.Empty;
}

public class MoveFolderDto
{
    public Guid FolderId { get; set; }
    public Guid DestnationFolderId { get; set; }
}

