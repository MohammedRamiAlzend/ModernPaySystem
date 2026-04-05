using System.ComponentModel.DataAnnotations;

namespace ModernPaySystem.Domain.Entities.Abstraction;

public class Entity<TKey>
{
    [Key]
    public virtual TKey Id { get; set; }
}
