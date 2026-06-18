using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Domain.Entities.Abstraction;
using ModernPaySystem.Domain.Entities.Archiving;
using ModernPaySystem.Domain.Entities.PaySystemEntities.FastOperations;
using ModernPaySystem.Domain.Entities.SharedEntities;
using ModernPaySystem.Domain.Entities.TransactionSystemEntities;

namespace ModernPaySystem.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<SubSystemUser> SubSystemUsers { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<PermissionEntity> Permissions { get; set; }
    public DbSet<Department> Departments { get; set; }

    public DbSet<Folder> Folders { get; set; }
    public DbSet<DepartmentArchiveLeader> DepartmentArchiveLeaders { get; set; }
    public DbSet<DeleteArchiveRequest> DeleteArchiveRequests { get; set; }
    public DbSet<EditArchiveRequest> EditArchiveRequests { get; set; }
    public DbSet<ArchiveFormTemplate> DynamicForms { get; set; }
    public DbSet<ArchiveRecord> ArchiveRecords { get; set; }
    public DbSet<ArchiveRecordTemplateValues> ArchiveRecordTemplateValues { get; set; }
    public DbSet<ArchiveRecordFormInputValue> ArchiveRecordFormInputValues { get; set; }
    public DbSet<PhysicalFile> PhysicalFiles { get; set; }
    public DbSet<FolderPermission> FolderPermissions { get; set; }
    public DbSet<FolderIcon> FolderIcons { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<DocumentChunk> DocumentChunks { get; set; }
    public DbSet<ArchiveAuditLog> ArchiveAuditLogs { get; set; }
    public DbSet<ArchiveConfig> ArchiveConfigs { get; set; }

    public DbSet<Template> Templates { get; set; }
    public DbSet<Request> Requests { get; set; }
    public DbSet<RequestRelation> RequestRelations { get; set; }
    public DbSet<RequestAttachment> RequestAttachments { get; set; }
    public DbSet<Response> Responses { get; set; }
    public DbSet<ResponseAttachment> ResponseAttachments { get; set; }
    public DbSet<TemplateDepartmentOwnership> TemplateOwnerships { get; set; }
    public DbSet<UserTemplateOwnership> UserTemplateOwnerships { get; set; }
    public DbSet<RequestTransaction> RequestTransactions { get; set; }
    public DbSet<RequestTransactionAttachment> RequestTransactionAttachments { get; set; }

    public DbSet<LookUpField> LookUpFields { get; set; }
    public DbSet<LookUpFiledValues> LookUpFiledValues { get; set; }
    public DbSet<DepartmentTemplateNumber> DepartmentTemplateNumbers { get; set; }

    public DbSet<Client> Clients { get; set; }
    public DbSet<Gender> Genders { get; set; }
    public DbSet<National> Nationals { get; set; }
    public DbSet<Gov> Govs { get; set; }
    public DbSet<KindShip> KindShips { get; set; }
    public DbSet<OperationStatus> OperationStatuses { get; set; }
    public DbSet<OperationServiceType> OperationServiceTypes { get; set; }
    public DbSet<Operation> Operations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasMany(u => u.VisitedTemplates)
            .WithMany(t => t.VisitedByUsers)
            .UsingEntity(j => j.ToTable("UserVisitedTemplates"));

        modelBuilder.Entity<RequestAttachment>()
            .HasKey(ra => new { ra.RequestId, ra.AttachmentId });

        modelBuilder.Entity<Request>()
            .HasOne(r => r.RequestTemplateValues)
            .WithOne(rt => rt.Request)
            .HasForeignKey<RequestTemplateValues>(rt => rt.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Request>()
            .HasOne(r => r.Requester)
            .WithMany(u => u.RequestsAsRequester)
            .HasForeignKey(r => r.RequesterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RequestTemplateValues>().HasMany(rt => rt.InputValues)
            .WithOne(iv => iv.RequestTemplateValues)
            .HasForeignKey(iv => iv.RequestTemplateValuesId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Request>()
            .HasOne(r => r.Approver)
            .WithMany(u => u.RequestsAsApprover)
            .HasForeignKey(r => r.ApproverId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RequestAttachment>()
            .HasOne(ra => ra.Request)
            .WithMany(r => r.RequestAttachments)
            .HasForeignKey(ra => ra.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RequestAttachment>()
            .HasOne(ra => ra.Attachment)
            .WithMany(a => a.RequestAttachments)
            .HasForeignKey(ra => ra.AttachmentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Request>()
            .HasOne(r => r.Response)
            .WithOne(resp => resp.Request)
            .HasForeignKey<Request>(r => r.ResponseId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<DepartmentTemplateNumber>()
            .HasIndex(r => new { r.DepartmentId, r.TemplateId })
            .IsUnique();


        modelBuilder.Entity<ResponseAttachment>()
            .HasOne(ra => ra.Response)
            .WithMany(r => r.ResponseAttachments)
            .HasForeignKey(ra => ra.ResponseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ResponseAttachment>()
            .HasOne(ra => ra.Attachment)
            .WithMany(a => a.ResponseAttachments)
            .HasForeignKey(ra => ra.AttachmentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Roles)
            .WithMany(r => r.Users)
            .UsingEntity(j => j.ToTable("UserRoles"));

        modelBuilder.Entity<Role>()
            .HasMany(r => r.Permissions)
            .WithMany(p => p.Roles)
            .UsingEntity(j => j.ToTable("RolePermissions"));

        modelBuilder.Entity<TemplateDepartmentOwnership>()
            .HasOne(to => to.Template)
            .WithMany(t => t.DepartmentOwnerships)
            .HasForeignKey(to => to.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TemplateDepartmentOwnership>()
            .HasOne(to => to.Department)
            .WithMany(u => u.TemplateOwnerships)
            .HasForeignKey(to => to.DepartmentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserTemplateOwnership>()
            .HasOne(uto => uto.Template)
            .WithMany(t => t.UserOwnerships)
            .HasForeignKey(uto => uto.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserTemplateOwnership>()
            .HasOne(uto => uto.User)
            .WithMany(u => u.TemplateOwnerships)
            .HasForeignKey(uto => uto.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SubSystemUser>()
            .HasOne(ssu => ssu.User)
            .WithOne(u => u.SubSystemUser)
            .HasForeignKey<SubSystemUser>(ssu => ssu.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LookUpFiledValues>()
            .HasOne(lfv => lfv.LookUpFiled)
            .WithMany(lf => lf.LookUpFiledValues)
            .HasForeignKey(lfv => lfv.LookUpFiledId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Client>()
            .HasOne(c => c.Gender)
            .WithMany()
            .HasForeignKey(c => c.GenderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Client>()
            .HasOne(c => c.National)
            .WithMany()
            .HasForeignKey(c => c.NationalId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Client>()
            .HasOne(c => c.Gov)
            .WithMany()
            .HasForeignKey(c => c.GovId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Operation>()
            .HasOne(o => o.ApplicantClient)
            .WithMany()
            .HasForeignKey(o => o.ApplicantClientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Operation>()
            .HasOne(o => o.RecipientClient)
            .WithMany()
            .HasForeignKey(o => o.RecipientClientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Operation>()
            .HasOne(o => o.KindShip)
            .WithMany()
            .HasForeignKey(o => o.KindShipId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Operation>()
            .HasOne(o => o.Status)
            .WithMany()
            .HasForeignKey(o => o.OperationStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Operation>()
            .HasOne(o => o.OperationServiceType)
            .WithMany()
            .HasForeignKey(o => o.OperationServiceTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RequestTransaction>()
            .HasOne(rt => rt.ParentTransaction)
            .WithMany(rt => rt.ChildTransactions)
            .HasForeignKey(rt => rt.ParentTransactionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RequestTransaction>()
            .HasOne(rt => rt.Request)
            .WithMany()
            .HasForeignKey(rt => rt.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RequestTransaction>()
            .HasOne(rt => rt.CurrentUserHolder)
            .WithMany()
            .HasForeignKey(rt => rt.CurrentUserHolderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Request>()
            .HasOne(r => r.FirstTransaction)
            .WithMany()
            .HasForeignKey(r => r.FirstTransactionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Request>()
            .HasOne(r => r.CurrentTransaction)
            .WithMany()
            .HasForeignKey(r => r.CurrentTransactionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RequestTransactionAttachment>()
            .HasOne(rta => rta.RequestTransaction)
            .WithMany(rt => rt.RequestTransactionAttachments)
            .HasForeignKey(rta => rta.RequestTransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RequestTransactionAttachment>()
            .HasOne(rta => rta.Attachment)
            .WithMany()
            .HasForeignKey(rta => rta.AttachmentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Folder>()
            .HasOne(f => f.Parent)
            .WithMany(f => f!.SubFolders)
            .HasForeignKey(f => f.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Folder>()
            .HasOne(f => f.Icon)
            .WithMany(i => i.Folders)
            .HasForeignKey(f => f.IconId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<FolderIcon>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.SvgContent).HasColumnType("text").IsRequired();
        });

        modelBuilder.Entity<Folder>()
            .HasOne(f => f.Department)
            .WithMany()
            .HasForeignKey(f => f.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Folder>()
            .HasMany(f => f.ArchiveRecords)
            .WithOne(ar => ar.Folder)
            .HasForeignKey(ar => ar.FolderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Folder>().HasQueryFilter(f => !f.IsDeleted);

        modelBuilder.Entity<ArchiveFormTemplate>()
            .HasMany(f => f.ArchiveRecords)
            .WithOne(ar => ar.Form)
            .HasForeignKey(ar => ar.FormId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ArchiveRecord>()
            .HasOne(ar => ar.Department)
            .WithMany()
            .HasForeignKey(ar => ar.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ArchiveRecord>()
            .HasMany(ar => ar.PhysicalFiles)
            .WithOne(pf => pf.ArchiveRecord)
            .HasForeignKey(pf => pf.ArchiveRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ArchiveRecord>().HasQueryFilter(ar => !ar.IsDeleted);

        modelBuilder.Entity<ArchiveRecordTemplateValues>()
            .HasMany(artv => artv.ArchiveRecordFormInputValues)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ArchiveRecord>()
            .HasOne(ar => ar.ArchiveRecordTemplateValuesId)
            .WithOne()
            .HasForeignKey<ArchiveRecordTemplateValues>(artv => artv.ArchiveRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasIndex(e => e.SourceType);
            entity.HasIndex(e => e.PhysicalFileId);
            entity.HasIndex(e => e.ArchiveRecordId);
            entity.HasIndex(e => e.FileType);
            entity.HasIndex(e => e.CreatedAt);

            entity.Property(e => e.FileName).HasMaxLength(500);
            entity.Property(e => e.FileType).HasMaxLength(10);
            entity.Property(e => e.ExtractedText).HasColumnType("text");

            entity.HasOne(e => e.PhysicalFile)
                .WithMany()
                .HasForeignKey(e => e.PhysicalFileId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ArchiveRecord)
                .WithMany()
                .HasForeignKey(e => e.ArchiveRecordId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<DocumentChunk>(entity =>
        {
            entity.Property(e => e.Content).HasColumnType("text");

            entity.HasOne(e => e.Document)
                .WithMany(d => d.Chunks)
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PhysicalFile>()
            .HasIndex(pf => new { pf.ArchiveRecordId, pf.CreatedAt })
            .HasDatabaseName("IX_PhysicalFiles_ArchiveRecordId_CreatedAt");

        modelBuilder.Entity<PhysicalFile>()
            .HasOne(pf => pf.EditArchiveRequest)
            .WithMany(r => r.PhysicalFiles)
            .HasForeignKey(pf => pf.EditArchiveRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PhysicalFile>()
            .HasIndex(pf => new { pf.ArchiveRecordId, pf.IsDeleted, pf.FileExtension })
            .IncludeProperties(pf => new { pf.FileSize, pf.ContentType, pf.FileName, pf.CreatedAt, pf.UpdatedAt })
            .HasDatabaseName("IX_PhysicalFiles_ArchiveRecordId_IsDeleted_FileExtension_Covering");

        modelBuilder.Entity<ArchiveAuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ArchiveRecordId, e.Timestamp })
                .HasDatabaseName("IX_ArchiveAuditLogs_ArchiveRecordId_Timestamp");
            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_ArchiveAuditLogs_UserId");
            entity.HasIndex(e => e.Action)
                .HasDatabaseName("IX_ArchiveAuditLogs_Action");
            entity.Property(e => e.UserId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.Details).HasColumnType("text");
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
        });

        modelBuilder.Entity<ArchiveConfig>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DefaultPath).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.HasIndex(e => e.IsActive);
        });

        modelBuilder.Entity<Folder>()
            .HasMany(f => f.Permissions)
            .WithOne(p => p.Folder)
            .HasForeignKey(p => p.FolderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ArchiveFormTemplate>()
            .Property(f => f.ContentAsJson)
            .HasColumnType("jsonb");

        modelBuilder.Entity<Department>()
            .HasOne(d => d.ParentDepartment)
            .WithMany(d => d.ChildDepartments)
            .HasForeignKey(d => d.ParentDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Department>()
            .HasOne(d => d.DepartmentHead)
            .WithOne(u => u.HeadedDepartment)
            .HasForeignKey<Department>(d => d.DepartmentHeadId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DepartmentArchiveLeader>(entity =>
        {
            entity.HasOne(x => x.Department)
                .WithMany()
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.User)
                .WithMany(x => x.DepartmentArchiveLeaders)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.DepartmentId, x.UserId })
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");

            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<DeleteArchiveRequest>(entity =>
        {
            entity.HasOne(x => x.Department)
                .WithMany()
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Requester)
                .WithMany()
                .HasForeignKey(x => x.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Approver)
                .WithMany()
                .HasForeignKey(x => x.ApproverId)
                .OnDelete(DeleteBehavior.Restrict);

            // Removed concurrency token because Npgsql throws DbUpdateConcurrencyException due to missing trigger for bytea
            entity.Property(x => x.TargetSnapshotJson).HasColumnType("jsonb");
            entity.Property(x => x.DependenciesSnapshotJson).HasColumnType("jsonb");
            entity.Property(x => x.ActivitySnapshotJson).HasColumnType("jsonb");
            entity.HasIndex(x => new { x.DepartmentId, x.TargetType, x.TargetId, x.Status });
        });

        modelBuilder.Entity<EditArchiveRequest>(entity =>
        {
            entity.HasOne(x => x.Department)
                .WithMany()
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ArchiveRecord)
                .WithMany()
                .HasForeignKey(x => x.ArchiveRecordId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Requester)
                .WithMany()
                .HasForeignKey(x => x.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Approver)
                .WithMany()
                .HasForeignKey(x => x.ApproverId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(x => x.RowVersion)
                .IsConcurrencyToken(false)
                .ValueGeneratedNever();

            entity.Property(x => x.RequestedChangesJson).HasColumnType("jsonb");
            entity.Property(x => x.OriginalSnapshotJson).HasColumnType("jsonb");
            entity.HasIndex(x => new { x.DepartmentId, x.ArchiveRecordId, x.Status });
        });

        modelBuilder.Entity<User>()
            .HasOne(u => u.Department)
            .WithMany(d => d.Users)
            .HasForeignKey(u => u.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<InputValue>()
            .HasIndex(e => new { e.RequestTemplateValuesId, e.Key, e.Value })
            .HasDatabaseName("IX_InputValue_Lookup");

        modelBuilder.Entity<RequestTemplateValues>()
            .HasIndex(e => e.RequestId)
            .HasDatabaseName("IX_RequestTemplateValues_RequestId");

        modelBuilder.Entity<RequestRelation>(entity =>
        {
            entity.ToTable("RequestRelations");
            entity.HasKey(e => e.Id);

            entity.HasOne(r => r.SourceRequest)
                  .WithMany(r => r.OutgoingRelations)
                  .HasForeignKey(r => r.SourceRequestId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.TargetRequest)
                  .WithMany(r => r.IncomingRelations)
                  .HasForeignKey(r => r.TargetRequestId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.SourceRequestId, e.TargetRequestId, e.RelationType })
                  .IsUnique();
        });
    }
}
