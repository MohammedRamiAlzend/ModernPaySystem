using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModernPaySystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFormIdRelationToAcceptNullForArchiveRecored : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArchiveRecordTemplateValues_ArchiveRecords_ArchiveRecordId1",
                table: "ArchiveRecordTemplateValues");

            migrationBuilder.AlterColumn<Guid>(
                name: "ArchiveRecordId1",
                table: "ArchiveRecordTemplateValues",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "FormId",
                table: "ArchiveRecords",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "ArchiveRecordTemplateValues",
                table: "ArchiveRecords",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_ArchiveRecordTemplateValues_ArchiveRecords_ArchiveRecordId1",
                table: "ArchiveRecordTemplateValues",
                column: "ArchiveRecordId1",
                principalTable: "ArchiveRecords",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArchiveRecordTemplateValues_ArchiveRecords_ArchiveRecordId1",
                table: "ArchiveRecordTemplateValues");

            migrationBuilder.AlterColumn<Guid>(
                name: "ArchiveRecordId1",
                table: "ArchiveRecordTemplateValues",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "FormId",
                table: "ArchiveRecords",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ArchiveRecordTemplateValues",
                table: "ArchiveRecords",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ArchiveRecordTemplateValues_ArchiveRecords_ArchiveRecordId1",
                table: "ArchiveRecordTemplateValues",
                column: "ArchiveRecordId1",
                principalTable: "ArchiveRecords",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
