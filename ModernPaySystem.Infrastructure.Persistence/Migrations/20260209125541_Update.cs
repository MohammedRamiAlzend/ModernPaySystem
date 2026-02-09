using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModernPaySystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentAsJson",
                table: "Requests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "RequestAttachments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "RequestAttachments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "RequestAttachments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "RequestAttachments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByUserId",
                table: "RequestAttachments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentAsJson",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "RequestAttachments");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "RequestAttachments");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "RequestAttachments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "RequestAttachments");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "RequestAttachments");
        }
    }
}
