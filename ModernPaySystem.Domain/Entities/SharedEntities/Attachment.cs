using ModernPaySystem.Domain.Entities.TransactionSystemEntities;

namespace ModernPaySystem.Domain.Entities.SharedEntities;

public class Attachment : Entity<Guid>, IAuditableEntity
{
    public required string FileName { get; set; }
    public required string SafeName { get; set; }
    public required string Extension { get; set; }
    public required string Path { get; set; }

    // Navigation properties
    public ICollection<RequestAttachment> RequestAttachments { get; set; } = new List<RequestAttachment>();
    public ICollection<ResponseAttachment> ResponseAttachments { get; set; } = new List<ResponseAttachment>();

    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public AttachmentDto ToDto()
    {
        return new AttachmentDto
        {
            Id = this.Id,
            FileName = this.FileName,
            SafeName = this.SafeName,
            Extension = this.Extension,
            Path = this.Path,
            CreatedByUserId = this.CreatedByUserId,
            CreatedAt = this.CreatedAt,
            UpdatedByUserId = this.UpdatedByUserId,
            UpdatedAt = this.UpdatedAt
        };
    }
}

public class AttachmentDto
{
    public Guid Id { get; set; }
    public required string FileName { get; set; }
    public required string SafeName { get; set; }
    public required string Extension { get; set; }
    public required string Path { get; set; }
    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateAttachmentDto
{
    public required string FileName { get; set; }
    public required string SafeName { get; set; }
    public required string Extension { get; set; }
    public required string Path { get; set; }
}

public class UpdateAttachmentDto
{
    public required string FileName { get; set; }
    public required string SafeName { get; set; }
    public required string Extension { get; set; }
    public required string Path { get; set; }
}
