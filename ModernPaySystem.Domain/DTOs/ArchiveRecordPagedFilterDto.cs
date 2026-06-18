namespace ModernPaySystem.Domain.DTOs;

public class ArchiveRecordPagedFilterDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchText { get; set; }
    public string? RecordId { get; set; }
    public List<InputValueFilterDto>? InputValueFilters { get; set; }
    public FilterLogicalOperator LogicalOperator { get; set; } = FilterLogicalOperator.And;
}
