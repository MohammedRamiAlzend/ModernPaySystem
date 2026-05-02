using ModernPaySystem.Application.Repos;
using ModernPaySystem.Domain.Entities.SharedEntities;
using ModernPaySystem.Domain.Entities.TransactionSystemEntities;

namespace ModernPaySystem.Infrastructure.Persistence.UnitOfWork;

public interface IUnitOfWork
{
    IRepositoryBase<User, Guid> Users { get; }

    IRepositoryBase<Role, Guid> Roles { get; }

    IRepositoryBase<PermissionEntity, Guid> Permissions { get; }

    IRepositoryBase<SubSystemUser, Guid> SubSystemUsers { get; }

    IRepositoryBase<Attachment, Guid> Attachments { get; }

    IRepositoryBase<Template, Guid> Templates { get; }

    IRepositoryBase<Request, Guid> Requests { get; }

    IRepositoryBase<Response, Guid> Responses { get; }

    IRepositoryBase<TemplateOwnership, Guid> TemplateOwnerships { get; }

    IRepositoryBase<ResponseAttachment, Guid> ResponseAttachments { get; }

    IRepositoryBase<RequestAttachment, Guid> RequestAttachments { get; }

    IRepositoryBase<LookUpField, Guid> LookUpFields { get; }

    IRepositoryBase<LookUpFiledValues, Guid> LookUpFiledValues { get; }

    IRepositoryBase<RequestTransaction, Guid> RequestTransactions { get; }

    IRepositoryBase<RequestTransactionAttachment, Guid> RequestTransactionAttachments { get; }

    Task<int> SaveChangesAsync();

    Task BeginTransactionAsync();

    Task CommitTransactionAsync();

    Task RollbackTransactionAsync();
}
