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
    IRepositoryBase<RequestTemplateValues, Guid> RequestTemplateValues { get; }

    IRepositoryBase<Response, Guid> Responses { get; }

    IRepositoryBase<TemplateDepartmentOwnership, Guid> TemplateOwnerships { get; }
    IRepositoryBase<UserTemplateOwnership, Guid> UserTemplateOwnerships { get; }

    IRepositoryBase<ResponseAttachment, Guid> ResponseAttachments { get; }

    IRepositoryBase<RequestAttachment, Guid> RequestAttachments { get; }

    IRepositoryBase<LookUpField, Guid> LookUpFields { get; }

    IRepositoryBase<LookUpFiledValues, Guid> LookUpFiledValues { get; }

    IRepositoryBase<RequestTransaction, Guid> RequestTransactions { get; }

    IRepositoryBase<RequestTransactionAttachment, Guid> RequestTransactionAttachments { get; }

    IRepositoryBase<Department, Guid> Departments { get; }
    IRepositoryBase<DepartmentTemplateNumber, Guid> DepartmentTemplateNumbers { get; }

    Task<int> SaveChangesAsync();
    //Task<int> GetNextRequestNumberAsync(Guid departmentId);
    bool HasActiveTransaction { get; }

    Task BeginTransactionAsync();

    Task CommitTransactionAsync();

    Task RollbackTransactionAsync();
}
