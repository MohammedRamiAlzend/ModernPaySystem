# Query Building — ModernPaySystem

## Dynamic Filtering with ExpressionBuilderLib

Services build `List<Expression<Func<T, bool>>>` filters and pass them to `RepositoryBase<T, TKey>` methods. The repository combines filters using `ExpressionCombiner.AndAll` or `OrAll` from ExpressionBuilderLib and pushes the combined expression to EF Core's `Where()` clause — ensuring filtering happens in SQL, not in memory.

### Pattern: Push filters to EF Core

```csharp
public async Task<Result<PagedList<ResponseDto>>> SearchAsync(FilterDto filterDto)
{
    List<Expression<Func<Response, bool>>> filters = [];

    if (filterDto.FromDate.HasValue)
        filters.Add(r => r.CreatedAt >= filterDto.FromDate);

    if (filterDto.ToDate.HasValue)
        filters.Add(r => r.CreatedAt <= filterDto.ToDate);

    if (!string.IsNullOrWhiteSpace(filterDto.SearchTerm))
        filters.Add(r => r.Name.Contains(filterDto.SearchTerm));

    var result = await unitOfWork.Responses.GetPagedAsync(
        page, pageSize,
        additionalFilters: filters);
    // ...
}
```

### Filter Combination Logic

`RepositoryBase` combines the main `filter` parameter with `additionalFilters`:

```csharp
var allFilters = new List<Expression<Func<TEntity, bool>>>();
if (filter != null) allFilters.Add(filter);
if (additionalFilters?.Count > 0)
    allFilters.AddRange(additionalFilters);

if (allFilters.Count > 0)
{
    var combinedFilter = logicalOperator == LogicalOperator.Or
        ? ExpressionCombiner.OrAll(allFilters.ToArray())
        : ExpressionCombiner.AndAll(allFilters.ToArray());
    query = query.Where(combinedFilter);
}
```

### Logical Operators

- `LogicalOperator.And` — all filters must match (default)
- `LogicalOperator.Or` — any filter can match

### Available Repository Methods

| Method | Supports `additionalFilters` | Supports `LogicalOperator` |
|--------|------------------------------|----------------------------|
| `GetAllAsync` | Yes | And only |
| `GetPagedAsync` | Yes | Yes |
| `GetPagedProjectedAsync` | Yes | Yes |
| `GetAsync` | Yes | And only |
| `FindAsync` | Yes | And only |
| `AnyAsync` | Yes | And only |

## Anti-Pattern: In-Memory Filtering

❌ **BAD** — loads all rows, filters in memory:

```csharp
var all = await unitOfWork.Responses.GetAllAsync();
var filtered = all.Where(x => x.Name.Contains(term)).ToList();
```

This loads the entire table into memory, causing performance degradation as data grows.

✅ **GOOD** — pushes filter to SQL:

```csharp
var result = await unitOfWork.Responses.GetAllAsync(
    filter: x => x.Name.Contains(term));
```

Or with additional dynamic filters:

```csharp
var result = await unitOfWork.Responses.GetPagedAsync(
    page, pageSize,
    additionalFilters: filters);
```

## ExpressionBuilderLib API

- `ExpressionCombiner.AndAll(params Expression<Func<T, bool>>[] filters)` — combines with `&&`
- `ExpressionCombiner.OrAll(params Expression<Func<T, bool>>[] filters)` — combines with `||`
