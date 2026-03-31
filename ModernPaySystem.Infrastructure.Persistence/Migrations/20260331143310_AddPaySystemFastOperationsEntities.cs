using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModernPaySystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPaySystemFastOperationsEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Genders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Desc = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Govs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Desc = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Govs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KindShips",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Desc = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KindShips", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Nationals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Desc = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nationals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OperationServiceTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Desc = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationServiceTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OperationStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Desc = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    FatherName = table.Column<string>(type: "text", nullable: false),
                    MotherName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    PlaceBirth = table.Column<string>(type: "text", nullable: false),
                    DateBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    NationalityNumber = table.Column<string>(type: "text", nullable: false),
                    GenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    NationalId = table.Column<Guid>(type: "uuid", nullable: false),
                    GovId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clients_Genders_GenderId",
                        column: x => x.GenderId,
                        principalTable: "Genders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Clients_Govs_GovId",
                        column: x => x.GovId,
                        principalTable: "Govs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Clients_Nationals_NationalId",
                        column: x => x.NationalId,
                        principalTable: "Nationals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Operations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicantClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    KindShipId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationStatusId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationServiceTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SumAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Operations_Clients_ApplicantClientId",
                        column: x => x.ApplicantClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Operations_Clients_RecipientClientId",
                        column: x => x.RecipientClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Operations_KindShips_KindShipId",
                        column: x => x.KindShipId,
                        principalTable: "KindShips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Operations_OperationServiceTypes_OperationServiceTypeId",
                        column: x => x.OperationServiceTypeId,
                        principalTable: "OperationServiceTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Operations_OperationStatuses_OperationStatusId",
                        column: x => x.OperationStatusId,
                        principalTable: "OperationStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_GenderId",
                table: "Clients",
                column: "GenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_GovId",
                table: "Clients",
                column: "GovId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_NationalId",
                table: "Clients",
                column: "NationalId");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_ApplicantClientId",
                table: "Operations",
                column: "ApplicantClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_KindShipId",
                table: "Operations",
                column: "KindShipId");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_OperationServiceTypeId",
                table: "Operations",
                column: "OperationServiceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_OperationStatusId",
                table: "Operations",
                column: "OperationStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_RecipientClientId",
                table: "Operations",
                column: "RecipientClientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Operations");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "KindShips");

            migrationBuilder.DropTable(
                name: "OperationServiceTypes");

            migrationBuilder.DropTable(
                name: "OperationStatuses");

            migrationBuilder.DropTable(
                name: "Genders");

            migrationBuilder.DropTable(
                name: "Govs");

            migrationBuilder.DropTable(
                name: "Nationals");
        }
    }
}
