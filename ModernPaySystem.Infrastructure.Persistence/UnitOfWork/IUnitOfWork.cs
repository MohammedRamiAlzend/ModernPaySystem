using ModernPaySystem.Application.Repos;
using ModernPaySystem.Domain.Entities.SharedEntities;
using ModernPaySystem.Domain.Entities.TransactionSystemEntities;

namespace ModernPaySystem.Infrastructure.Persistence.UnitOfWork;

/// <summary>
/// Unit of Work interface for coordinating multiple repositories
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    /// <summary>
    /// Repository for User entities
    /// </summary>
    IRepositoryBase<User, Guid> Users { get; }

    /// <summary>
    /// Repository for Role entities
    /// </summary>
    IRepositoryBase<Role, Guid> Roles { get; }

    /// <summary>
    /// Repository for PermissionEntity entities
    /// </summary>
    IRepositoryBase<PermissionEntity, Guid> Permissions { get; }

    /// <summary>
    /// Repository for SubSystemUser entities
    /// </summary>
    IRepositoryBase<SubSystemUser, Guid> SubSystemUsers { get; }

    /// <summary>
    /// Repository for Attachment entities
    /// </summary>
    IRepositoryBase<Attachment, Guid> Attachments { get; }

    /// <summary>
    /// Repository for Template entities
    /// </summary>
    IRepositoryBase<Template, Guid> Templates { get; }

    /// <summary>
    /// Repository for Request entities
    /// </summary>
    IRepositoryBase<Request, Guid> Requests { get; }

    /// <summary>
    /// Repository for Response entities
    /// </summary>
    IRepositoryBase<Response, Guid> Responses { get; }

    /// <summary>
    /// Repository for TemplateOwnership entities
    /// </summary>
    IRepositoryBase<TemplateOwnership, Guid> TemplateOwnerships { get; }

    /// <summary>
    /// Saves all changes made to the database
    /// </summary>
    Task<int> SaveChangesAsync();

    /// <summary>
    /// Begins a new database transaction
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    Task CommitTransactionAsync();

    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    Task RollbackTransactionAsync();
}
