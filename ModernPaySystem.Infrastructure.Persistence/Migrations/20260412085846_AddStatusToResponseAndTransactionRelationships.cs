using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModernPaySystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToResponseAndTransactionRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CurrentTransactionId",
                table: "Responses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FirstTransactionId",
                table: "Responses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Responses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Responses_CurrentTransactionId",
                table: "Responses",
                column: "CurrentTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Responses_FirstTransactionId",
                table: "Responses",
                column: "FirstTransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Responses_ResponseTransactions_CurrentTransactionId",
                table: "Responses",
                column: "CurrentTransactionId",
                principalTable: "ResponseTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Responses_ResponseTransactions_FirstTransactionId",
                table: "Responses",
                column: "FirstTransactionId",
                principalTable: "ResponseTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Responses_ResponseTransactions_CurrentTransactionId",
                table: "Responses");

            migrationBuilder.DropForeignKey(
                name: "FK_Responses_ResponseTransactions_FirstTransactionId",
                table: "Responses");

            migrationBuilder.DropIndex(
                name: "IX_Responses_CurrentTransactionId",
                table: "Responses");

            migrationBuilder.DropIndex(
                name: "IX_Responses_FirstTransactionId",
                table: "Responses");

            migrationBuilder.DropColumn(
                name: "CurrentTransactionId",
                table: "Responses");

            migrationBuilder.DropColumn(
                name: "FirstTransactionId",
                table: "Responses");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Responses");
        }
    }
}
