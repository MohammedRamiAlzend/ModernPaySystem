using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModernPaySystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPhysicalFileIndexesForPaginatedRetrieval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PhysicalFiles_ArchiveRecordId",
                table: "PhysicalFiles");

            migrationBuilder.CreateIndex(
                name: "IX_PhysicalFiles_ArchiveRecordId_CreatedAt",
                table: "PhysicalFiles",
                columns: new[] { "ArchiveRecordId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PhysicalFiles_ArchiveRecordId_IsDeleted_FileExtension_Covering",
                table: "PhysicalFiles",
                columns: new[] { "ArchiveRecordId", "IsDeleted", "FileExtension" })
                .Annotation("Npgsql:IndexInclude", new[] { "FileSize", "ContentType", "FileName", "CreatedAt", "UpdatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PhysicalFiles_ArchiveRecordId_CreatedAt",
                table: "PhysicalFiles");

            migrationBuilder.DropIndex(
                name: "IX_PhysicalFiles_ArchiveRecordId_IsDeleted_FileExtension_Covering",
                table: "PhysicalFiles");

            migrationBuilder.CreateIndex(
                name: "IX_PhysicalFiles_ArchiveRecordId",
                table: "PhysicalFiles",
                column: "ArchiveRecordId");
        }
    }
}
