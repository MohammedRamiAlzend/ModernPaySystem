using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModernPaySystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddArchiveDeletionWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeletedByUserId",
                table: "PhysicalFiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Folders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByRequestId",
                table: "Folders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByUserId",
                table: "Folders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                table: "Folders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Folders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedByRequestId",
                table: "ArchiveRecords",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ArchiveRecords",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByRequestId",
                table: "ArchiveRecords",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByUserId",
                table: "ArchiveRecords",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                table: "ArchiveRecords",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ArchiveRecords",
                type: "boolean",
                nullable: false,
                defaultValue: false);

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
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeleteArchiveRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeleteArchiveRequests_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeleteArchiveRequests_Users_ApproverId",
                        column: x => x.ApproverId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeleteArchiveRequests_Users_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    table.ForeignKey(
                        name: "FK_DepartmentArchiveLeaders_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DepartmentArchiveLeaders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Folders_DepartmentId",
                table: "Folders",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveRecords_DepartmentId",
                table: "ArchiveRecords",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_DeleteArchiveRequests_ApproverId",
                table: "DeleteArchiveRequests",
                column: "ApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_DeleteArchiveRequests_DepartmentId_TargetType_TargetId_Stat~",
                table: "DeleteArchiveRequests",
                columns: new[] { "DepartmentId", "TargetType", "TargetId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_DeleteArchiveRequests_RequesterId",
                table: "DeleteArchiveRequests",
                column: "RequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentArchiveLeaders_DepartmentId_UserId",
                table: "DepartmentArchiveLeaders",
                columns: new[] { "DepartmentId", "UserId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentArchiveLeaders_UserId",
                table: "DepartmentArchiveLeaders",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ArchiveRecords_Departments_DepartmentId",
                table: "ArchiveRecords",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Folders_Departments_DepartmentId",
                table: "Folders",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArchiveRecords_Departments_DepartmentId",
                table: "ArchiveRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_Folders_Departments_DepartmentId",
                table: "Folders");

            migrationBuilder.DropTable(
                name: "DeleteArchiveRequests");

            migrationBuilder.DropTable(
                name: "DepartmentArchiveLeaders");

            migrationBuilder.DropIndex(
                name: "IX_Folders_DepartmentId",
                table: "Folders");

            migrationBuilder.DropIndex(
                name: "IX_ArchiveRecords_DepartmentId",
                table: "ArchiveRecords");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "PhysicalFiles");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "DeletedByRequestId",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "ApprovedByRequestId",
                table: "ArchiveRecords");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ArchiveRecords");

            migrationBuilder.DropColumn(
                name: "DeletedByRequestId",
                table: "ArchiveRecords");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "ArchiveRecords");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "ArchiveRecords");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ArchiveRecords");
        }
    }
}
