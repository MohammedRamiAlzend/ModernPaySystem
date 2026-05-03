namespace ModernPaySystem.Infrastructure.Persistence.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;
    using System;

    /// <inheritdoc />

    /// <summary>
    /// Defines the <see cref="AddMentionedUsersToRequest" />
    /// </summary>
    public partial class AddMentionedUsersToRequest : Migration
    {
        /// <inheritdoc />

        /// <summary>
        /// The Up
        /// </summary>
        /// <param name="migrationBuilder">The migrationBuilder<see cref="MigrationBuilder"/></param>
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

        /// <summary>
        /// The Down
        /// </summary>
        /// <param name="migrationBuilder">The migrationBuilder<see cref="MigrationBuilder"/></param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequestUser");
        }
    }
}
