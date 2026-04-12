using System.Linq.Expressions;
using System.Text.RegularExpressions;
using ExpressionBuilderLib.src.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernPaySystem.Application.Repos;
using ModernPaySystem.Application.Services;
using ModernPaySystem.Domain.Commons;
using ModernPaySystem.Domain.Entities.Abstraction;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Linq;
using ExpressionBuilderLib.src.Core.Enums;

namespace ModernPaySystem.Infrastructure.Persistence.Repos;

public class RepositoryBase<TEntity, TKey>(AppDbContext dbcontext,
    ILogger<RepositoryBase<TEntity, TKey>> logger,
    IHttpContextServiceManager httpContextServiceManager) : IRepositoryBase<TEntity, TKey>
    where TEntity : Entity<TKey>
{
    public async Task<Result<Success>> AddAsync(TEntity entity, bool bypassAuth = false)
    {
        if (entity == null)
            return new Error("00", "Entity cannot be null.", ErrorKind.Failure);
        try
        {
            await dbcontext.AddAsync(entity);
            return Result.Success;
        }
        catch (DbUpdateException dbEx)
        {
            string? fkName = ExtractForeignKeyName(dbEx);

            if (fkName != null)
            {
                logger.LogWarning("Foreign key constraint violation: {FKName}", fkName);
                return new Error("FK_ERROR", $"Foreign key constraint violated: {fkName}", ErrorKind.Failure);
            }

            logger.LogError(dbEx, "Database update error adding {EntityType}", typeof(TEntity).Name);
            return new Error("DB_ERROR", dbEx.Message, ErrorKind.Failure);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unexpected error adding entity of type {EntityType}.", typeof(TEntity).Name);
            return new Error("UNEXPECTED", e.Message, ErrorKind.Failure);
        }
    }

    public async Task<Result<List<TEntity>>> GetAllAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? transform = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool bypassAuth = false,
        List<Expression<Func<TEntity, bool>>>? additionalFilters = null)
    {
        IQueryable<TEntity> query = dbcontext.Set<TEntity>();


        try
        {
            // Combine main filter with additional filters
            var allFilters = new List<Expression<Func<TEntity, bool>>>();
            if (filter != null) allFilters.Add(filter);
            if (additionalFilters != null && additionalFilters.Count > 0)
                allFilters.AddRange(additionalFilters);

            if (allFilters.Count > 0)
            {
                var combinedFilter = ExpressionCombiner.AndAll(allFilters.ToArray());
                query = query.Where(combinedFilter);
            }

            if (transform != null) query = transform(query);

            if (orderBy == null)
            {
                query = query.OrderByDescending(e => e.Id);
            }
            else
            {
                query = orderBy(query);
            }

            return await query.ToListAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving entities of type {EntityType}.", typeof(TEntity).Name);
            return new Error(
                "00",
                $"Something went wrong while retrieving entities of type {typeof(TEntity).Name}. Exception: {e.Message}",
                type: ErrorKind.Failure);
        }
    }

    public async Task<Result<PagedList<TEntity>>> GetPagedAsync(
        int page,
        int pageSize,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? transform = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool bypassAuth = false,
        List<Expression<Func<TEntity, bool>>>? additionalFilters = null,
        LogicalOperator logicalOperator = LogicalOperator.And)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;

        IQueryable<TEntity> query = dbcontext.Set<TEntity>();

        try
        {
            // Combine main filter with additional filters
            var allFilters = new List<Expression<Func<TEntity, bool>>>();
            if (filter != null) allFilters.Add(filter);
            if (additionalFilters != null && additionalFilters.Count > 0)
                allFilters.AddRange(additionalFilters);

            if (allFilters.Count > 0)
            {
                if (logicalOperator == LogicalOperator.Or)
                {
                    var combinedFilter = ExpressionCombiner.OrAll(allFilters.ToArray());
                    query = query.Where(combinedFilter);
                }
                else
                {
                    var combinedFilter = ExpressionCombiner.AndAll(allFilters.ToArray());
                    query = query.Where(combinedFilter);
                }
            }

            if (transform != null) query = transform(query);

            int totalItems = await query.CountAsync();

            if (orderBy == null)
            {
                query = query.OrderByDescending(e => e.Id);
            }
            else
            {
                query = orderBy(query);
            }

            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return PagedList<TEntity>.Create(items, totalItems, page, pageSize);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving paged entities of type {EntityType}.", typeof(TEntity).Name);
            return new Error(
                "00",
                $"Something went wrong while retrieving paged entities of type {typeof(TEntity).Name}. Exception: {e.Message}",
                ErrorKind.Failure);
        }
    }

    public async Task<Result<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool bypassAuth = false,
        List<Expression<Func<TEntity, bool>>>? additionalFilters = null)
    {
        IQueryable<TEntity> query = dbcontext.Set<TEntity>();

        try
        {
            // Combine main filter with additional filters
            var allFilters = new List<Expression<Func<TEntity, bool>>>();
            if (filter != null) allFilters.Add(filter);
            if (additionalFilters != null && additionalFilters.Count > 0)
                allFilters.AddRange(additionalFilters);

            if (allFilters.Count > 0)
            {
                var combinedFilter = ExpressionCombiner.AndAll(allFilters.ToArray());
                query = query.Where(combinedFilter);
            }

            if (include != null) query = include(query);

            var result = await query.FirstOrDefaultAsync();

            if (result == null)
                return new Error("404", "not found", ErrorKind.NotFound);
            else
                return result;

        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving entity of type {EntityType}.", typeof(TEntity).Name);
            return new Error(
                "00",
                $"Something went wrong while retrieving an entity of type {typeof(TEntity).Name}. Exception: {e.Message}",
                ErrorKind.Failure);
        }
    }

    public async Task<Result<Deleted>> RemoveAsync(Expression<Func<TEntity, bool>> filter, bool bypassAuth = false)
    {
        if (filter == null) return new Error("00", "Filter cannot be null.", ErrorKind.Failure);

        try
        {
            var getEntity = await GetAsync(filter, bypassAuth: true);
            if (getEntity.IsError) return getEntity.Errors;
            var entity = getEntity.Value;
            if (entity == null) return new Error("00", "Entity not found.", ErrorKind.Failure);

            dbcontext.Set<TEntity>().Remove(entity);
            return Result.Deleted;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error removing entity of type {EntityType}.", typeof(TEntity).Name);
            return new Error(
                "00",
                $"Something went wrong while removing entity of type {typeof(TEntity).Name}. Exception: {e.Message}",
                ErrorKind.Failure);
        }
    }

    public async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> filter,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? transform = null,
        List<Expression<Func<TEntity, bool>>>? additionalFilters = null)
    {
        try
        {
            IQueryable<TEntity> query = dbcontext.Set<TEntity>();
            if (transform != null) query = transform(query);

            // Combine main filter with additional filters
            var allFilters = new List<Expression<Func<TEntity, bool>>> { filter };
            if (additionalFilters != null && additionalFilters.Count > 0)
                allFilters.AddRange(additionalFilters);

            var combinedFilter = ExpressionCombiner.AndAll(allFilters.ToArray());
            return await query.AnyAsync(combinedFilter);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error checking existence of entities of type {EntityType}.", typeof(TEntity).Name);
            return false;
        }
    }

    public async Task<Result<Updated>> UpdateAsync(TEntity entity, bool bypassAuth = false)
    {
        if (entity == null) return new Error("00", "Entity cannot be null.", ErrorKind.Failure);
        try
        {
            //if (!bypassAuth && !entity.CanEdit(httpContextServiceManager.GetCurrentUserId()))
            //    return new Error("403", "You do not have permission to edit this entity.", ErrorKind.Failure);

            dbcontext.Attach(entity);
            dbcontext.Set<TEntity>().Entry(entity).State = EntityState.Modified;
            int saveResult = await dbcontext.SaveChangesAsync();
            return saveResult > 0 ? Result.Updated : new Error("00", "Failed to update entity.", ErrorKind.Failure);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error updating entity of type {EntityType}.", typeof(TEntity).Name);
            return new Error(
                "00",
                $"Something went wrong while updating entity of type {typeof(TEntity).Name}. Exception: {e.Message}",
                ErrorKind.Failure);
        }
    }

    public async Task<Result<TEntity?>> GetByIdAsync(TKey id, bool bypassAuth = false)
    {
        try
        {
            var entity = await dbcontext.Set<TEntity>().FindAsync(id);

            //if (entity != null && !bypassAuth && !entity.CanView(httpContextServiceManager.GetCurrentUserId()))
            //    return new Error("403", "You do not have permission to view this entity.", ErrorKind.Failure);

            return entity;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving entity of type {EntityType} with id {Id}.", typeof(TEntity).Name, id);
            return new Error(
                "00",
                $"Something went wrong while retrieving entity of type {typeof(TEntity).Name} with id {id}. Exception: {e.Message}",
                ErrorKind.Failure);
        }
    }

    public async Task<Result<List<TEntity>>> FindAsync(
        Expression<Func<TEntity, bool>> filter,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? transform = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool bypassAuth = false,
        List<Expression<Func<TEntity, bool>>>? additionalFilters = null)
    {
        IQueryable<TEntity> query = dbcontext.Set<TEntity>();

        try
        {
            // Combine main filter with additional filters
            var allFilters = new List<Expression<Func<TEntity, bool>>> { filter };
            if (additionalFilters != null && additionalFilters.Count > 0)
                allFilters.AddRange(additionalFilters);

            var combinedFilter = ExpressionCombiner.AndAll(allFilters.ToArray());
            query = query.Where(combinedFilter);

            if (transform != null) query = transform(query);

            if (orderBy == null)
            {
                query = query.OrderByDescending(e => e.Id);
            }
            else
            {
                query = orderBy(query);
            }

            return await query.ToListAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error finding entities of type {EntityType}.", typeof(TEntity).Name);
            return new Error(
                "00",
                $"Something went wrong while finding entities of type {typeof(TEntity).Name}. Exception: {e.Message}",
                type: ErrorKind.Failure);
        }
    }

    private static string? ExtractForeignKeyName(DbUpdateException ex)
    {
        string message = ex.InnerException?.Message ?? ex.Message;

        var match = Regex.Match(message, @"FOREIGN KEY constraint ""?(?<fk>FK_[\w\d_]+)""?", RegexOptions.IgnoreCase);
        if (match.Success)
            return match.Groups["fk"].Value;

        match = Regex.Match(message, @"violates foreign key constraint ""?(?<fk>FK_[\w\d_]+)""?", RegexOptions.IgnoreCase);
        if (match.Success)
            return match.Groups["fk"].Value;

        match = Regex.Match(message, @"CONSTRAINT [`""]?(?<fk>FK_[\w\d_]+)[`""]? FOREIGN KEY", RegexOptions.IgnoreCase);
        if (match.Success)
            return match.Groups["fk"].Value;

        return null;
    }

}
