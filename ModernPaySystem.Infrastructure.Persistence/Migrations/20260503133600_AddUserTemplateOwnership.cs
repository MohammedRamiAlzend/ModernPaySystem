using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModernPaySystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTemplateOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserTemplateOwnerships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTemplateOwnerships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTemplateOwnerships_Templates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "Templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserTemplateOwnerships_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserTemplateOwnerships_TemplateId",
                table: "UserTemplateOwnerships",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTemplateOwnerships_UserId",
                table: "UserTemplateOwnerships",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserTemplateOwnerships");
        }
    }
}
