using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModernPaySystem.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddLookUpFieldEntities : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "LookUpFields",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FiledName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LookUpFields", x => x.Id);
                table.ForeignKey(
                    name: "FK_LookUpFields_Templates_TemplateId",
                    column: x => x.TemplateId,
                    principalTable: "Templates",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "LookUpFiledValues",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                LookUpFiledId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Desc = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LookUpFiledValues", x => x.Id);
                table.ForeignKey(
                    name: "FK_LookUpFiledValues_LookUpFields_LookUpFiledId",
                    column: x => x.LookUpFiledId,
                    principalTable: "LookUpFields",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_LookUpFields_TemplateId",
            table: "LookUpFields",
            column: "TemplateId");

        migrationBuilder.CreateIndex(
            name: "IX_LookUpFiledValues_LookUpFiledId",
            table: "LookUpFiledValues",
            column: "LookUpFiledId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "LookUpFiledValues");

        migrationBuilder.DropTable(
            name: "LookUpFields");
    }
}
