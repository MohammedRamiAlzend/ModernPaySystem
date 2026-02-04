using System;

namespace ModernPaySystem.Domain.Entities.Abstraction
{
    public interface IAuditableEntity
    {
        string? CreatedByUserId { get; set; }
        DateTime? CreatedAt { get; set; }
        string? UpdatedByUserId { get; set; }
        DateTime? UpdatedAt { get; set; }
    }
}
