using Microsoft.EntityFrameworkCore;
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

    public DbSet<Template> Templates { get; set; }
    public DbSet<Request> Requests { get; set; }
    public DbSet<RequestAttachment> RequestAttachments { get; set; }
    public DbSet<Response> Responses { get; set; }
    public DbSet<ResponseAttachment> ResponseAttachments { get; set; }
    public DbSet<TemplateDepartmentOwnership> TemplateOwnerships { get; set; }
    public DbSet<UserTemplateOwnership> UserTemplateOwnerships { get; set; }
    public DbSet<RequestTransaction> RequestTransactions { get; set; }
    public DbSet<RequestTransactionAttachment> RequestTransactionAttachments { get; set; }

    public DbSet<LookUpField> LookUpFields { get; set; }
    public DbSet<LookUpFiledValues> LookUpFiledValues { get; set; }

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

        modelBuilder.Entity<RequestAttachment>()
            .HasKey(ra => new { ra.RequestId, ra.AttachmentId });

        modelBuilder.Entity<Request>()
            .HasOne(r => r.Template)
            .WithMany(t => t.Requests)
            .HasForeignKey(r => r.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Request>()
            .HasOne(r => r.Requester)
            .WithMany(u => u.RequestsAsRequester)
            .HasForeignKey(r => r.RequesterId)
            .OnDelete(DeleteBehavior.Restrict);

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

        // RequestTransaction self-referencing relationship
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

        // Request to RequestTransaction relationships
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

        // Department self-referencing relationship
        modelBuilder.Entity<Department>()
            .HasOne(d => d.ParentDepartment)
            .WithMany(d => d.ChildDepartments)
            .HasForeignKey(d => d.ParentDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Department>()
            .HasOne(d => d.DepartmentHead)
            .WithOne(u => u.HeadedDepartment)
            .HasForeignKey<Department>(d => d.DepartmentHeadId)
            .OnDelete(DeleteBehavior.Restrict);

        // Department-User relationship
        modelBuilder.Entity<User>()
            .HasOne(u => u.Department)
            .WithMany(d => d.Users)
            .HasForeignKey(u => u.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
