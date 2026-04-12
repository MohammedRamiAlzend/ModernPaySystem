using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModernPaySystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddResponseTransactionTable2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ResponseTransactions_ResponseTransactions_ParentTransaction~",
                table: "ResponseTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_ResponseTransactions_Users_CurrentUserHolderId",
                table: "ResponseTransactions");

            migrationBuilder.AddForeignKey(
                name: "FK_ResponseTransactions_ResponseTransactions_ParentTransaction~",
                table: "ResponseTransactions",
                column: "ParentTransactionId",
                principalTable: "ResponseTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ResponseTransactions_Users_CurrentUserHolderId",
                table: "ResponseTransactions",
                column: "CurrentUserHolderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ResponseTransactions_ResponseTransactions_ParentTransaction~",
                table: "ResponseTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_ResponseTransactions_Users_CurrentUserHolderId",
                table: "ResponseTransactions");

            migrationBuilder.AddForeignKey(
                name: "FK_ResponseTransactions_ResponseTransactions_ParentTransaction~",
                table: "ResponseTransactions",
                column: "ParentTransactionId",
                principalTable: "ResponseTransactions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ResponseTransactions_Users_CurrentUserHolderId",
                table: "ResponseTransactions",
                column: "CurrentUserHolderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
