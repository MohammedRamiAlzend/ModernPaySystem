using System;

namespace ModernPaySystem.Domain.Entities.Abstraction
{
    public class AuditableEntity<TKey> : Entity<TKey>, IAuditableEntity
    {
        public string? CreatedByUserId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? UpdatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
