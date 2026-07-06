using ModernPaySystem.SharedKernel.Domain.Entities.Abstraction;

namespace ModernPaySystem.Module.Archive.Domain.Entities;

public class ArchiveRecordFormInputValue : Entity<Guid>
{
    public required string Key { get; set; }
    public string? Value { get; set; }

    public ArchiveRecordFormInputValueDto ToDto()
    {
        return new ArchiveRecordFormInputValueDto
        {
            Key = Key,
            Value = Value
        };
    }
}
