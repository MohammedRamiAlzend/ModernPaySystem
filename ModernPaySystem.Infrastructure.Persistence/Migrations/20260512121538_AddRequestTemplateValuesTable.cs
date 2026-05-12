using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModernPaySystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestTemplateValuesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Templates_TemplateId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "ContentAsJson",
                table: "Requests");

            migrationBuilder.AlterColumn<Guid>(
                name: "TemplateId",
                table: "Requests",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "RequestTemplateValuesId",
                table: "Requests",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "RequestTemplateValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestTemplateValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestTemplateValues_Requests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "Requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequestTemplateValues_Templates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "Templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InputValue",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestTemplateValuesId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InputValue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InputValue_RequestTemplateValues_RequestTemplateValuesId",
                        column: x => x.RequestTemplateValuesId,
                        principalTable: "RequestTemplateValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InputValue_RequestTemplateValuesId",
                table: "InputValue",
                column: "RequestTemplateValuesId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestTemplateValues_RequestId",
                table: "RequestTemplateValues",
                column: "RequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequestTemplateValues_TemplateId",
                table: "RequestTemplateValues",
                column: "TemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Templates_TemplateId",
                table: "Requests",
                column: "TemplateId",
                principalTable: "Templates",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Templates_TemplateId",
                table: "Requests");

            migrationBuilder.DropTable(
                name: "InputValue");

            migrationBuilder.DropTable(
                name: "RequestTemplateValues");

            migrationBuilder.DropColumn(
                name: "RequestTemplateValuesId",
                table: "Requests");

            migrationBuilder.AlterColumn<Guid>(
                name: "TemplateId",
                table: "Requests",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContentAsJson",
                table: "Requests",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Templates_TemplateId",
                table: "Requests",
                column: "TemplateId",
                principalTable: "Templates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
