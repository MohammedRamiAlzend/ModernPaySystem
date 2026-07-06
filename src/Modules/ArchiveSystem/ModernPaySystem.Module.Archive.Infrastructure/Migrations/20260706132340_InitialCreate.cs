using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModernPaySystem.Module.Archive.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArchiveConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DefaultPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AllowedFileExtensions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeleteArchiveRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetType = table.Column<int>(type: "integer", nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequesterId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApproverId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Justification = table.Column<string>(type: "text", nullable: false),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    ApprovalNotes = table.Column<string>(type: "text", nullable: true),
                    TargetSnapshotJson = table.Column<string>(type: "jsonb", nullable: false),
                    DependenciesSnapshotJson = table.Column<string>(type: "jsonb", nullable: false),
                    ActivitySnapshotJson = table.Column<string>(type: "jsonb", nullable: true),
                    SourceFolderId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetDisplayName = table.Column<string>(type: "text", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExecutedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RequesterNotificationMessage = table.Column<string>(type: "text", nullable: true),
                    RequesterNotifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeleteArchiveRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DepartmentArchiveLeaders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentArchiveLeaders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DynamicForms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FormName = table.Column<string>(type: "text", nullable: false),
                    ContentAsJson = table.Column<string>(type: "jsonb", nullable: false),
                    FormDescription = table.Column<string>(type: "text", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicForms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FolderIcons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SvgContent = table.Column<string>(type: "text", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FolderIcons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Folders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DefaultStoragePath = table.Column<string>(type: "text", nullable: true),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    IconId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedByUserId = table.Column<string>(type: "text", nullable: true),
                    DeletedByRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Folders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Folders_FolderIcons_IconId",
                        column: x => x.IconId,
                        principalTable: "FolderIcons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Folders_Folders_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Folders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ArchiveRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    FolderId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    FormId = table.Column<Guid>(type: "uuid", nullable: true),
                    ArchiveRecordTemplateValues = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedByUserId = table.Column<string>(type: "text", nullable: true),
                    DeletedByRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedByRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchiveRecords_DynamicForms_FormId",
                        column: x => x.FormId,
                        principalTable: "DynamicForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ArchiveRecords_Folders_FolderId",
                        column: x => x.FolderId,
                        principalTable: "Folders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FolderPermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FolderId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    AccessLevel = table.Column<int>(type: "integer", nullable: false),
                    IsInherited = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FolderPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FolderPermissions_Folders_FolderId",
                        column: x => x.FolderId,
                        principalTable: "Folders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchiveAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ArchiveRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchiveAuditLogs_ArchiveRecords_ArchiveRecordId",
                        column: x => x.ArchiveRecordId,
                        principalTable: "ArchiveRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchiveRecordTemplateValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ArchiveRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    ArchiveFormTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveRecordTemplateValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchiveRecordTemplateValues_ArchiveRecords_ArchiveRecordId",
                        column: x => x.ArchiveRecordId,
                        principalTable: "ArchiveRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArchiveRecordTemplateValues_DynamicForms_ArchiveFormTemplat~",
                        column: x => x.ArchiveFormTemplateId,
                        principalTable: "DynamicForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EditArchiveRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ArchiveRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequesterId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApproverId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Justification = table.Column<string>(type: "text", nullable: false),
                    RequestedChangesJson = table.Column<string>(type: "jsonb", nullable: false),
                    RequestedRecordName = table.Column<string>(type: "text", nullable: true),
                    RequestedFileDeletionIdsJson = table.Column<string>(type: "text", nullable: true),
                    OriginalSnapshotJson = table.Column<string>(type: "jsonb", nullable: false),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    ApprovalNotes = table.Column<string>(type: "text", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EditArchiveRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EditArchiveRequests_ArchiveRecords_ArchiveRecordId",
                        column: x => x.ArchiveRecordId,
                        principalTable: "ArchiveRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchiveRecordFormInputValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true),
                    ArchiveRecordTemplateValuesId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveRecordFormInputValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchiveRecordFormInputValues_ArchiveRecordTemplateValues_Ar~",
                        column: x => x.ArchiveRecordTemplateValuesId,
                        principalTable: "ArchiveRecordTemplateValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhysicalFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ArchiveRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    EditArchiveRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    FileExtension = table.Column<string>(type: "text", nullable: false),
                    StoragePath = table.Column<string>(type: "text", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: false),
                    IsQrPage = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhysicalFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhysicalFiles_ArchiveRecords_ArchiveRecordId",
                        column: x => x.ArchiveRecordId,
                        principalTable: "ArchiveRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PhysicalFiles_EditArchiveRequests_EditArchiveRequestId",
                        column: x => x.EditArchiveRequestId,
                        principalTable: "EditArchiveRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    PhysicalFileId = table.Column<Guid>(type: "uuid", nullable: true),
                    ArchiveRecordId = table.Column<Guid>(type: "uuid", nullable: true),
                    FileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    TotalChunks = table.Column<int>(type: "integer", nullable: false),
                    ExtractedText = table.Column<string>(type: "text", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_ArchiveRecords_ArchiveRecordId",
                        column: x => x.ArchiveRecordId,
                        principalTable: "ArchiveRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Documents_PhysicalFiles_PhysicalFileId",
                        column: x => x.PhysicalFileId,
                        principalTable: "PhysicalFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DocumentChunks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChunkIndex = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    TokenCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentChunks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentChunks_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveAuditLogs_Action",
                table: "ArchiveAuditLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveAuditLogs_ArchiveRecordId_Timestamp",
                table: "ArchiveAuditLogs",
                columns: new[] { "ArchiveRecordId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveAuditLogs_UserId",
                table: "ArchiveAuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveConfigs_IsActive",
                table: "ArchiveConfigs",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveRecordFormInputValues_ArchiveRecordTemplateValuesId",
                table: "ArchiveRecordFormInputValues",
                column: "ArchiveRecordTemplateValuesId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveRecords_FolderId",
                table: "ArchiveRecords",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveRecords_FormId",
                table: "ArchiveRecords",
                column: "FormId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveRecordTemplateValues_ArchiveFormTemplateId",
                table: "ArchiveRecordTemplateValues",
                column: "ArchiveFormTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveRecordTemplateValues_ArchiveRecordId",
                table: "ArchiveRecordTemplateValues",
                column: "ArchiveRecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeleteArchiveRequests_DepartmentId_TargetType_TargetId_Stat~",
                table: "DeleteArchiveRequests",
                columns: new[] { "DepartmentId", "TargetType", "TargetId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentArchiveLeaders_DepartmentId_UserId",
                table: "DepartmentArchiveLeaders",
                columns: new[] { "DepartmentId", "UserId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentChunks_DocumentId",
                table: "DocumentChunks",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ArchiveRecordId",
                table: "Documents",
                column: "ArchiveRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_CreatedAt",
                table: "Documents",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_FileType",
                table: "Documents",
                column: "FileType");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_PhysicalFileId",
                table: "Documents",
                column: "PhysicalFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_SourceType",
                table: "Documents",
                column: "SourceType");

            migrationBuilder.CreateIndex(
                name: "IX_EditArchiveRequests_ArchiveRecordId",
                table: "EditArchiveRequests",
                column: "ArchiveRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_EditArchiveRequests_DepartmentId_ArchiveRecordId_Status",
                table: "EditArchiveRequests",
                columns: new[] { "DepartmentId", "ArchiveRecordId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_FolderPermissions_FolderId",
                table: "FolderPermissions",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_IconId",
                table: "Folders",
                column: "IconId");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_ParentId",
                table: "Folders",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_PhysicalFiles_ArchiveRecordId_CreatedAt",
                table: "PhysicalFiles",
                columns: new[] { "ArchiveRecordId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PhysicalFiles_ArchiveRecordId_IsDeleted_FileExtension_Covering",
                table: "PhysicalFiles",
                columns: new[] { "ArchiveRecordId", "IsDeleted", "FileExtension" })
                .Annotation("Npgsql:IndexInclude", new[] { "FileSize", "ContentType", "FileName", "CreatedAt", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PhysicalFiles_EditArchiveRequestId",
                table: "PhysicalFiles",
                column: "EditArchiveRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArchiveAuditLogs");

            migrationBuilder.DropTable(
                name: "ArchiveConfigs");

            migrationBuilder.DropTable(
                name: "ArchiveRecordFormInputValues");

            migrationBuilder.DropTable(
                name: "DeleteArchiveRequests");

            migrationBuilder.DropTable(
                name: "DepartmentArchiveLeaders");

            migrationBuilder.DropTable(
                name: "DocumentChunks");

            migrationBuilder.DropTable(
                name: "FolderPermissions");

            migrationBuilder.DropTable(
                name: "ArchiveRecordTemplateValues");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "PhysicalFiles");

            migrationBuilder.DropTable(
                name: "EditArchiveRequests");

            migrationBuilder.DropTable(
                name: "ArchiveRecords");

            migrationBuilder.DropTable(
                name: "DynamicForms");

            migrationBuilder.DropTable(
                name: "Folders");

            migrationBuilder.DropTable(
                name: "FolderIcons");
        }
    }
}
