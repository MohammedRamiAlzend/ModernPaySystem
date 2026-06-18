using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModernPaySystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveArchivalNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArchivalNumber",
                table: "ArchiveRecords");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ArchivalNumber",
                table: "ArchiveRecords",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
