using ModernPaySystem.Module.Archive.Domain.Entities;
using ModernPaySystem.SharedKernel.Application.Repos;

namespace ModernPaySystem.Module.Archive.Application;

public interface IArchiveUnitOfWork
{
    IRepositoryBase<Folder, Guid> Folders { get; }
    IRepositoryBase<DepartmentArchiveLeader, Guid> DepartmentArchiveLeaders { get; }
    IRepositoryBase<DeleteArchiveRequest, Guid> DeleteArchiveRequests { get; }
    IRepositoryBase<EditArchiveRequest, Guid> EditArchiveRequests { get; }
    IRepositoryBase<ArchiveFormTemplate, Guid> DynamicForms { get; }
    IRepositoryBase<ArchiveRecord, Guid> ArchiveRecords { get; }
    IRepositoryBase<ArchiveRecordTemplateValues, Guid> ArchiveRecordTemplateValues { get; }
    IRepositoryBase<ArchiveRecordFormInputValue, Guid> ArchiveRecordFormInputValues { get; }
    IRepositoryBase<PhysicalFile, Guid> PhysicalFiles { get; }
    IRepositoryBase<FolderPermission, Guid> FolderPermissions { get; }
    IRepositoryBase<Document, Guid> Documents { get; }
    IRepositoryBase<DocumentChunk, Guid> DocumentChunks { get; }
    IRepositoryBase<ArchiveAuditLog, Guid> ArchiveAuditLogs { get; }
    IRepositoryBase<ArchiveConfig, Guid> ArchiveConfigs { get; }
    IRepositoryBase<FolderIcon, Guid> FolderIcons { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    bool HasActiveTransaction { get; }
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
