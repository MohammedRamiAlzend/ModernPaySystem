using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModernPaySystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MigrationTest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ResponseId1",
                table: "ResponseAttachments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResponseAttachments_ResponseId1",
                table: "ResponseAttachments",
                column: "ResponseId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ResponseAttachments_Responses_ResponseId1",
                table: "ResponseAttachments",
                column: "ResponseId1",
                principalTable: "Responses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ResponseAttachments_Responses_ResponseId1",
                table: "ResponseAttachments");

            migrationBuilder.DropIndex(
                name: "IX_ResponseAttachments_ResponseId1",
                table: "ResponseAttachments");

            migrationBuilder.DropColumn(
                name: "ResponseId1",
                table: "ResponseAttachments");
        }
    }
}
