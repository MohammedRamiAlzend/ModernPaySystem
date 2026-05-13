namespace ModernPaySystem.Domain.DTOs;

public enum FilterLogicalOperator
{
    And = 1,
    Or = 2
}

public class InputValueFilterDto
{
    public string Key { get; set; } = null!;
    public string? Value { get; set; }
}

public class RequestPagedFilterDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public List<InputValueFilterDto>? InputValueFilters { get; set; }
    public FilterLogicalOperator LogicalOperator { get; set; } = FilterLogicalOperator.And;
}
