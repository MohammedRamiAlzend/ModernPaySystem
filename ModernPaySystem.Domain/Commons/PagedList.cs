namespace ModernPaySystem.Domain.Commons;

public class PagedList<TData>
{
    public IEnumerable<TData> Items { get; set; } = default!;
    public int TotalItems { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);

    public PagedList(IEnumerable<TData> items, int totalItems, int page, int pageSize)
    {
        Items = items;
        TotalItems = totalItems;
        Page = page;
        PageSize = pageSize;
    }

    public static PagedList<TData> Create(IEnumerable<TData> items, int totalItems, int page, int pageSize)
    {
        return new PagedList<TData>(items, totalItems, page, pageSize);
    }
}
