using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Domain.Entities.SharedEntities;
using ModernPaySystem.Domain.Entities.TransactionSystemEntities;

namespace ModernPaySystem.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // Shared Entities
    public DbSet<User> Users { get; set; }
    public DbSet<SubSystemUser> SubSystemUsers { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<PermissionEntity> Permissions { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }

    // Transaction System Entities
    public DbSet<Template> Templates { get; set; }
    public DbSet<Request> Requests { get; set; }
    public DbSet<RequestAttachment> RequestAttachments { get; set; }
    public DbSet<Response> Responses { get; set; }
    public DbSet<ResponseAttachment> ResponseAttachments { get; set; }
    public DbSet<TemplateOwnership> TemplateOwnerships { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<RequestAttachment>()
            .HasKey(ra => new { ra.RequestId, ra.AttachmentId });

        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        modelBuilder.Entity<RolePermission>()
            .HasKey(rp => new { rp.RoleId, rp.PermissionId });

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

        modelBuilder.Entity<Response>()
            .HasOne(resp => resp.Request)
            .WithMany()
            .HasForeignKey(resp => resp.RequestId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ResponseAttachment>()
            .HasOne(ra => ra.Response)
            .WithMany()
            .HasForeignKey(ra => ra.ResponseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ResponseAttachment>()
            .HasOne(ra => ra.Attachment)
            .WithMany()
            .HasForeignKey(ra => ra.AttachmentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TemplateOwnership>()
            .HasOne(to => to.Template)
            .WithMany(t => t.Ownerships)
            .HasForeignKey(to => to.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TemplateOwnership>()
            .HasOne(to => to.User)
            .WithMany(u => u.TemplateOwnerships)
            .HasForeignKey(to => to.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SubSystemUser>()
            .HasOne(ssu => ssu.User)
            .WithOne(u => u.SubSystemUser)
            .HasForeignKey<SubSystemUser>(ssu => ssu.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
