using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModernPaySystem.Module.Transaction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    SafeName = table.Column<string>(type: "text", nullable: false),
                    Extension = table.Column<string>(type: "text", nullable: false),
                    Path = table.Column<string>(type: "text", nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Responses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    RespondedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Responses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentAsJson = table.Column<string>(type: "jsonb", nullable: false),
                    TemplateName = table.Column<string>(type: "text", nullable: false),
                    TemplateDescription = table.Column<string>(type: "text", nullable: true),
                    IsRequireAttachments = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultReceiverDepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ResponseAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponseId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttachmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponseAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResponseAttachments_Attachments_AttachmentId",
                        column: x => x.AttachmentId,
                        principalTable: "Attachments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResponseAttachments_Responses_ResponseId",
                        column: x => x.ResponseId,
                        principalTable: "Responses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LookUpFields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FiledName = table.Column<string>(type: "text", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookUpFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LookUpFields_Templates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "Templates",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TemplateDepartmentOwnerships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateDepartmentOwnerships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplateDepartmentOwnerships_Templates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "Templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTemplateOwnerships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTemplateOwnerships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTemplateOwnerships_Templates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "Templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LookUpFiledValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LookUpFiledId = table.Column<Guid>(type: "uuid", nullable: false),
                    Desc = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookUpFiledValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LookUpFiledValues_LookUpFields_LookUpFiledId",
                        column: x => x.LookUpFiledId,
                        principalTable: "LookUpFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InputValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestTemplateValuesId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InputValues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RequestAttachments",
                columns: table => new
                {
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttachmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestAttachments", x => new { x.RequestId, x.AttachmentId });
                    table.ForeignKey(
                        name: "FK_RequestAttachments_Attachments_AttachmentId",
                        column: x => x.AttachmentId,
                        principalTable: "Attachments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RequestAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RequestRelations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelationType = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestRelations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestTemplateValuesId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequesterId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequesterDepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApproverId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApproverDepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponseId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    FirstTransactionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CurrentTransactionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReadOnlyUsers = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Requests_Responses_ResponseId",
                        column: x => x.ResponseId,
                        principalTable: "Responses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Requests_Templates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "Templates",
                        principalColumn: "Id");
                });

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
                name: "IX_InputValue_Lookup",
                table: "InputValues",
                columns: new[] { "RequestTemplateValuesId", "Key", "Value" });

            migrationBuilder.CreateIndex(
                name: "IX_LookUpFields_TemplateId",
                table: "LookUpFields",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_LookUpFiledValues_LookUpFiledId",
                table: "LookUpFiledValues",
                column: "LookUpFiledId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestAttachments_AttachmentId",
                table: "RequestAttachments",
                column: "AttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestAuditLogs_Action",
                table: "RequestAuditLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_RequestAuditLogs_RequestId_Timestamp",
                table: "RequestAuditLogs",
                columns: new[] { "RequestId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_RequestAuditLogs_UserId",
                table: "RequestAuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestRelations_SourceRequestId_TargetRequestId_RelationTy~",
                table: "RequestRelations",
                columns: new[] { "SourceRequestId", "TargetRequestId", "RelationType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequestRelations_TargetRequestId",
                table: "RequestRelations",
                column: "TargetRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_CurrentTransactionId",
                table: "Requests",
                column: "CurrentTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_FirstTransactionId",
                table: "Requests",
                column: "FirstTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_ResponseId",
                table: "Requests",
                column: "ResponseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Requests_TemplateId",
                table: "Requests",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestTemplateValues_RequestId",
                table: "RequestTemplateValues",
                column: "RequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequestTemplateValues_TemplateId",
                table: "RequestTemplateValues",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestTransactionAttachments_AttachmentId",
                table: "RequestTransactionAttachments",
                column: "AttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestTransactionAttachments_RequestTransactionId",
                table: "RequestTransactionAttachments",
                column: "RequestTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestTransactions_ParentTransactionId",
                table: "RequestTransactions",
                column: "ParentTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestTransactions_RequestId",
                table: "RequestTransactions",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ResponseAttachments_AttachmentId",
                table: "ResponseAttachments",
                column: "AttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ResponseAttachments_ResponseId",
                table: "ResponseAttachments",
                column: "ResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateDepartmentOwnerships_TemplateId",
                table: "TemplateDepartmentOwnerships",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTemplateOwnerships_TemplateId",
                table: "UserTemplateOwnerships",
                column: "TemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_InputValues_RequestTemplateValues_RequestTemplateValuesId",
                table: "InputValues",
                column: "RequestTemplateValuesId",
                principalTable: "RequestTemplateValues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RequestAttachments_Requests_RequestId",
                table: "RequestAttachments",
                column: "RequestId",
                principalTable: "Requests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RequestAuditLogs_Requests_RequestId",
                table: "RequestAuditLogs",
                column: "RequestId",
                principalTable: "Requests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RequestRelations_Requests_SourceRequestId",
                table: "RequestRelations",
                column: "SourceRequestId",
                principalTable: "Requests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RequestRelations_Requests_TargetRequestId",
                table: "RequestRelations",
                column: "TargetRequestId",
                principalTable: "Requests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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
                name: "FK_Requests_Templates_TemplateId",
                table: "Requests");

            migrationBuilder.DropForeignKey(
                name: "FK_RequestTransactions_Requests_RequestId",
                table: "RequestTransactions");

            migrationBuilder.DropTable(
                name: "InputValues");

            migrationBuilder.DropTable(
                name: "LookUpFiledValues");

            migrationBuilder.DropTable(
                name: "RequestAttachments");

            migrationBuilder.DropTable(
                name: "RequestAuditLogs");

            migrationBuilder.DropTable(
                name: "RequestRelations");

            migrationBuilder.DropTable(
                name: "RequestTransactionAttachments");

            migrationBuilder.DropTable(
                name: "ResponseAttachments");

            migrationBuilder.DropTable(
                name: "TemplateDepartmentOwnerships");

            migrationBuilder.DropTable(
                name: "UserTemplateOwnerships");

            migrationBuilder.DropTable(
                name: "RequestTemplateValues");

            migrationBuilder.DropTable(
                name: "LookUpFields");

            migrationBuilder.DropTable(
                name: "Attachments");

            migrationBuilder.DropTable(
                name: "Templates");

            migrationBuilder.DropTable(
                name: "Requests");

            migrationBuilder.DropTable(
                name: "RequestTransactions");

            migrationBuilder.DropTable(
                name: "Responses");
        }
    }
}
