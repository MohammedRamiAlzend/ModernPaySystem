using ModernPaySystem.Domain.Entities.Abstraction;
using System.Linq;

namespace ModernPaySystem.Domain.Entities.Archiving;

public class Folder : Entity<Guid>, IAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? DefaultStoragePath { get; set; }
    public int Level { get; set; }
    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public Guid? ParentId { get; set; }
    public Folder? Parent { get; set; }

    public ICollection<Folder> SubFolders { get; set; } = [];
    public ICollection<ArchiveRecord> ArchiveRecords { get; set; } = [];
    public ICollection<FolderPermission> Permissions { get; set; } = [];

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedByUserId { get; set; }
    public Guid? DeletedByRequestId { get; set; }

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
            DefaultStoragePath = DefaultStoragePath,
            Level = Level,
            DepartmentId = DepartmentId,
            ParentId = ParentId,
            FolderDtos = SubFolders is not null && SubFolders.Count != 0 ? [.. SubFolders.Where(x => x.Level == Level + 1).Select(f => f.ToDto())] : [],
            IsDeleted = IsDeleted,
            DeletedAt = DeletedAt,
            DeletedByUserId = DeletedByUserId,
            DeletedByRequestId = DeletedByRequestId,
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
    public string? DefaultStoragePath { get; set; }
    public int Level { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? ParentId { get; set; }

    public required List<FolderDto> FolderDtos { get; set; } = [];
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedByUserId { get; set; }
    public Guid? DeletedByRequestId { get; set; }

    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public bool CanManagePermissions { get; set; }
}

public class CreateFolderDto
{
    public string Name { get; set; } = string.Empty;
    public string? DefaultStoragePath { get; set; }
    public Guid? ParentId { get; set; }
    public Guid? DepartmentId { get; set; }
    public List<InitialFolderPermissionDto> InitialPermissions { get; set; } = [];
}

public class InitialFolderPermissionDto
{
    public Guid UserId { get; set; }
    public AccessLevel AccessLevel { get; set; } = AccessLevel.View;
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
