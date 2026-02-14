using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernPaySystem.Application.Repos;
using ModernPaySystem.Domain.Commons;
using ModernPaySystem.Domain.Entities.Abstraction;
using ModernPaySystem.Infrastructure.Persistence;

namespace ModernPaySystem.Infrastructure.Persistence.Repos;

public class RepositoryBase<TEntity, TKey>(AppDbContext dbcontext, ILogger<RepositoryBase<TEntity, TKey>> logger) : IRepositoryBase<TEntity, TKey>
    where TEntity : Entity<TKey>
{
    public async Task<Result<Success>> AddAsync(TEntity entity)
    {
        if (entity == null)
            return new Error("00", "Entity cannot be null.", ErrorKind.Failure);

        try
        {
            await dbcontext.AddAsync(entity);
            int saveResult = await dbcontext.SaveChangesAsync();

            return saveResult > 0
                ? Result.Success
                : new Error("00", "Failed to save entity.", ErrorKind.Failure);
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
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null)
    {
        IQueryable<TEntity> query = dbcontext.Set<TEntity>();

        try
        {
            if (filter != null) query = query.Where(filter);
            if (transform != null) query = transform(query);

            // If no specific ordering is provided, order by Id descending (newest first, assuming auto-incrementing IDs)
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
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;

        IQueryable<TEntity> query = dbcontext.Set<TEntity>();

        try
        {
            if (filter != null) query = query.Where(filter);
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
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null)
    {
        IQueryable<TEntity> query = dbcontext.Set<TEntity>();

        try
        {
            if (filter != null) query = query.Where(filter);
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

    public async Task<Result<Deleted>> RemoveAsync(Expression<Func<TEntity, bool>> filter)
    {
        if (filter == null) return new Error("00", "Filter cannot be null.", ErrorKind.Failure);

        try
        {
            var getEntity = await GetAsync(filter);
            if (getEntity.IsError) return getEntity.Errors;
            var entity = getEntity.Value;
            if (entity == null) return new Error("00", "Entity not found.", ErrorKind.Failure);

            dbcontext.Set<TEntity>().Remove(entity);
            int saveResult = await dbcontext.SaveChangesAsync();
            return saveResult > 0 ? Result.Deleted : new Error("00", "Failed to delete entity.", ErrorKind.Failure);
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

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter, Func<IQueryable<TEntity>, IQueryable<TEntity>>? transform = null)
    {
        try
        {
            IQueryable<TEntity> query = dbcontext.Set<TEntity>();
            if (transform != null) query = transform(query);
            return await query.AnyAsync(filter);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error checking existence of entities of type {EntityType}.", typeof(TEntity).Name);
            return false; // Return false on error, as AnyAsync should be safe
        }
    }

    public async Task<Result<Updated>> UpdateAsync(TEntity entity)
    {
        if (entity == null) return new Error("00", "Entity cannot be null.", ErrorKind.Failure);
        try
        {
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

    public async Task<Result<TEntity?>> GetByIdAsync(TKey id)
    {
        try
        {
            return await dbcontext.Set<TEntity>().FindAsync(id);
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
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null)
    {
        IQueryable<TEntity> query = dbcontext.Set<TEntity>();

        try
        {
            query = query.Where(filter);
            if (transform != null) query = transform(query);

            // If no specific ordering is provided, order by Id descending (newest first, assuming auto-incrementing IDs)
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

        // SQL Server format: "FOREIGN KEY constraint "FK_Table_RefTable"..."
        var match = Regex.Match(message, @"FOREIGN KEY constraint ""?(?<fk>FK_[\w\d_]+)""?", RegexOptions.IgnoreCase);
        if (match.Success)
            return match.Groups["fk"].Value;

        // PostgreSQL format example: "insert or update on table ... violates foreign key constraint "FK_Table_RefTable""
        match = Regex.Match(message, @"violates foreign key constraint ""?(?<fk>FK_[\w\d_]+)""?", RegexOptions.IgnoreCase);
        if (match.Success)
            return match.Groups["fk"].Value;

        // MySQL format example: "Cannot add or update a child row: a foreign key constraint fails (`db`.`table`, CONSTRAINT `FK_Table_RefTable` FOREIGN KEY (`Column`)..."
        match = Regex.Match(message, @"CONSTRAINT [`""]?(?<fk>FK_[\w\d_]+)[`""]? FOREIGN KEY", RegexOptions.IgnoreCase);
        if (match.Success)
            return match.Groups["fk"].Value;

        return null;
    }

}
