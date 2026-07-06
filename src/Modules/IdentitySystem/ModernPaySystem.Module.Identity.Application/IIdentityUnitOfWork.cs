using ModernPaySystem.SharedKernel.Application.Repos;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Identity.Application;

public interface IIdentityUnitOfWork
{
    IRepositoryBase<User, Guid> Users { get; }
    IRepositoryBase<Role, Guid> Roles { get; }
    IRepositoryBase<PermissionEntity, Guid> Permissions { get; }
    IRepositoryBase<Department, Guid> Departments { get; }
    IRepositoryBase<SubSystemUser, Guid> SubSystemUsers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    bool HasActiveTransaction { get; }
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
