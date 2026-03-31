using System;
using System.Collections.Generic;
using System.Text;

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

    // Auditable properties
    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}


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

public class KindShip : Entity<Guid>
{
    public string Desc { get; set; } = string.Empty;
}

public class OperationStatus : Entity<Guid>
{
    public string Desc { get; set; } = string.Empty;
}

public class OperationServiceType : Entity<Guid>
{
    public string Desc { get; set; } = string.Empty;
}

public class Gender : Entity<Guid>
{
    public string Desc { get; set; } = string.Empty;
}

public class National : Entity<Guid>
{
    public string Desc { get; set; } = string.Empty;
}
public class Gov : Entity<Guid>
{
    public string Desc { get; set; } = string.Empty;
}