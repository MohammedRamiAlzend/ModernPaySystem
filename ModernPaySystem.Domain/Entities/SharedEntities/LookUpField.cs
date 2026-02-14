namespace ModernPaySystem.Domain.Entities.SharedEntities;

public class LookUpField : Entity<Guid>
{
    public required string FiledName { get; set; }

    // Navigation property for LookUpFiledValues
    public ICollection<LookUpFiledValues> LookUpFiledValues { get; set; } = [];

    public LookUpFieldDto ToDto()
    {
        return new LookUpFieldDto
        {
            Id = this.Id,
            FiledName = this.FiledName
        };
    }
}

public class LookUpFiledValues : Entity<Guid>
{
    public Guid LookUpFiledId { get; set; }
    public LookUpField LookUpFiled { get; set; }

    public required string Desc { get; set; }

    public LookUpFiledValuesDto ToDto()
    {
        return new LookUpFiledValuesDto
        {
            Id = this.Id,
            LookUpFiledId = this.LookUpFiledId,
            Desc = this.Desc
        };
    }
}

public class LookUpFieldDto
{
    public Guid Id { get; set; }
    public required string FiledName { get; set; }
}

public class CreateLookUpFieldDto
{
    public required string FiledName { get; set; }
}

public class UpdateLookUpFieldDto
{
    public required string FiledName { get; set; }
}

public class LookUpFiledValuesDto
{
    public Guid Id { get; set; }
    public Guid LookUpFiledId { get; set; }
    public required string Desc { get; set; }
}

public class CreateLookUpFiledValuesDto
{
    public Guid LookUpFiledId { get; set; }
    public required string Desc { get; set; }
}

public class UpdateLookUpFiledValuesDto
{
    public Guid LookUpFiledId { get; set; }
    public required string Desc { get; set; }
}
