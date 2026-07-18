using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModernPaySystem.Module.Archive.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentIdToFolderPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "FolderPermissions",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                table: "FolderPermissions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FolderPermissions_DepartmentId",
                table: "FolderPermissions",
                column: "DepartmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FolderPermissions_DepartmentId",
                table: "FolderPermissions");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "FolderPermissions");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "FolderPermissions",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(450)",
                oldMaxLength: 450,
                oldNullable: true);
        }
    }
}
