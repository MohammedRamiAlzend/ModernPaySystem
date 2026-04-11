using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModernPaySystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddResponseTransactionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ResponseTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Path = table.Column<string>(type: "text", nullable: false),
                    ParentTransactionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CurrentUserHolderId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponseTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResponseTransactions_ResponseTransactions_ParentTransaction~",
                        column: x => x.ParentTransactionId,
                        principalTable: "ResponseTransactions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ResponseTransactions_Responses_ResponseId",
                        column: x => x.ResponseId,
                        principalTable: "Responses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResponseTransactions_Users_CurrentUserHolderId",
                        column: x => x.CurrentUserHolderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResponseTransactionAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponseTransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttachmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponseTransactionAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResponseTransactionAttachments_Attachments_AttachmentId",
                        column: x => x.AttachmentId,
                        principalTable: "Attachments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResponseTransactionAttachments_ResponseTransactions_Respons~",
                        column: x => x.ResponseTransactionId,
                        principalTable: "ResponseTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResponseTransactionAttachments_AttachmentId",
                table: "ResponseTransactionAttachments",
                column: "AttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ResponseTransactionAttachments_ResponseTransactionId",
                table: "ResponseTransactionAttachments",
                column: "ResponseTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_ResponseTransactions_CurrentUserHolderId",
                table: "ResponseTransactions",
                column: "CurrentUserHolderId");

            migrationBuilder.CreateIndex(
                name: "IX_ResponseTransactions_ParentTransactionId",
                table: "ResponseTransactions",
                column: "ParentTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_ResponseTransactions_ResponseId",
                table: "ResponseTransactions",
                column: "ResponseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResponseTransactionAttachments");

            migrationBuilder.DropTable(
                name: "ResponseTransactions");
        }
    }
}
