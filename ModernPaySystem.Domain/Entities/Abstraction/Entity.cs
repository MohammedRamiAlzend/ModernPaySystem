using System.ComponentModel.DataAnnotations;

namespace ModernPaySystem.Domain.Entities.Abstraction;

public class Entity<TKey>
{
    [Key]
    public virtual TKey Id { get; set; }
    public virtual bool CanEdit(string userId) => true;
    public virtual bool CanView(string userId) => true;
}
