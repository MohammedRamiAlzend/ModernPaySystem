using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModernPaySystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addDefaultReceiverDepartmentIdAndIsRequireAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DefaultReceiverDepartmentId",
                table: "Templates",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsRequireAttachments",
                table: "Templates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Templates_DefaultReceiverDepartmentId",
                table: "Templates",
                column: "DefaultReceiverDepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Templates_Departments_DefaultReceiverDepartmentId",
                table: "Templates",
                column: "DefaultReceiverDepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Templates_Departments_DefaultReceiverDepartmentId",
                table: "Templates");

            migrationBuilder.DropIndex(
                name: "IX_Templates_DefaultReceiverDepartmentId",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "DefaultReceiverDepartmentId",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "IsRequireAttachments",
                table: "Templates");
        }
    }
}
