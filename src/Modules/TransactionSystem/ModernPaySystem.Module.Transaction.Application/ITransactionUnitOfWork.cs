using ModernPaySystem.Module.Transaction.Domain.Entities;
using ModernPaySystem.SharedKernel.Application.Repos;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Transaction.Application;

public interface ITransactionUnitOfWork
{
    IRepositoryBase<Request, Guid> Requests { get; }
    IRepositoryBase<RequestTemplateValues, Guid> RequestTemplateValues { get; }
    IRepositoryBase<RequestRelation, Guid> RequestRelations { get; }
    IRepositoryBase<Response, Guid> Responses { get; }
    IRepositoryBase<Template, Guid> Templates { get; }
    IRepositoryBase<TemplateDepartmentOwnership, Guid> TemplateDepartmentOwnerships { get; }
    IRepositoryBase<UserTemplateOwnership, Guid> UserTemplateOwnerships { get; }
    IRepositoryBase<RequestAttachment, Guid> RequestAttachments { get; }
    IRepositoryBase<ResponseAttachment, Guid> ResponseAttachments { get; }
    IRepositoryBase<RequestTransaction, Guid> RequestTransactions { get; }
    IRepositoryBase<RequestTransactionAttachment, Guid> RequestTransactionAttachments { get; }
    IRepositoryBase<RequestAuditLog, Guid> RequestAuditLogs { get; }
    IRepositoryBase<InputValue, Guid> InputValues { get; }
    IRepositoryBase<Attachment, Guid> Attachments { get; }
    IRepositoryBase<LookUpField, Guid> LookUpFields { get; }
    IRepositoryBase<LookUpFiledValues, Guid> LookUpFiledValues { get; }
    IRepositoryBase<DepartmentTemplateNumber, Guid> DepartmentTemplateNumbers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    bool HasActiveTransaction { get; }
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
