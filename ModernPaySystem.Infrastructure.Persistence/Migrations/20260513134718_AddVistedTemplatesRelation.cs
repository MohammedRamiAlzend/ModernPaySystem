using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModernPaySystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVistedTemplatesRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserVisitedTemplates",
                columns: table => new
                {
                    VisitedByUsersId = table.Column<Guid>(type: "uuid", nullable: false),
                    VisitedTemplatesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserVisitedTemplates", x => new { x.VisitedByUsersId, x.VisitedTemplatesId });
                    table.ForeignKey(
                        name: "FK_UserVisitedTemplates_Templates_VisitedTemplatesId",
                        column: x => x.VisitedTemplatesId,
                        principalTable: "Templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserVisitedTemplates_Users_VisitedByUsersId",
                        column: x => x.VisitedByUsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserVisitedTemplates_VisitedTemplatesId",
                table: "UserVisitedTemplates",
                column: "VisitedTemplatesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserVisitedTemplates");
        }
    }
}
