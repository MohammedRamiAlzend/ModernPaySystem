global using System.Linq;
using ExpressionBuilderLib.src.Core.Enums;

namespace ModernPaySystem.Application.Repos;

public interface IRepositoryBase<TEntity, TKey>
    where TEntity : Entity<TKey>
{
    Task<Result<Success>> AddAsync(TEntity entity, bool bypassAuth = false);

    Task<Result<List<TEntity>>> GetAllAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? transform = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool bypassAuth = false,
        List<Expression<Func<TEntity, bool>>>? additionalFilters = null);

    Task<Result<List<TEntity>>> FindAsync(
        Expression<Func<TEntity, bool>> filter,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? transform = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool bypassAuth = false,
        List<Expression<Func<TEntity, bool>>>? additionalFilters = null);

    Task<Result<PagedList<TEntity>>> GetPagedAsync(
        int page,
        int pageSize,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? transform = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool bypassAuth = false,
        List<Expression<Func<TEntity, bool>>>? additionalFilters = null,
        LogicalOperator logicalOperator = LogicalOperator.And);

    Task<Result<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? transform = null,
        bool bypassAuth = false,
        List<Expression<Func<TEntity, bool>>>? additionalFilters = null);

    Task<Result<Deleted>> RemoveAsync(Expression<Func<TEntity, bool>> filter, bool bypassAuth = false);

    Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> filter,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? transform = null,
        List<Expression<Func<TEntity, bool>>>? additionalFilters = null);

    Task<Result<Updated>> UpdateAsync(TEntity entity, bool bypassAuth = false);

    Task<Result<TEntity?>> GetByIdAsync(TKey id, bool bypassAuth = false);
}
