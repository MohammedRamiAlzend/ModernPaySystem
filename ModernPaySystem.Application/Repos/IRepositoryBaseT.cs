global using System.Linq;

namespace ModernPaySystem.Application.Repos;

public interface IRepositoryBase<TEntity, TKey>
    where TEntity : Entity<TKey>
{
    Task<Result<Success>> AddAsync(TEntity entity, bool bypassAuth = false);

    Task<Result<List<TEntity>>> GetAllAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? transform = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool bypassAuth = false);

    Task<Result<List<TEntity>>> FindAsync(
        Expression<Func<TEntity, bool>> filter,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? transform = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool bypassAuth = false);

    Task<Result<PagedList<TEntity>>> GetPagedAsync(
        int page,
        int pageSize,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? transform = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool bypassAuth = false);

    Task<Result<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? transform = null,
        bool bypassAuth = false);

    Task<Result<Deleted>> RemoveAsync(Expression<Func<TEntity, bool>> filter, bool bypassAuth = false);

    Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> filter,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? transform = null);

    Task<Result<Updated>> UpdateAsync(TEntity entity, bool bypassAuth = false);

    Task<Result<TEntity?>> GetByIdAsync(TKey id, bool bypassAuth = false);
}
