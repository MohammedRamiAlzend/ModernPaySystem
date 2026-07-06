using System.ComponentModel.DataAnnotations;

namespace ModernPaySystem.SharedKernel.Domain.Entities.Abstraction;

public class Entity<TKey>
{
    [Key]
    public virtual TKey Id { get; set; }
}
