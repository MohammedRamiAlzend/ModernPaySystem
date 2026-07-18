# Pagination — ModernPaySystem

## PagedList<T>

All paginated queries return `PagedList<T>` wrapped in `Result<T>`:

```csharp
public class PagedList<T>
{
    public List<T> Items { get; }
    public int TotalItems { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalPages { get; }
    public bool HasPreviousPage { get; }
    public bool HasNextPage { get; }
}
```

## GetPagedAsync

Use `IRepositoryBase.GetPagedAsync` which applies pagination at the SQL level:

```csharp
var result = await unitOfWork.Requests.GetPagedAsync(
    page: 1,
    pageSize: 20,
    filter: r => r.Status == RequestStatus.Pending,
    transform: q => q.Include(r => r.Requester),
    additionalFilters: filters);
```

## Filter DTOs

Use `RequestPagedFilterDto` or `ArchiveRecordPagedFilterDto` from `Domain.DTOs`:

```csharp
public class RequestPagedFilterDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public List<InputValueFilter>? InputValueFilters { get; set; }
}
```
