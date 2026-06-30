using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModernPaySystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArchiveConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DefaultPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AllowedFileExtensions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveConfigs", x => x.Id);
                });

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
                name: "DynamicForms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FormName = table.Column<string>(type: "text", nullable: false),
                    ContentAsJson = table.Column<string>(type: "jsonb", nullable: false),
                    FormDescription = table.Column<string>(type: "text", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicForms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FolderIcons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SvgContent = table.Column<string>(type: "text", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FolderIcons", x => x.Id);
                });

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
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    SubSystem = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
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
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
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
                name: "RolePermissions",
                columns: table => new
                {
                    PermissionsId = table.Column<Guid>(type: "uuid", nullable: false),
                    RolesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.PermissionsId, x.RolesId });
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionsId",
                        column: x => x.PermissionsId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RolesId",
                        column: x => x.RolesId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateTable(
                name: "ArchiveAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ArchiveRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
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
                    table.PrimaryKey("PK_ArchiveAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArchiveRecordFormInputValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true),
                    ArchiveRecordTemplateValuesId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveRecordFormInputValues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArchiveRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    FolderId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    FormId = table.Column<Guid>(type: "uuid", nullable: true),
                    ArchiveRecordTemplateValues = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedByUserId = table.Column<string>(type: "text", nullable: true),
                    DeletedByRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedByRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchiveRecords_DynamicForms_FormId",
                        column: x => x.FormId,
                        principalTable: "DynamicForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ArchiveRecordTemplateValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ArchiveRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    ArchiveRecordId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    ArchiveFormTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveRecordTemplateValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchiveRecordTemplateValues_ArchiveRecords_ArchiveRecordId",
                        column: x => x.ArchiveRecordId,
                        principalTable: "ArchiveRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArchiveRecordTemplateValues_ArchiveRecords_ArchiveRecordId1",
                        column: x => x.ArchiveRecordId1,
                        principalTable: "ArchiveRecords",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ArchiveRecordTemplateValues_DynamicForms_ArchiveFormTemplat~",
                        column: x => x.ArchiveFormTemplateId,
                        principalTable: "DynamicForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeleteArchiveRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetType = table.Column<int>(type: "integer", nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequesterId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApproverId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Justification = table.Column<string>(type: "text", nullable: false),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    ApprovalNotes = table.Column<string>(type: "text", nullable: true),
                    TargetSnapshotJson = table.Column<string>(type: "jsonb", nullable: false),
                    DependenciesSnapshotJson = table.Column<string>(type: "jsonb", nullable: false),
                    ActivitySnapshotJson = table.Column<string>(type: "jsonb", nullable: true),
                    SourceFolderId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetDisplayName = table.Column<string>(type: "text", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExecutedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RequesterNotificationMessage = table.Column<string>(type: "text", nullable: true),
                    RequesterNotifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeleteArchiveRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DepartmentArchiveLeaders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentArchiveLeaders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ParentDepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepartmentHeadId = table.Column<Guid>(type: "uuid", nullable: true),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    MaterializedPath = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Departments_Departments_ParentDepartmentId",
                        column: x => x.ParentDepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Folders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DefaultStoragePath = table.Column<string>(type: "text", nullable: true),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    IconId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedByUserId = table.Column<string>(type: "text", nullable: true),
                    DeletedByRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Folders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Folders_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Folders_FolderIcons_IconId",
                        column: x => x.IconId,
                        principalTable: "FolderIcons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Folders_Folders_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Folders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentAsJson = table.Column<string>(type: "text", nullable: false),
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
                    table.ForeignKey(
                        name: "FK_Templates_Departments_DefaultReceiverDepartmentId",
                        column: x => x.DefaultReceiverDepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    HashedPassword = table.Column<string>(type: "text", nullable: false),
                    SubSystemUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDepartmentHead = table.Column<bool>(type: "boolean", nullable: false),
                    HeadedDepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "FolderPermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FolderId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    AccessLevel = table.Column<int>(type: "integer", nullable: false),
                    IsInherited = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FolderPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FolderPermissions_Folders_FolderId",
                        column: x => x.FolderId,
                        principalTable: "Folders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DepartmentTemplateNumbers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastRequestNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentTemplateNumbers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepartmentTemplateNumbers_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepartmentTemplateNumbers_Templates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "Templates",
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
                name: "TemplateOwnerships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateOwnerships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplateOwnerships_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TemplateOwnerships_Templates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "Templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EditArchiveRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ArchiveRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequesterId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApproverId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Justification = table.Column<string>(type: "text", nullable: false),
                    RequestedChangesJson = table.Column<string>(type: "jsonb", nullable: false),
                    RequestedRecordName = table.Column<string>(type: "text", nullable: true),
                    RequestedFileDeletionIdsJson = table.Column<string>(type: "text", nullable: true),
                    OriginalSnapshotJson = table.Column<string>(type: "jsonb", nullable: false),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    ApprovalNotes = table.Column<string>(type: "text", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EditArchiveRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EditArchiveRequests_ArchiveRecords_ArchiveRecordId",
                        column: x => x.ArchiveRecordId,
                        principalTable: "ArchiveRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EditArchiveRequests_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EditArchiveRequests_Users_ApproverId",
                        column: x => x.ApproverId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EditArchiveRequests_Users_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubSystemUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubSystem = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubSystemUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubSystemUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    RolesId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsersId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.RolesId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RolesId",
                        column: x => x.RolesId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
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
                    table.ForeignKey(
                        name: "FK_UserTemplateOwnerships_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserVisitedTemplates",
                columns: table => new
                {
                    VisitedByUsersId = table.Column<Guid>(type: "uuid", nullable: false),
                    VisitedTemplatesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserVisitedTemplates", x => new { x.VisitedByUsersId, x.VisitedTemplatesId });
                    table.ForeignKey(
                        name: "FK_UserVisitedTemplates_Templates_VisitedTemplatesId",
                        column: x => x.VisitedTemplatesId,
                        principalTable: "Templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserVisitedTemplates_Users_VisitedByUsersId",
                        column: x => x.VisitedByUsersId,
                        principalTable: "Users",
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
                name: "PhysicalFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ArchiveRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    EditArchiveRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    FileExtension = table.Column<string>(type: "text", nullable: false),
                    StoragePath = table.Column<string>(type: "text", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: false),
                    IsQrPage = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhysicalFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhysicalFiles_ArchiveRecords_ArchiveRecordId",
                        column: x => x.ArchiveRecordId,
                        principalTable: "ArchiveRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PhysicalFiles_EditArchiveRequests_EditArchiveRequestId",
                        column: x => x.EditArchiveRequestId,
                        principalTable: "EditArchiveRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    PhysicalFileId = table.Column<Guid>(type: "uuid", nullable: true),
                    ArchiveRecordId = table.Column<Guid>(type: "uuid", nullable: true),
                    FileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    TotalChunks = table.Column<int>(type: "integer", nullable: false),
                    ExtractedText = table.Column<string>(type: "text", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_ArchiveRecords_ArchiveRecordId",
                        column: x => x.ArchiveRecordId,
                        principalTable: "ArchiveRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Documents_PhysicalFiles_PhysicalFileId",
                        column: x => x.PhysicalFileId,
                        principalTable: "PhysicalFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DocumentChunks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChunkIndex = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    TokenCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentChunks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentChunks_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
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
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InputValue", x => x.Id);
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
                    table.ForeignKey(
                        name: "FK_RequestAuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    table.ForeignKey(
                        name: "FK_Requests_Users_ApproverId",
                        column: x => x.ApproverId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Requests_Users_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    table.ForeignKey(
                        name: "FK_RequestTransactions_Users_CurrentUserHolderId",
                        column: x => x.CurrentUserHolderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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
                name: "IX_ArchiveAuditLogs_Action",
                table: "ArchiveAuditLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveAuditLogs_ArchiveRecordId_Timestamp",
                table: "ArchiveAuditLogs",
                columns: new[] { "ArchiveRecordId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveAuditLogs_UserId",
                table: "ArchiveAuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveConfigs_IsActive",
                table: "ArchiveConfigs",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveRecordFormInputValues_ArchiveRecordTemplateValuesId",
                table: "ArchiveRecordFormInputValues",
                column: "ArchiveRecordTemplateValuesId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveRecords_DepartmentId",
                table: "ArchiveRecords",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveRecords_FolderId",
                table: "ArchiveRecords",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveRecords_FormId",
                table: "ArchiveRecords",
                column: "FormId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveRecordTemplateValues_ArchiveFormTemplateId",
                table: "ArchiveRecordTemplateValues",
                column: "ArchiveFormTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveRecordTemplateValues_ArchiveRecordId",
                table: "ArchiveRecordTemplateValues",
                column: "ArchiveRecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveRecordTemplateValues_ArchiveRecordId1",
                table: "ArchiveRecordTemplateValues",
                column: "ArchiveRecordId1");

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
                name: "IX_DeleteArchiveRequests_ApproverId",
                table: "DeleteArchiveRequests",
                column: "ApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_DeleteArchiveRequests_DepartmentId_TargetType_TargetId_Stat~",
                table: "DeleteArchiveRequests",
                columns: new[] { "DepartmentId", "TargetType", "TargetId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_DeleteArchiveRequests_RequesterId",
                table: "DeleteArchiveRequests",
                column: "RequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentArchiveLeaders_DepartmentId_UserId",
                table: "DepartmentArchiveLeaders",
                columns: new[] { "DepartmentId", "UserId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentArchiveLeaders_UserId",
                table: "DepartmentArchiveLeaders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_DepartmentHeadId",
                table: "Departments",
                column: "DepartmentHeadId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_ParentDepartmentId",
                table: "Departments",
                column: "ParentDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentTemplateNumbers_DepartmentId_TemplateId",
                table: "DepartmentTemplateNumbers",
                columns: new[] { "DepartmentId", "TemplateId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentTemplateNumbers_TemplateId",
                table: "DepartmentTemplateNumbers",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentChunks_DocumentId",
                table: "DocumentChunks",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ArchiveRecordId",
                table: "Documents",
                column: "ArchiveRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_CreatedAt",
                table: "Documents",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_FileType",
                table: "Documents",
                column: "FileType");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_PhysicalFileId",
                table: "Documents",
                column: "PhysicalFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_SourceType",
                table: "Documents",
                column: "SourceType");

            migrationBuilder.CreateIndex(
                name: "IX_EditArchiveRequests_ApproverId",
                table: "EditArchiveRequests",
                column: "ApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_EditArchiveRequests_ArchiveRecordId",
                table: "EditArchiveRequests",
                column: "ArchiveRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_EditArchiveRequests_DepartmentId_ArchiveRecordId_Status",
                table: "EditArchiveRequests",
                columns: new[] { "DepartmentId", "ArchiveRecordId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_EditArchiveRequests_RequesterId",
                table: "EditArchiveRequests",
                column: "RequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_FolderPermissions_FolderId",
                table: "FolderPermissions",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_DepartmentId",
                table: "Folders",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_IconId",
                table: "Folders",
                column: "IconId");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_ParentId",
                table: "Folders",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_InputValue_Lookup",
                table: "InputValue",
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

            migrationBuilder.CreateIndex(
                name: "IX_PhysicalFiles_ArchiveRecordId_CreatedAt",
                table: "PhysicalFiles",
                columns: new[] { "ArchiveRecordId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PhysicalFiles_ArchiveRecordId_IsDeleted_FileExtension_Covering",
                table: "PhysicalFiles",
                columns: new[] { "ArchiveRecordId", "IsDeleted", "FileExtension" })
                .Annotation("Npgsql:IndexInclude", new[] { "FileSize", "ContentType", "FileName", "CreatedAt", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PhysicalFiles_EditArchiveRequestId",
                table: "PhysicalFiles",
                column: "EditArchiveRequestId");

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
                name: "IX_Requests_ApproverId",
                table: "Requests",
                column: "ApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_CurrentTransactionId",
                table: "Requests",
                column: "CurrentTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_FirstTransactionId",
                table: "Requests",
                column: "FirstTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_RequesterId",
                table: "Requests",
                column: "RequesterId");

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

            migrationBuilder.CreateIndex(
                name: "IX_RequestUser_ReadOnlyUsersId",
                table: "RequestUser",
                column: "ReadOnlyUsersId");

            migrationBuilder.CreateIndex(
                name: "IX_ResponseAttachments_AttachmentId",
                table: "ResponseAttachments",
                column: "AttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ResponseAttachments_ResponseId",
                table: "ResponseAttachments",
                column: "ResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RolesId",
                table: "RolePermissions",
                column: "RolesId");

            migrationBuilder.CreateIndex(
                name: "IX_SubSystemUsers_UserId",
                table: "SubSystemUsers",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TemplateOwnerships_DepartmentId",
                table: "TemplateOwnerships",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateOwnerships_TemplateId",
                table: "TemplateOwnerships",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_DefaultReceiverDepartmentId",
                table: "Templates",
                column: "DefaultReceiverDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UsersId",
                table: "UserRoles",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DepartmentId",
                table: "Users",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTemplateOwnerships_TemplateId",
                table: "UserTemplateOwnerships",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTemplateOwnerships_UserId",
                table: "UserTemplateOwnerships",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserVisitedTemplates_VisitedTemplatesId",
                table: "UserVisitedTemplates",
                column: "VisitedTemplatesId");

            migrationBuilder.AddForeignKey(
                name: "FK_ArchiveAuditLogs_ArchiveRecords_ArchiveRecordId",
                table: "ArchiveAuditLogs",
                column: "ArchiveRecordId",
                principalTable: "ArchiveRecords",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ArchiveRecordFormInputValues_ArchiveRecordTemplateValues_Ar~",
                table: "ArchiveRecordFormInputValues",
                column: "ArchiveRecordTemplateValuesId",
                principalTable: "ArchiveRecordTemplateValues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ArchiveRecords_Departments_DepartmentId",
                table: "ArchiveRecords",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ArchiveRecords_Folders_FolderId",
                table: "ArchiveRecords",
                column: "FolderId",
                principalTable: "Folders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeleteArchiveRequests_Departments_DepartmentId",
                table: "DeleteArchiveRequests",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeleteArchiveRequests_Users_ApproverId",
                table: "DeleteArchiveRequests",
                column: "ApproverId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeleteArchiveRequests_Users_RequesterId",
                table: "DeleteArchiveRequests",
                column: "RequesterId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DepartmentArchiveLeaders_Departments_DepartmentId",
                table: "DepartmentArchiveLeaders",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DepartmentArchiveLeaders_Users_UserId",
                table: "DepartmentArchiveLeaders",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Users_DepartmentHeadId",
                table: "Departments",
                column: "DepartmentHeadId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InputValue_RequestTemplateValues_RequestTemplateValuesId",
                table: "InputValue",
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
                name: "FK_Templates_Departments_DefaultReceiverDepartmentId",
                table: "Templates");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Departments_DepartmentId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Users_ApproverId",
                table: "Requests");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Users_RequesterId",
                table: "Requests");

            migrationBuilder.DropForeignKey(
                name: "FK_RequestTransactions_Users_CurrentUserHolderId",
                table: "RequestTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Templates_TemplateId",
                table: "Requests");

            migrationBuilder.DropForeignKey(
                name: "FK_RequestTransactions_Requests_RequestId",
                table: "RequestTransactions");

            migrationBuilder.DropTable(
                name: "ArchiveAuditLogs");

            migrationBuilder.DropTable(
                name: "ArchiveConfigs");

            migrationBuilder.DropTable(
                name: "ArchiveRecordFormInputValues");

            migrationBuilder.DropTable(
                name: "DeleteArchiveRequests");

            migrationBuilder.DropTable(
                name: "DepartmentArchiveLeaders");

            migrationBuilder.DropTable(
                name: "DepartmentTemplateNumbers");

            migrationBuilder.DropTable(
                name: "DocumentChunks");

            migrationBuilder.DropTable(
                name: "FolderPermissions");

            migrationBuilder.DropTable(
                name: "InputValue");

            migrationBuilder.DropTable(
                name: "LookUpFiledValues");

            migrationBuilder.DropTable(
                name: "Operations");

            migrationBuilder.DropTable(
                name: "RequestAttachments");

            migrationBuilder.DropTable(
                name: "RequestAuditLogs");

            migrationBuilder.DropTable(
                name: "RequestRelations");

            migrationBuilder.DropTable(
                name: "RequestTransactionAttachments");

            migrationBuilder.DropTable(
                name: "RequestUser");

            migrationBuilder.DropTable(
                name: "ResponseAttachments");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "SubSystemUsers");

            migrationBuilder.DropTable(
                name: "TemplateOwnerships");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "UserTemplateOwnerships");

            migrationBuilder.DropTable(
                name: "UserVisitedTemplates");

            migrationBuilder.DropTable(
                name: "ArchiveRecordTemplateValues");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "RequestTemplateValues");

            migrationBuilder.DropTable(
                name: "LookUpFields");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "KindShips");

            migrationBuilder.DropTable(
                name: "OperationServiceTypes");

            migrationBuilder.DropTable(
                name: "OperationStatuses");

            migrationBuilder.DropTable(
                name: "Attachments");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "PhysicalFiles");

            migrationBuilder.DropTable(
                name: "Genders");

            migrationBuilder.DropTable(
                name: "Govs");

            migrationBuilder.DropTable(
                name: "Nationals");

            migrationBuilder.DropTable(
                name: "EditArchiveRequests");

            migrationBuilder.DropTable(
                name: "ArchiveRecords");

            migrationBuilder.DropTable(
                name: "DynamicForms");

            migrationBuilder.DropTable(
                name: "Folders");

            migrationBuilder.DropTable(
                name: "FolderIcons");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "Users");

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
