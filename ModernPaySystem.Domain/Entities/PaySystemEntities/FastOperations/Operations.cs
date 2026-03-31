namespace ModernPaySystem.Domain.Entities.PaySystemEntities.FastOperations;

public class Operation : Entity<Guid>, IAuditableEntity
{

    public Guid ApplicantClientId { get; set; }
    public Client ApplicantClient { get; set; }

    public Guid RecipientClientId { get; set; }
    public Client RecipientClient { get; set; }

    public Guid KindShipId { get; set; }
    public KindShip KindShip { get; set; }

    public Guid OperationStatusId { get; set; }
    public OperationStatus Status { get; set; }

    public Guid OperationServiceTypeId { get; set; }
    public OperationServiceType OperationServiceType { get; set; }

    public decimal SumAmount { get; set; }
    public string Notes { get; set; } = string.Empty;

    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}