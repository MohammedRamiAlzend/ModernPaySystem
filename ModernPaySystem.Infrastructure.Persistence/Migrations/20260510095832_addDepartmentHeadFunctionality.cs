using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModernPaySystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addDepartmentHeadFunctionality : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "HeadedDepartmentId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentHeadId",
                table: "Departments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Departments_DepartmentHeadId",
                table: "Departments",
                column: "DepartmentHeadId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Users_DepartmentHeadId",
                table: "Departments",
                column: "DepartmentHeadId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Users_DepartmentHeadId",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Departments_DepartmentHeadId",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "HeadedDepartmentId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DepartmentHeadId",
                table: "Departments");
        }
    }
}
