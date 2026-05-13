using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModernPaySystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesForInputValueLookupAndRequestTemplateValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InputValue_RequestTemplateValuesId",
                table: "InputValue");

            migrationBuilder.CreateIndex(
                name: "IX_InputValue_Lookup",
                table: "InputValue",
                columns: new[] { "RequestTemplateValuesId", "Key", "Value" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InputValue_Lookup",
                table: "InputValue");

            migrationBuilder.CreateIndex(
                name: "IX_InputValue_RequestTemplateValuesId",
                table: "InputValue",
                column: "RequestTemplateValuesId");
        }
    }
}
