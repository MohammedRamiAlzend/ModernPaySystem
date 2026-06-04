using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModernPaySystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class archiveleader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EditArchiveRequestId",
                table: "PhysicalFiles",
                type: "uuid",
                nullable: true);

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
                    OriginalSnapshotJson = table.Column<string>(type: "jsonb", nullable: false),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    ApprovalNotes = table.Column<string>(type: "text", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false),
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
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EditArchiveRequests_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EditArchiveRequests_Users_ApproverId",
                        column: x => x.ApproverId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EditArchiveRequests_Users_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PhysicalFiles_EditArchiveRequestId",
                table: "PhysicalFiles",
                column: "EditArchiveRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_EditArchiveRequests_ApproverId",
                table: "EditArchiveRequests",
                column: "ApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_EditArchiveRequests_ArchiveRecordId",
                table: "EditArchiveRequests",
                column: "ArchiveRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_EditArchiveRequests_DepartmentId_ArchiveRecordId_Status",
                table: "EditArchiveRequests",
                columns: new[] { "DepartmentId", "ArchiveRecordId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_EditArchiveRequests_RequesterId",
                table: "EditArchiveRequests",
                column: "RequesterId");

            migrationBuilder.AddForeignKey(
                name: "FK_PhysicalFiles_EditArchiveRequests_EditArchiveRequestId",
                table: "PhysicalFiles",
                column: "EditArchiveRequestId",
                principalTable: "EditArchiveRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PhysicalFiles_EditArchiveRequests_EditArchiveRequestId",
                table: "PhysicalFiles");

            migrationBuilder.DropTable(
                name: "EditArchiveRequests");

            migrationBuilder.DropIndex(
                name: "IX_PhysicalFiles_EditArchiveRequestId",
                table: "PhysicalFiles");

            migrationBuilder.DropColumn(
                name: "EditArchiveRequestId",
                table: "PhysicalFiles");
        }
    }
}
