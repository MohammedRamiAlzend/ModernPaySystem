using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using ModernPaySystem.Domain.Entities.Abstraction;
using ModernPaySystem.Domain.Entities.TransactionSystemEntities;

namespace ModernPaySystem.Domain.Entities.SharedEntities;

public class Attachment : Entity<Guid>, IAuditableEntity
{
    public required string FileName { get; set; }
    public required string SafeName { get; set; }
    public required string Extension { get; set; }
    public required string Path { get; set; }

    // Navigation property
    public ICollection<RequestAttachment> RequestAttachments { get; set; } = new List<RequestAttachment>();

    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
