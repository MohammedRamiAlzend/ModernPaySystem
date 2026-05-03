using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModernPaySystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTemplateOwnerShipFromUserToDeparmnet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TemplateOwnerships_Users_UserId",
                table: "TemplateOwnerships");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "TemplateOwnerships",
                newName: "DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_TemplateOwnerships_UserId",
                table: "TemplateOwnerships",
                newName: "IX_TemplateOwnerships_DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_TemplateOwnerships_Departments_DepartmentId",
                table: "TemplateOwnerships",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TemplateOwnerships_Departments_DepartmentId",
                table: "TemplateOwnerships");

            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                table: "TemplateOwnerships",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_TemplateOwnerships_DepartmentId",
                table: "TemplateOwnerships",
                newName: "IX_TemplateOwnerships_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TemplateOwnerships_Users_UserId",
                table: "TemplateOwnerships",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
