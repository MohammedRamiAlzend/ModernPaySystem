using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Module.Archive.Domain.Entities;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Archive.Infrastructure.Persistence;

public class ArchiveDbContext(DbContextOptions<ArchiveDbContext> options) : DbContext(options)
{
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Folder>(entity =>
        {
            entity.Ignore(e => e.Department);

            entity.HasOne(f => f.Parent)
                .WithMany(f => f.SubFolders)
                .HasForeignKey(f => f.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(f => f.Icon)
                .WithMany(i => i.Folders)
                .HasForeignKey(f => f.IconId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(f => f.ArchiveRecords)
                .WithOne(ar => ar.Folder)
                .HasForeignKey(ar => ar.FolderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(f => f.Permissions)
                .WithOne(p => p.Folder)
                .HasForeignKey(p => p.FolderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(f => !f.IsDeleted);
        });

        modelBuilder.Entity<ArchiveRecord>(entity =>
        {
            entity.Ignore(e => e.Department);

            entity.HasMany(ar => ar.PhysicalFiles)
                .WithOne(pf => pf.ArchiveRecord)
                .HasForeignKey(pf => pf.ArchiveRecordId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ar => ar.Form)
                .WithMany(f => f.ArchiveRecords)
                .HasForeignKey(ar => ar.FormId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(ar => !ar.IsDeleted);
        });

        modelBuilder.Entity<ArchiveRecordTemplateValues>(entity =>
        {
            entity.HasMany(artv => artv.ArchiveRecordFormInputValues)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
        });

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

        modelBuilder.Entity<PhysicalFile>(entity =>
        {
            entity.HasIndex(pf => new { pf.ArchiveRecordId, pf.CreatedAt })
                .HasDatabaseName("IX_PhysicalFiles_ArchiveRecordId_CreatedAt");

            entity.HasOne(pf => pf.EditArchiveRequest)
                .WithMany(r => r.PhysicalFiles)
                .HasForeignKey(pf => pf.EditArchiveRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(pf => new { pf.ArchiveRecordId, pf.IsDeleted, pf.FileExtension })
                .IncludeProperties(pf => new { pf.FileSize, pf.ContentType, pf.FileName, pf.CreatedAt, pf.UpdatedAt })
                .HasDatabaseName("IX_PhysicalFiles_ArchiveRecordId_IsDeleted_FileExtension_Covering");
        });

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
            entity.Property(e => e.AllowedFileExtensions).HasMaxLength(2000);
            entity.HasIndex(e => e.IsActive);
        });

        modelBuilder.Entity<ArchiveFormTemplate>(entity =>
        {
            entity.Property(e => e.ContentAsJson).HasColumnType("jsonb");
        });

        modelBuilder.Entity<FolderPermission>(entity =>
        {
            entity.HasIndex(e => e.FolderId);
            entity.HasIndex(e => e.DepartmentId);
            entity.Property(e => e.UserId).HasMaxLength(450);
        });

        modelBuilder.Entity<FolderIcon>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.SvgContent).HasColumnType("text").IsRequired();
        });

        modelBuilder.Entity<DepartmentArchiveLeader>(entity =>
        {
            entity.Ignore(e => e.Department);
            entity.Ignore(e => e.User);

            entity.HasIndex(x => new { x.DepartmentId, x.UserId })
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");

            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<DeleteArchiveRequest>(entity =>
        {
            entity.Ignore(e => e.Department);
            entity.Ignore(e => e.Requester);
            entity.Ignore(e => e.Approver);

            entity.Property(x => x.TargetSnapshotJson).HasColumnType("jsonb");
            entity.Property(x => x.DependenciesSnapshotJson).HasColumnType("jsonb");
            entity.Property(x => x.ActivitySnapshotJson).HasColumnType("jsonb");
            entity.HasIndex(x => new { x.DepartmentId, x.TargetType, x.TargetId, x.Status });
        });

        modelBuilder.Entity<EditArchiveRequest>(entity =>
        {
            entity.Ignore(e => e.Department);
            entity.Ignore(e => e.Requester);
            entity.Ignore(e => e.Approver);

            entity.Property(x => x.RowVersion)
                .IsConcurrencyToken(false)
                .ValueGeneratedNever();

            entity.Property(x => x.RequestedChangesJson).HasColumnType("jsonb");
            entity.Property(x => x.OriginalSnapshotJson).HasColumnType("jsonb");
            entity.HasIndex(x => new { x.DepartmentId, x.ArchiveRecordId, x.Status });
        });
    }
}
