using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModernPaySystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMentionedUsersToRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RequestUser",
                columns: table => new
                {
                    MentionedRequestsId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReadOnlyUsersId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestUser", x => new { x.MentionedRequestsId, x.ReadOnlyUsersId });
                    table.ForeignKey(
                        name: "FK_RequestUser_Requests_MentionedRequestsId",
                        column: x => x.MentionedRequestsId,
                        principalTable: "Requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequestUser_Users_ReadOnlyUsersId",
                        column: x => x.ReadOnlyUsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequestUser_ReadOnlyUsersId",
                table: "RequestUser",
                column: "ReadOnlyUsersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequestUser");
        }
    }
}
