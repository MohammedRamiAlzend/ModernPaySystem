using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModernPaySystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MoveStatusToRequestAndUpdateRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResponseTransactionAttachments");

            migrationBuilder.DropTable(
                name: "ResponseTransactions");

            migrationBuilder.AddColumn<Guid>(
                name: "CurrentTransactionId",
                table: "Requests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FirstTransactionId",
                table: "Requests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Requests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "RequestTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_RequestTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestTransactions_RequestTransactions_ParentTransactionId",
                        column: x => x.ParentTransactionId,
                        principalTable: "RequestTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RequestTransactions_Requests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "Requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequestTransactions_Users_CurrentUserHolderId",
                        column: x => x.CurrentUserHolderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RequestTransactionAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestTransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttachmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestTransactionAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestTransactionAttachments_Attachments_AttachmentId",
                        column: x => x.AttachmentId,
                        principalTable: "Attachments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequestTransactionAttachments_RequestTransactions_RequestTr~",
                        column: x => x.RequestTransactionId,
                        principalTable: "RequestTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Requests_CurrentTransactionId",
                table: "Requests",
                column: "CurrentTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_FirstTransactionId",
                table: "Requests",
                column: "FirstTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestTransactionAttachments_AttachmentId",
                table: "RequestTransactionAttachments",
                column: "AttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestTransactionAttachments_RequestTransactionId",
                table: "RequestTransactionAttachments",
                column: "RequestTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestTransactions_CurrentUserHolderId",
                table: "RequestTransactions",
                column: "CurrentUserHolderId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestTransactions_ParentTransactionId",
                table: "RequestTransactions",
                column: "ParentTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestTransactions_RequestId",
                table: "RequestTransactions",
                column: "RequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_RequestTransactions_CurrentTransactionId",
                table: "Requests",
                column: "CurrentTransactionId",
                principalTable: "RequestTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_RequestTransactions_FirstTransactionId",
                table: "Requests",
                column: "FirstTransactionId",
                principalTable: "RequestTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requests_RequestTransactions_CurrentTransactionId",
                table: "Requests");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_RequestTransactions_FirstTransactionId",
                table: "Requests");

            migrationBuilder.DropTable(
                name: "RequestTransactionAttachments");

            migrationBuilder.DropTable(
                name: "RequestTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Requests_CurrentTransactionId",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_FirstTransactionId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "CurrentTransactionId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "FirstTransactionId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Requests");

            migrationBuilder.CreateTable(
                name: "ResponseTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentUserHolderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentTransactionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResponseId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    Path = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponseTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResponseTransactions_ResponseTransactions_ParentTransaction~",
                        column: x => x.ParentTransactionId,
                        principalTable: "ResponseTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ResponseTransactionAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AttachmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponseTransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true)
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
    }
}
