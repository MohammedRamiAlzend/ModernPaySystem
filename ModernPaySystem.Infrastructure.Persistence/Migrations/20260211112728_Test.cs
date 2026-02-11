using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModernPaySystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Test : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ResponseId",
                table: "Requests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ResponseId1",
                table: "Requests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Requests_ResponseId1",
                table: "Requests",
                column: "ResponseId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Responses_ResponseId1",
                table: "Requests",
                column: "ResponseId1",
                principalTable: "Responses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Responses_ResponseId1",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_ResponseId1",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "ResponseId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "ResponseId1",
                table: "Requests");
        }
    }
}
