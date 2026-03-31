namespace ModernPaySystem.Domain.Entities.PaySystemEntities.FastOperations;

public class Client : Entity<Guid>, IAuditableEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string FatherName { get; set; } = string.Empty;
    public string MotherName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;


    public string PlaceBirth { get; set; } = string.Empty;
    public DateTime DateBirth { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string NationalityNumber { get; set; } = string.Empty;

    public Guid GenderId { get; set; }
    public Gender Gender { get; set; }

    public Guid NationalId { get; set; }
    public National National { get; set; }

    public Guid GovId { get; set; }
    public Gov Gov { get; set; }
    // Auditable properties
    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
