using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Module.Transaction.Domain.Entities;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Transaction.Infrastructure.Persistence;

public class TransactionDbContext(DbContextOptions<TransactionDbContext> options) : DbContext(options)
{
    public DbSet<Request> Requests { get; set; }
    public DbSet<RequestTemplateValues> RequestTemplateValues { get; set; }
    public DbSet<RequestRelation> RequestRelations { get; set; }
    public DbSet<RequestAttachment> RequestAttachments { get; set; }
    public DbSet<Response> Responses { get; set; }
    public DbSet<ResponseAttachment> ResponseAttachments { get; set; }
    public DbSet<Template> Templates { get; set; }
    public DbSet<TemplateDepartmentOwnership> TemplateDepartmentOwnerships { get; set; }
    public DbSet<UserTemplateOwnership> UserTemplateOwnerships { get; set; }
    public DbSet<RequestTransaction> RequestTransactions { get; set; }
    public DbSet<RequestTransactionAttachment> RequestTransactionAttachments { get; set; }
    public DbSet<RequestAuditLog> RequestAuditLogs { get; set; }
    public DbSet<InputValue> InputValues { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<LookUpField> LookUpFields { get; set; }
    public DbSet<LookUpFiledValues> LookUpFiledValues { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Template>()
            .HasMany(t => t.VisitedByUsers)
            .WithMany()
            .UsingEntity(j => j.ToTable("UserVisitedTemplates"));

        modelBuilder.Entity<Department>()
        .HasOne(u => u.DepartmentHead)
        .WithOne(u => u.HeadedDepartment)
        .HasForeignKey<Department>(d => d.DepartmentHeadId)
        .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<User>()
        .HasOne(u => u.SubSystemUser)
        .WithOne(u => u.User)
        .HasForeignKey<User>(d => d.SubSystemUserId)
        .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RequestAttachment>()
            .HasKey(ra => new { ra.RequestId, ra.AttachmentId });

        modelBuilder.Entity<Request>()
            .HasOne(r => r.RequestTemplateValues)
            .WithOne(rt => rt.Request)
            .HasForeignKey<RequestTemplateValues>(rt => rt.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Request>()
            .HasOne(r => r.Requester)
            .WithMany()
            .HasForeignKey(r => r.RequesterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RequestTemplateValues>()
            .HasMany(rt => rt.InputValues)
            .WithOne(iv => iv.RequestTemplateValues)
            .HasForeignKey(iv => iv.RequestTemplateValuesId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Request>()
            .HasOne(r => r.Approver)
            .WithMany()
            .HasForeignKey(r => r.ApproverId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RequestAttachment>()
            .HasOne(ra => ra.Request)
            .WithMany(r => r.RequestAttachments)
            .HasForeignKey(ra => ra.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RequestAttachment>()
            .HasOne(ra => ra.Attachment)
            .WithMany()
            .HasForeignKey(ra => ra.AttachmentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Request>()
            .HasOne(r => r.Response)
            .WithOne(resp => resp.Request)
            .HasForeignKey<Request>(r => r.ResponseId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ResponseAttachment>()
            .HasOne(ra => ra.Response)
            .WithMany(r => r.ResponseAttachments)
            .HasForeignKey(ra => ra.ResponseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ResponseAttachment>()
            .HasOne(ra => ra.Attachment)
            .WithMany()
            .HasForeignKey(ra => ra.AttachmentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TemplateDepartmentOwnership>()
            .HasOne(to => to.Template)
            .WithMany(t => t.DepartmentOwnerships)
            .HasForeignKey(to => to.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TemplateDepartmentOwnership>()
            .HasOne(to => to.Department)
            .WithMany()
            .HasForeignKey(to => to.DepartmentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserTemplateOwnership>()
            .HasOne(uto => uto.Template)
            .WithMany(t => t.UserOwnerships)
            .HasForeignKey(uto => uto.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserTemplateOwnership>()
            .HasOne(uto => uto.User)
            .WithMany()
            .HasForeignKey(uto => uto.UserId)
            .OnDelete(DeleteBehavior.Cascade);

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

        modelBuilder.Entity<RequestAuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.RequestId, e.Timestamp })
                .HasDatabaseName("IX_RequestAuditLogs_RequestId_Timestamp");
            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_RequestAuditLogs_UserId");
            entity.HasIndex(e => e.Action)
                .HasDatabaseName("IX_RequestAuditLogs_Action");
            entity.Property(e => e.Details).HasColumnType("text");
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);

            entity.HasOne(e => e.Request)
                .WithMany()
                .HasForeignKey(e => e.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

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

        modelBuilder.Entity<Template>()
            .Property(t => t.ContentAsJson)
            .HasColumnType("jsonb");
    }
}
