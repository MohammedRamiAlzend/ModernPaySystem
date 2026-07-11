using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModernPaySystem.Module.Transaction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentTemplateNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DepartmentTemplateNumbers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastRequestNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentTemplateNumbers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentTemplateNumbers_DepartmentId_TemplateId",
                table: "DepartmentTemplateNumbers",
                columns: new[] { "DepartmentId", "TemplateId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DepartmentTemplateNumbers");
        }
    }
}
