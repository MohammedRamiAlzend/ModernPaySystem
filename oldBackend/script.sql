CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260316092023_InitialCreate') THEN
    CREATE TABLE "Attachments" (
        "Id" uuid NOT NULL,
        "FileName" text NOT NULL,
        "SafeName" text NOT NULL,
        "Extension" text NOT NULL,
        "Path" text NOT NULL,
        "CreatedByUserId" text,
        "CreatedAt" timestamp with time zone,
        "UpdatedByUserId" text,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_Attachments" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260316092023_InitialCreate') THEN
    CREATE TABLE "Permissions" (
        "Id" uuid NOT NULL,
        "Key" text NOT NULL,
        "Name" text,
        "Description" text,
        "Type" integer NOT NULL,
        "SubSystem" integer NOT NULL,
        CONSTRAINT "PK_Permissions" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260316092023_InitialCreate') THEN
    CREATE TABLE "Responses" (
        "Id" uuid NOT NULL,
        "RequestId" uuid NOT NULL,
        "RespondedByUserId" uuid NOT NULL,
        "Comment" text,
        "CreatedByUserId" text,
        "CreatedAt" timestamp with time zone,
        "UpdatedByUserId" text,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_Responses" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260316092023_InitialCreate') THEN
    CREATE TABLE "Roles" (
        "Id" uuid NOT NULL,
        "Name" text NOT NULL,
        "Description" text,
        CONSTRAINT "PK_Roles" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260316092023_InitialCreate') THEN
    CREATE TABLE "Templates" (
        "Id" uuid NOT NULL,
        "ContentAsJson" text NOT NULL,
        "TemplateName" text NOT NULL,
        "TemplateDescription" text,
        "CreatedByUserId" text,
        "CreatedAt" timestamp with time zone,
        "UpdatedByUserId" text,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_Templates" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260316092023_InitialCreate') THEN
    CREATE TABLE "Users" (
        "Id" uuid NOT NULL,
        "UserName" text NOT NULL,
        "HashedPassword" text NOT NULL,
        "SubSystemUserId" uuid,
        "CreatedByUserId" text,
        "CreatedAt" timestamp with time zone,
        "UpdatedByUserId" text,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260316092023_InitialCreate') THEN
    CREATE TABLE "ResponseAttachments" (
        "Id" uuid NOT NULL,
        "ResponseId" uuid NOT NULL,
        "AttachmentId" uuid NOT NULL,
        "CreatedByUserId" text,
        "CreatedAt" timestamp with time zone,
        "UpdatedByUserId" text,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_ResponseAttachments" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ResponseAttachments_Attachments_AttachmentId" FOREIGN KEY ("AttachmentId") REFERENCES "Attachments" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_ResponseAttachments_Responses_ResponseId" FOREIGN KEY ("ResponseId") REFERENCES "Responses" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260316092023_InitialCreate') THEN
    CREATE TABLE "RolePermissions" (
        "PermissionsId" uuid NOT NULL,
        "RolesId" uuid NOT NULL,
        CONSTRAINT "PK_RolePermissions" PRIMARY KEY ("PermissionsId", "RolesId"),
        CONSTRAINT "FK_RolePermissions_Permissions_PermissionsId" FOREIGN KEY ("PermissionsId") REFERENCES "Permissions" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_RolePermissions_Roles_RolesId" FOREIGN KEY ("RolesId") REFERENCES "Roles" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260316092023_InitialCreate') THEN
    CREATE TABLE "LookUpFields" (
        "Id" uuid NOT NULL,
        "FiledName" text NOT NULL,
        "TemplateId" uuid,
        CONSTRAINT "PK_LookUpFields" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_LookUpFields_Templates_TemplateId" FOREIGN KEY ("TemplateId") REFERENCES "Templates" ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260316092023_InitialCreate') THEN
    CREATE TABLE "Requests" (
        "Id" uuid NOT NULL,
        "TemplateId" uuid NOT NULL,
        "RequesterId" uuid NOT NULL,
        "ApproverId" uuid NOT NULL,
        "ResponseId" uuid,
        "ContentAsJson" text NOT NULL,
        "CreatedByUserId" text,
        "CreatedAt" timestamp with time zone,
        "UpdatedByUserId" text,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_Requests" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Requests_Responses_ResponseId" FOREIGN KEY ("ResponseId") REFERENCES "Responses" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_Requests_Templates_TemplateId" FOREIGN KEY ("TemplateId") REFERENCES "Templates" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Requests_Users_ApproverId" FOREIGN KEY ("ApproverId") REFERENCES "Users" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Requests_Users_RequesterId" FOREIGN KEY ("RequesterId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260316092023_InitialCreate') THEN
    CREATE TABLE "SubSystemUsers" (
        "Id" uuid NOT NULL,
        "SubSystem" integer,
        "UserId" uuid NOT NULL,
        CONSTRAINT "PK_SubSystemUsers" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_SubSystemUsers_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260316092023_InitialCreate') THEN
    CREATE TABLE "TemplateOwnerships" (
        "Id" uuid NOT NULL,
        "TemplateId" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        CONSTRAINT "PK_TemplateOwnerships" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TemplateOwnerships_Templates_TemplateId" FOREIGN KEY ("TemplateId") REFERENCES "Templates" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_TemplateOwnerships_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260316092023_InitialCreate') THEN
    CREATE TABLE "UserRoles" (
        "RolesId" uuid NOT NULL,
        "UsersId" uuid NOT NULL,
        CONSTRAINT "PK_UserRoles" PRIMARY KEY ("RolesId", "UsersId"),
        CONSTRAINT "FK_UserRoles_Roles_RolesId" FOREIGN KEY ("RolesId") REFERENCES "Roles" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_UserRoles_Users_UsersId" FOREIGN KEY ("UsersId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260316092023_InitialCreate') THEN
    CREATE TABLE "LookUpFiledValues" (
        "Id" uuid NOT NULL,
        "LookUpFiledId" uuid NOT NULL,
        "Desc" text NOT NULL,
        CONSTRAINT "PK_LookUpFiledValues" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_LookUpFiledValues_LookUpFields_LookUpFiledId" FOREIGN KEY ("LookUpFiledId") REFERENCES "LookUpFields" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260316092023_InitialCreate') THEN
    CREATE TABLE "RequestAttachments" (
        "RequestId" uuid NOT NULL,
        "AttachmentId" uuid NOT NULL,
        "CreatedByUserId" text,
        "CreatedAt" timestamp with time zone,
        "UpdatedByUserId" text,
        "UpdatedAt" timestamp with time zone,
        "Id" uuid NOT NULL,
        CONSTRAINT "PK_RequestAttachments" PRIMARY KEY ("RequestId", "AttachmentId"),
        CONSTRAINT "FK_RequestAttachments_Attachments_AttachmentId" FOREIGN KEY ("AttachmentId") REFERENCES "Attachments" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_RequestAttachments_Requests_RequestId" FOREIGN KEY ("RequestId") REFERENCES "Requests" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260316092023_InitialCreate') THEN
    CREATE INDEX "IX_LookUpFields_TemplateId" ON "LookUpFields" ("TemplateId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260316092023_InitialCreate') THEN
    CREATE INDEX "IX_LookUpFiledValues_LookUpFiledId" ON "LookUpFiledValues" ("LookUpFiledId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260316092023_InitialCreate') THEN
    CREATE INDEX "IX_RequestAttachments_AttachmentId" ON "RequestAttachments" ("AttachmentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260316092023_InitialCreate') THEN
    CREATE INDEX "IX_Requests_ApproverId" ON "Requests" ("ApproverId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260316092023_InitialCreate') THEN
    CREATE INDEX "IX_Requests_RequesterId" ON "Requests" ("RequesterId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260316092023_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Requests_ResponseId" ON "Requests" ("ResponseId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260316092023_InitialCreate') THEN
    CREATE INDEX "IX_Requests_TemplateId" ON "Requests" ("TemplateId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260316092023_InitialCreate') THEN
    CREATE INDEX "IX_ResponseAttachments_AttachmentId" ON "ResponseAttachments" ("AttachmentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260316092023_InitialCreate') THEN
    CREATE INDEX "IX_ResponseAttachments_ResponseId" ON "ResponseAttachments" ("ResponseId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260316092023_InitialCreate') THEN
    CREATE INDEX "IX_RolePermissions_RolesId" ON "RolePermissions" ("RolesId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260316092023_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_SubSystemUsers_UserId" ON "SubSystemUsers" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260316092023_InitialCreate') THEN
    CREATE INDEX "IX_TemplateOwnerships_TemplateId" ON "TemplateOwnerships" ("TemplateId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260316092023_InitialCreate') THEN
    CREATE INDEX "IX_TemplateOwnerships_UserId" ON "TemplateOwnerships" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260316092023_InitialCreate') THEN
    CREATE INDEX "IX_UserRoles_UsersId" ON "UserRoles" ("UsersId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260316092023_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260316092023_InitialCreate', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331143310_AddPaySystemFastOperationsEntities') THEN
    CREATE TABLE "Genders" (
        "Id" uuid NOT NULL,
        "Desc" text NOT NULL,
        CONSTRAINT "PK_Genders" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331143310_AddPaySystemFastOperationsEntities') THEN
    CREATE TABLE "Govs" (
        "Id" uuid NOT NULL,
        "Desc" text NOT NULL,
        CONSTRAINT "PK_Govs" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331143310_AddPaySystemFastOperationsEntities') THEN
    CREATE TABLE "KindShips" (
        "Id" uuid NOT NULL,
        "Desc" text NOT NULL,
        CONSTRAINT "PK_KindShips" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331143310_AddPaySystemFastOperationsEntities') THEN
    CREATE TABLE "Nationals" (
        "Id" uuid NOT NULL,
        "Desc" text NOT NULL,
        CONSTRAINT "PK_Nationals" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331143310_AddPaySystemFastOperationsEntities') THEN
    CREATE TABLE "OperationServiceTypes" (
        "Id" uuid NOT NULL,
        "Desc" text NOT NULL,
        CONSTRAINT "PK_OperationServiceTypes" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331143310_AddPaySystemFastOperationsEntities') THEN
    CREATE TABLE "OperationStatuses" (
        "Id" uuid NOT NULL,
        "Desc" text NOT NULL,
        CONSTRAINT "PK_OperationStatuses" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331143310_AddPaySystemFastOperationsEntities') THEN
    CREATE TABLE "Clients" (
        "Id" uuid NOT NULL,
        "FirstName" text NOT NULL,
        "FatherName" text NOT NULL,
        "MotherName" text NOT NULL,
        "LastName" text NOT NULL,
        "PlaceBirth" text NOT NULL,
        "DateBirth" timestamp with time zone NOT NULL,
        "PhoneNumber" text NOT NULL,
        "NationalityNumber" text NOT NULL,
        "GenderId" uuid NOT NULL,
        "NationalId" uuid NOT NULL,
        "GovId" uuid NOT NULL,
        "CreatedByUserId" text,
        "CreatedAt" timestamp with time zone,
        "UpdatedByUserId" text,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_Clients" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Clients_Genders_GenderId" FOREIGN KEY ("GenderId") REFERENCES "Genders" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Clients_Govs_GovId" FOREIGN KEY ("GovId") REFERENCES "Govs" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Clients_Nationals_NationalId" FOREIGN KEY ("NationalId") REFERENCES "Nationals" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331143310_AddPaySystemFastOperationsEntities') THEN
    CREATE TABLE "Operations" (
        "Id" uuid NOT NULL,
        "ApplicantClientId" uuid NOT NULL,
        "RecipientClientId" uuid NOT NULL,
        "KindShipId" uuid NOT NULL,
        "OperationStatusId" uuid NOT NULL,
        "OperationServiceTypeId" uuid NOT NULL,
        "SumAmount" numeric NOT NULL,
        "Notes" text NOT NULL,
        "CreatedByUserId" text,
        "CreatedAt" timestamp with time zone,
        "UpdatedByUserId" text,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_Operations" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Operations_Clients_ApplicantClientId" FOREIGN KEY ("ApplicantClientId") REFERENCES "Clients" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Operations_Clients_RecipientClientId" FOREIGN KEY ("RecipientClientId") REFERENCES "Clients" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Operations_KindShips_KindShipId" FOREIGN KEY ("KindShipId") REFERENCES "KindShips" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Operations_OperationServiceTypes_OperationServiceTypeId" FOREIGN KEY ("OperationServiceTypeId") REFERENCES "OperationServiceTypes" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Operations_OperationStatuses_OperationStatusId" FOREIGN KEY ("OperationStatusId") REFERENCES "OperationStatuses" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331143310_AddPaySystemFastOperationsEntities') THEN
    CREATE INDEX "IX_Clients_GenderId" ON "Clients" ("GenderId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331143310_AddPaySystemFastOperationsEntities') THEN
    CREATE INDEX "IX_Clients_GovId" ON "Clients" ("GovId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331143310_AddPaySystemFastOperationsEntities') THEN
    CREATE INDEX "IX_Clients_NationalId" ON "Clients" ("NationalId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331143310_AddPaySystemFastOperationsEntities') THEN
    CREATE INDEX "IX_Operations_ApplicantClientId" ON "Operations" ("ApplicantClientId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331143310_AddPaySystemFastOperationsEntities') THEN
    CREATE INDEX "IX_Operations_KindShipId" ON "Operations" ("KindShipId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331143310_AddPaySystemFastOperationsEntities') THEN
    CREATE INDEX "IX_Operations_OperationServiceTypeId" ON "Operations" ("OperationServiceTypeId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331143310_AddPaySystemFastOperationsEntities') THEN
    CREATE INDEX "IX_Operations_OperationStatusId" ON "Operations" ("OperationStatusId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331143310_AddPaySystemFastOperationsEntities') THEN
    CREATE INDEX "IX_Operations_RecipientClientId" ON "Operations" ("RecipientClientId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331143310_AddPaySystemFastOperationsEntities') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260331143310_AddPaySystemFastOperationsEntities', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260405092203_AddMentionedUsersToRequest') THEN
    CREATE TABLE "RequestUser" (
        "MentionedRequestsId" uuid NOT NULL,
        "ReadOnlyUsersId" uuid NOT NULL,
        CONSTRAINT "PK_RequestUser" PRIMARY KEY ("MentionedRequestsId", "ReadOnlyUsersId"),
        CONSTRAINT "FK_RequestUser_Requests_MentionedRequestsId" FOREIGN KEY ("MentionedRequestsId") REFERENCES "Requests" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_RequestUser_Users_ReadOnlyUsersId" FOREIGN KEY ("ReadOnlyUsersId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260405092203_AddMentionedUsersToRequest') THEN
    CREATE INDEX "IX_RequestUser_ReadOnlyUsersId" ON "RequestUser" ("ReadOnlyUsersId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260405092203_AddMentionedUsersToRequest') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260405092203_AddMentionedUsersToRequest', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260411120851_AddResponseTransactionTable') THEN
    CREATE TABLE "ResponseTransactions" (
        "Id" uuid NOT NULL,
        "ResponseId" uuid NOT NULL,
        "Notes" text NOT NULL,
        "Level" integer NOT NULL,
        "Path" text NOT NULL,
        "ParentTransactionId" uuid,
        "CurrentUserHolderId" uuid NOT NULL,
        "CreatedByUserId" text,
        "CreatedAt" timestamp with time zone,
        "UpdatedByUserId" text,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_ResponseTransactions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ResponseTransactions_ResponseTransactions_ParentTransaction~" FOREIGN KEY ("ParentTransactionId") REFERENCES "ResponseTransactions" ("Id"),
        CONSTRAINT "FK_ResponseTransactions_Responses_ResponseId" FOREIGN KEY ("ResponseId") REFERENCES "Responses" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_ResponseTransactions_Users_CurrentUserHolderId" FOREIGN KEY ("CurrentUserHolderId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260411120851_AddResponseTransactionTable') THEN
    CREATE TABLE "ResponseTransactionAttachments" (
        "Id" uuid NOT NULL,
        "ResponseTransactionId" uuid NOT NULL,
        "AttachmentId" uuid NOT NULL,
        "CreatedByUserId" text,
        "CreatedAt" timestamp with time zone,
        "UpdatedByUserId" text,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_ResponseTransactionAttachments" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ResponseTransactionAttachments_Attachments_AttachmentId" FOREIGN KEY ("AttachmentId") REFERENCES "Attachments" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_ResponseTransactionAttachments_ResponseTransactions_Respons~" FOREIGN KEY ("ResponseTransactionId") REFERENCES "ResponseTransactions" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260411120851_AddResponseTransactionTable') THEN
    CREATE INDEX "IX_ResponseTransactionAttachments_AttachmentId" ON "ResponseTransactionAttachments" ("AttachmentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260411120851_AddResponseTransactionTable') THEN
    CREATE INDEX "IX_ResponseTransactionAttachments_ResponseTransactionId" ON "ResponseTransactionAttachments" ("ResponseTransactionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260411120851_AddResponseTransactionTable') THEN
    CREATE INDEX "IX_ResponseTransactions_CurrentUserHolderId" ON "ResponseTransactions" ("CurrentUserHolderId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260411120851_AddResponseTransactionTable') THEN
    CREATE INDEX "IX_ResponseTransactions_ParentTransactionId" ON "ResponseTransactions" ("ParentTransactionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260411120851_AddResponseTransactionTable') THEN
    CREATE INDEX "IX_ResponseTransactions_ResponseId" ON "ResponseTransactions" ("ResponseId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260411120851_AddResponseTransactionTable') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260411120851_AddResponseTransactionTable', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260412080256_AddResponseTransactionTable2') THEN
    ALTER TABLE "ResponseTransactions" DROP CONSTRAINT "FK_ResponseTransactions_ResponseTransactions_ParentTransaction~";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260412080256_AddResponseTransactionTable2') THEN
    ALTER TABLE "ResponseTransactions" DROP CONSTRAINT "FK_ResponseTransactions_Users_CurrentUserHolderId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260412080256_AddResponseTransactionTable2') THEN
    ALTER TABLE "ResponseTransactions" ADD CONSTRAINT "FK_ResponseTransactions_ResponseTransactions_ParentTransaction~" FOREIGN KEY ("ParentTransactionId") REFERENCES "ResponseTransactions" ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260412080256_AddResponseTransactionTable2') THEN
    ALTER TABLE "ResponseTransactions" ADD CONSTRAINT "FK_ResponseTransactions_Users_CurrentUserHolderId" FOREIGN KEY ("CurrentUserHolderId") REFERENCES "Users" ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260412080256_AddResponseTransactionTable2') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260412080256_AddResponseTransactionTable2', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260412110435_MoveStatusToRequestAndUpdateRelationships') THEN
    DROP TABLE "ResponseTransactionAttachments";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260412110435_MoveStatusToRequestAndUpdateRelationships') THEN
    DROP TABLE "ResponseTransactions";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260412110435_MoveStatusToRequestAndUpdateRelationships') THEN
    ALTER TABLE "Requests" ADD "CurrentTransactionId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260412110435_MoveStatusToRequestAndUpdateRelationships') THEN
    ALTER TABLE "Requests" ADD "FirstTransactionId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260412110435_MoveStatusToRequestAndUpdateRelationships') THEN
    ALTER TABLE "Requests" ADD "Status" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260412110435_MoveStatusToRequestAndUpdateRelationships') THEN
    CREATE TABLE "RequestTransactions" (
        "Id" uuid NOT NULL,
        "RequestId" uuid NOT NULL,
        "Status" integer NOT NULL,
        "Notes" text NOT NULL,
        "Level" integer NOT NULL,
        "Path" text NOT NULL,
        "ParentTransactionId" uuid,
        "CurrentUserHolderId" uuid NOT NULL,
        "CreatedByUserId" text,
        "CreatedAt" timestamp with time zone,
        "UpdatedByUserId" text,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_RequestTransactions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RequestTransactions_RequestTransactions_ParentTransactionId" FOREIGN KEY ("ParentTransactionId") REFERENCES "RequestTransactions" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_RequestTransactions_Requests_RequestId" FOREIGN KEY ("RequestId") REFERENCES "Requests" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_RequestTransactions_Users_CurrentUserHolderId" FOREIGN KEY ("CurrentUserHolderId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260412110435_MoveStatusToRequestAndUpdateRelationships') THEN
    CREATE TABLE "RequestTransactionAttachments" (
        "Id" uuid NOT NULL,
        "RequestTransactionId" uuid NOT NULL,
        "AttachmentId" uuid NOT NULL,
        "CreatedByUserId" text,
        "CreatedAt" timestamp with time zone,
        "UpdatedByUserId" text,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_RequestTransactionAttachments" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RequestTransactionAttachments_Attachments_AttachmentId" FOREIGN KEY ("AttachmentId") REFERENCES "Attachments" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_RequestTransactionAttachments_RequestTransactions_RequestTr~" FOREIGN KEY ("RequestTransactionId") REFERENCES "RequestTransactions" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260412110435_MoveStatusToRequestAndUpdateRelationships') THEN
    CREATE INDEX "IX_Requests_CurrentTransactionId" ON "Requests" ("CurrentTransactionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260412110435_MoveStatusToRequestAndUpdateRelationships') THEN
    CREATE INDEX "IX_Requests_FirstTransactionId" ON "Requests" ("FirstTransactionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260412110435_MoveStatusToRequestAndUpdateRelationships') THEN
    CREATE INDEX "IX_RequestTransactionAttachments_AttachmentId" ON "RequestTransactionAttachments" ("AttachmentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260412110435_MoveStatusToRequestAndUpdateRelationships') THEN
    CREATE INDEX "IX_RequestTransactionAttachments_RequestTransactionId" ON "RequestTransactionAttachments" ("RequestTransactionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260412110435_MoveStatusToRequestAndUpdateRelationships') THEN
    CREATE INDEX "IX_RequestTransactions_CurrentUserHolderId" ON "RequestTransactions" ("CurrentUserHolderId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260412110435_MoveStatusToRequestAndUpdateRelationships') THEN
    CREATE INDEX "IX_RequestTransactions_ParentTransactionId" ON "RequestTransactions" ("ParentTransactionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260412110435_MoveStatusToRequestAndUpdateRelationships') THEN
    CREATE INDEX "IX_RequestTransactions_RequestId" ON "RequestTransactions" ("RequestId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260412110435_MoveStatusToRequestAndUpdateRelationships') THEN
    ALTER TABLE "Requests" ADD CONSTRAINT "FK_Requests_RequestTransactions_CurrentTransactionId" FOREIGN KEY ("CurrentTransactionId") REFERENCES "RequestTransactions" ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260412110435_MoveStatusToRequestAndUpdateRelationships') THEN
    ALTER TABLE "Requests" ADD CONSTRAINT "FK_Requests_RequestTransactions_FirstTransactionId" FOREIGN KEY ("FirstTransactionId") REFERENCES "RequestTransactions" ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260412110435_MoveStatusToRequestAndUpdateRelationships') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260412110435_MoveStatusToRequestAndUpdateRelationships', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260502135421_AddDepartmentsTable') THEN
    ALTER TABLE "Users" ADD "DepartmentId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260502135421_AddDepartmentsTable') THEN
    ALTER TABLE "Users" ADD "IsDepartmentHead" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260502135421_AddDepartmentsTable') THEN
    CREATE TABLE "Departments" (
        "Id" uuid NOT NULL,
        "Name" text NOT NULL,
        "Code" text,
        "Description" text,
        "ParentDepartmentId" uuid,
        "Level" integer NOT NULL,
        "MaterializedPath" text,
        "Type" integer NOT NULL,
        "CreatedByUserId" text,
        "CreatedAt" timestamp with time zone,
        "UpdatedByUserId" text,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_Departments" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Departments_Departments_ParentDepartmentId" FOREIGN KEY ("ParentDepartmentId") REFERENCES "Departments" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260502135421_AddDepartmentsTable') THEN
    CREATE INDEX "IX_Users_DepartmentId" ON "Users" ("DepartmentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260502135421_AddDepartmentsTable') THEN
    CREATE INDEX "IX_Departments_ParentDepartmentId" ON "Departments" ("ParentDepartmentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260502135421_AddDepartmentsTable') THEN
    ALTER TABLE "Users" ADD CONSTRAINT "FK_Users_Departments_DepartmentId" FOREIGN KEY ("DepartmentId") REFERENCES "Departments" ("Id") ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260502135421_AddDepartmentsTable') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260502135421_AddDepartmentsTable', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260503114500_UpdateTemplateOwnerShipFromUserToDeparmnet') THEN
    ALTER TABLE "TemplateOwnerships" DROP CONSTRAINT "FK_TemplateOwnerships_Users_UserId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260503114500_UpdateTemplateOwnerShipFromUserToDeparmnet') THEN
    ALTER TABLE "TemplateOwnerships" RENAME COLUMN "UserId" TO "DepartmentId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260503114500_UpdateTemplateOwnerShipFromUserToDeparmnet') THEN
    ALTER INDEX "IX_TemplateOwnerships_UserId" RENAME TO "IX_TemplateOwnerships_DepartmentId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260503114500_UpdateTemplateOwnerShipFromUserToDeparmnet') THEN
    ALTER TABLE "TemplateOwnerships" ADD CONSTRAINT "FK_TemplateOwnerships_Departments_DepartmentId" FOREIGN KEY ("DepartmentId") REFERENCES "Departments" ("Id") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260503114500_UpdateTemplateOwnerShipFromUserToDeparmnet') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260503114500_UpdateTemplateOwnerShipFromUserToDeparmnet', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260503133600_AddUserTemplateOwnership') THEN
    CREATE TABLE "UserTemplateOwnerships" (
        "Id" uuid NOT NULL,
        "TemplateId" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        CONSTRAINT "PK_UserTemplateOwnerships" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_UserTemplateOwnerships_Templates_TemplateId" FOREIGN KEY ("TemplateId") REFERENCES "Templates" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_UserTemplateOwnerships_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260503133600_AddUserTemplateOwnership') THEN
    CREATE INDEX "IX_UserTemplateOwnerships_TemplateId" ON "UserTemplateOwnerships" ("TemplateId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260503133600_AddUserTemplateOwnership') THEN
    CREATE INDEX "IX_UserTemplateOwnerships_UserId" ON "UserTemplateOwnerships" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260503133600_AddUserTemplateOwnership') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260503133600_AddUserTemplateOwnership', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260504062947_UpdateTemplateOwnerShipFromUserToDeparmnet4') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260504062947_UpdateTemplateOwnerShipFromUserToDeparmnet4', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260510095832_addDepartmentHeadFunctionality') THEN
    ALTER TABLE "Users" ADD "HeadedDepartmentId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260510095832_addDepartmentHeadFunctionality') THEN
    ALTER TABLE "Departments" ADD "DepartmentHeadId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260510095832_addDepartmentHeadFunctionality') THEN
    CREATE UNIQUE INDEX "IX_Departments_DepartmentHeadId" ON "Departments" ("DepartmentHeadId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260510095832_addDepartmentHeadFunctionality') THEN
    ALTER TABLE "Departments" ADD CONSTRAINT "FK_Departments_Users_DepartmentHeadId" FOREIGN KEY ("DepartmentHeadId") REFERENCES "Users" ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260510095832_addDepartmentHeadFunctionality') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260510095832_addDepartmentHeadFunctionality', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260510111716_addDefaultReceiverDepartmentIdAndIsRequireAttachments') THEN
    ALTER TABLE "Templates" ADD "DefaultReceiverDepartmentId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260510111716_addDefaultReceiverDepartmentIdAndIsRequireAttachments') THEN
    ALTER TABLE "Templates" ADD "IsRequireAttachments" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260510111716_addDefaultReceiverDepartmentIdAndIsRequireAttachments') THEN
    CREATE INDEX "IX_Templates_DefaultReceiverDepartmentId" ON "Templates" ("DefaultReceiverDepartmentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260510111716_addDefaultReceiverDepartmentIdAndIsRequireAttachments') THEN
    ALTER TABLE "Templates" ADD CONSTRAINT "FK_Templates_Departments_DefaultReceiverDepartmentId" FOREIGN KEY ("DefaultReceiverDepartmentId") REFERENCES "Departments" ("Id") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260510111716_addDefaultReceiverDepartmentIdAndIsRequireAttachments') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260510111716_addDefaultReceiverDepartmentIdAndIsRequireAttachments', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260510125233_update') THEN
    ALTER TABLE "Departments" ALTER COLUMN "DepartmentHeadId" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260510125233_update') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260510125233_update', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260512121538_AddRequestTemplateValuesTable') THEN
    ALTER TABLE "Requests" DROP CONSTRAINT "FK_Requests_Templates_TemplateId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260512121538_AddRequestTemplateValuesTable') THEN
    ALTER TABLE "Requests" DROP COLUMN "ContentAsJson";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260512121538_AddRequestTemplateValuesTable') THEN
    ALTER TABLE "Requests" ALTER COLUMN "TemplateId" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260512121538_AddRequestTemplateValuesTable') THEN
    ALTER TABLE "Requests" ADD "RequestTemplateValuesId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260512121538_AddRequestTemplateValuesTable') THEN
    CREATE TABLE "RequestTemplateValues" (
        "Id" uuid NOT NULL,
        "TemplateId" uuid NOT NULL,
        "RequestId" uuid NOT NULL,
        CONSTRAINT "PK_RequestTemplateValues" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RequestTemplateValues_Requests_RequestId" FOREIGN KEY ("RequestId") REFERENCES "Requests" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_RequestTemplateValues_Templates_TemplateId" FOREIGN KEY ("TemplateId") REFERENCES "Templates" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260512121538_AddRequestTemplateValuesTable') THEN
    CREATE TABLE "InputValue" (
        "Id" uuid NOT NULL,
        "RequestTemplateValuesId" uuid NOT NULL,
        "Key" text NOT NULL,
        "Value" text NOT NULL,
        CONSTRAINT "PK_InputValue" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_InputValue_RequestTemplateValues_RequestTemplateValuesId" FOREIGN KEY ("RequestTemplateValuesId") REFERENCES "RequestTemplateValues" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260512121538_AddRequestTemplateValuesTable') THEN
    CREATE INDEX "IX_InputValue_RequestTemplateValuesId" ON "InputValue" ("RequestTemplateValuesId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260512121538_AddRequestTemplateValuesTable') THEN
    CREATE UNIQUE INDEX "IX_RequestTemplateValues_RequestId" ON "RequestTemplateValues" ("RequestId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260512121538_AddRequestTemplateValuesTable') THEN
    CREATE INDEX "IX_RequestTemplateValues_TemplateId" ON "RequestTemplateValues" ("TemplateId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260512121538_AddRequestTemplateValuesTable') THEN
    ALTER TABLE "Requests" ADD CONSTRAINT "FK_Requests_Templates_TemplateId" FOREIGN KEY ("TemplateId") REFERENCES "Templates" ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260512121538_AddRequestTemplateValuesTable') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260512121538_AddRequestTemplateValuesTable', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260513104727_AddIndexesForInputValueLookupAndRequestTemplateValues') THEN
    DROP INDEX "IX_InputValue_RequestTemplateValuesId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260513104727_AddIndexesForInputValueLookupAndRequestTemplateValues') THEN
    CREATE INDEX "IX_InputValue_Lookup" ON "InputValue" ("RequestTemplateValuesId", "Key", "Value");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260513104727_AddIndexesForInputValueLookupAndRequestTemplateValues') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260513104727_AddIndexesForInputValueLookupAndRequestTemplateValues', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260513134718_AddVistedTemplatesRelation') THEN
    CREATE TABLE "UserVisitedTemplates" (
        "VisitedByUsersId" uuid NOT NULL,
        "VisitedTemplatesId" uuid NOT NULL,
        CONSTRAINT "PK_UserVisitedTemplates" PRIMARY KEY ("VisitedByUsersId", "VisitedTemplatesId"),
        CONSTRAINT "FK_UserVisitedTemplates_Templates_VisitedTemplatesId" FOREIGN KEY ("VisitedTemplatesId") REFERENCES "Templates" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_UserVisitedTemplates_Users_VisitedByUsersId" FOREIGN KEY ("VisitedByUsersId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260513134718_AddVistedTemplatesRelation') THEN
    CREATE INDEX "IX_UserVisitedTemplates_VisitedTemplatesId" ON "UserVisitedTemplates" ("VisitedTemplatesId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260513134718_AddVistedTemplatesRelation') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260513134718_AddVistedTemplatesRelation', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260516141739_AddRequesterAndApproverDepartmentIdAtRequest') THEN
    ALTER TABLE "Requests" ADD "ApproverDepartmentId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260516141739_AddRequesterAndApproverDepartmentIdAtRequest') THEN
    ALTER TABLE "Requests" ADD "RequesterDepartmentId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260516141739_AddRequesterAndApproverDepartmentIdAtRequest') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260516141739_AddRequesterAndApproverDepartmentIdAtRequest', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260518184323_newbymajid') THEN
    ALTER TABLE "InputValue" ALTER COLUMN "Value" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260518184323_newbymajid') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260518184323_newbymajid', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260520100639_AddNumberForRequest') THEN
    ALTER TABLE "Requests" ADD "RequestNumber" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260520100639_AddNumberForRequest') THEN
    CREATE TABLE "DepartmentTemplateNumbers" (
        "Id" uuid NOT NULL,
        "DepartmentId" uuid NOT NULL,
        "TemplateId" uuid NOT NULL,
        "LastRequestNumber" integer NOT NULL,
        CONSTRAINT "PK_DepartmentTemplateNumbers" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_DepartmentTemplateNumbers_Departments_DepartmentId" FOREIGN KEY ("DepartmentId") REFERENCES "Departments" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_DepartmentTemplateNumbers_Templates_TemplateId" FOREIGN KEY ("TemplateId") REFERENCES "Templates" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260520100639_AddNumberForRequest') THEN
    CREATE UNIQUE INDEX "IX_DepartmentTemplateNumbers_DepartmentId_TemplateId" ON "DepartmentTemplateNumbers" ("DepartmentId", "TemplateId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260520100639_AddNumberForRequest') THEN
    CREATE INDEX "IX_DepartmentTemplateNumbers_TemplateId" ON "DepartmentTemplateNumbers" ("TemplateId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260520100639_AddNumberForRequest') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260520100639_AddNumberForRequest', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260525073746_MoveArchivalNumberToArchiveRecord') THEN
    CREATE TABLE "DynamicForms" (
        "Id" uuid NOT NULL,
        "FormName" text NOT NULL,
        "ContentAsJson" jsonb NOT NULL,
        "FormDescription" text,
        "CreatedByUserId" text,
        "CreatedAt" timestamp with time zone,
        "UpdatedByUserId" text,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_DynamicForms" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260525073746_MoveArchivalNumberToArchiveRecord') THEN
    CREATE TABLE "Folders" (
        "Id" uuid NOT NULL,
        "Name" text NOT NULL,
        "Level" integer NOT NULL,
        "ParentId" uuid,
        "CreatedByUserId" text,
        "CreatedAt" timestamp with time zone,
        "UpdatedByUserId" text,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_Folders" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Folders_Folders_ParentId" FOREIGN KEY ("ParentId") REFERENCES "Folders" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260525073746_MoveArchivalNumberToArchiveRecord') THEN
    CREATE TABLE "ArchiveRecords" (
        "Id" uuid NOT NULL,
        "FolderId" uuid NOT NULL,
        "FormId" uuid NOT NULL,
        "ArchivalNumber" text NOT NULL,
        "ArchiveRecordTemplateValues" uuid NOT NULL,
        "MetadataValues" jsonb,
        "CreatedByUserId" text,
        "CreatedAt" timestamp with time zone,
        "UpdatedByUserId" text,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_ArchiveRecords" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ArchiveRecords_DynamicForms_FormId" FOREIGN KEY ("FormId") REFERENCES "DynamicForms" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_ArchiveRecords_Folders_FolderId" FOREIGN KEY ("FolderId") REFERENCES "Folders" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260525073746_MoveArchivalNumberToArchiveRecord') THEN
    CREATE TABLE "FolderPermissions" (
        "Id" uuid NOT NULL,
        "FolderId" uuid NOT NULL,
        "UserId" text NOT NULL,
        "AccessLevel" integer NOT NULL,
        "IsInherited" boolean NOT NULL,
        "CreatedByUserId" text,
        "CreatedAt" timestamp with time zone,
        "UpdatedByUserId" text,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_FolderPermissions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_FolderPermissions_Folders_FolderId" FOREIGN KEY ("FolderId") REFERENCES "Folders" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260525073746_MoveArchivalNumberToArchiveRecord') THEN
    CREATE TABLE "ArchiveRecordTemplateValues" (
        "Id" uuid NOT NULL,
        "ArchiveRecordId" uuid NOT NULL,
        "ArchiveRecordId1" uuid NOT NULL,
        "ArchiveFormTemplateId" uuid NOT NULL,
        "CreatedByUserId" text,
        "CreatedAt" timestamp with time zone,
        "UpdatedByUserId" text,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_ArchiveRecordTemplateValues" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ArchiveRecordTemplateValues_ArchiveRecords_ArchiveRecordId" FOREIGN KEY ("ArchiveRecordId") REFERENCES "ArchiveRecords" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_ArchiveRecordTemplateValues_ArchiveRecords_ArchiveRecordId1" FOREIGN KEY ("ArchiveRecordId1") REFERENCES "ArchiveRecords" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_ArchiveRecordTemplateValues_DynamicForms_ArchiveFormTemplat~" FOREIGN KEY ("ArchiveFormTemplateId") REFERENCES "DynamicForms" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260525073746_MoveArchivalNumberToArchiveRecord') THEN
    CREATE TABLE "PhysicalFiles" (
        "Id" uuid NOT NULL,
        "ArchiveRecordId" uuid NOT NULL,
        "FileName" text NOT NULL,
        "FileExtension" text NOT NULL,
        "StoragePath" text NOT NULL,
        "FileSize" bigint NOT NULL,
        "CreatedByUserId" text,
        "CreatedAt" timestamp with time zone,
        "UpdatedByUserId" text,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_PhysicalFiles" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_PhysicalFiles_ArchiveRecords_ArchiveRecordId" FOREIGN KEY ("ArchiveRecordId") REFERENCES "ArchiveRecords" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260525073746_MoveArchivalNumberToArchiveRecord') THEN
    CREATE TABLE "ArchiveRecordFormInputValues" (
        "Id" uuid NOT NULL,
        "Key" text NOT NULL,
        "Value" text,
        "ArchiveRecordTemplateValuesId" uuid,
        CONSTRAINT "PK_ArchiveRecordFormInputValues" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ArchiveRecordFormInputValues_ArchiveRecordTemplateValues_Ar~" FOREIGN KEY ("ArchiveRecordTemplateValuesId") REFERENCES "ArchiveRecordTemplateValues" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260525073746_MoveArchivalNumberToArchiveRecord') THEN
    CREATE INDEX "IX_ArchiveRecordFormInputValues_ArchiveRecordTemplateValuesId" ON "ArchiveRecordFormInputValues" ("ArchiveRecordTemplateValuesId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260525073746_MoveArchivalNumberToArchiveRecord') THEN
    CREATE INDEX "IX_ArchiveRecords_FolderId" ON "ArchiveRecords" ("FolderId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260525073746_MoveArchivalNumberToArchiveRecord') THEN
    CREATE INDEX "IX_ArchiveRecords_FormId" ON "ArchiveRecords" ("FormId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260525073746_MoveArchivalNumberToArchiveRecord') THEN
    CREATE INDEX "IX_ArchiveRecordTemplateValues_ArchiveFormTemplateId" ON "ArchiveRecordTemplateValues" ("ArchiveFormTemplateId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260525073746_MoveArchivalNumberToArchiveRecord') THEN
    CREATE UNIQUE INDEX "IX_ArchiveRecordTemplateValues_ArchiveRecordId" ON "ArchiveRecordTemplateValues" ("ArchiveRecordId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260525073746_MoveArchivalNumberToArchiveRecord') THEN
    CREATE INDEX "IX_ArchiveRecordTemplateValues_ArchiveRecordId1" ON "ArchiveRecordTemplateValues" ("ArchiveRecordId1");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260525073746_MoveArchivalNumberToArchiveRecord') THEN
    CREATE INDEX "IX_FolderPermissions_FolderId" ON "FolderPermissions" ("FolderId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260525073746_MoveArchivalNumberToArchiveRecord') THEN
    CREATE INDEX "IX_Folders_ParentId" ON "Folders" ("ParentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260525073746_MoveArchivalNumberToArchiveRecord') THEN
    CREATE INDEX "IX_PhysicalFiles_ArchiveRecordId" ON "PhysicalFiles" ("ArchiveRecordId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260525073746_MoveArchivalNumberToArchiveRecord') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260525073746_MoveArchivalNumberToArchiveRecord', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260525115909_RemoveMetadataValuesFromArchiveRecord') THEN
    ALTER TABLE "ArchiveRecords" DROP COLUMN "MetadataValues";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260525115909_RemoveMetadataValuesFromArchiveRecord') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260525115909_RemoveMetadataValuesFromArchiveRecord', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260525134549_AddPhysicalFileMetadataAndSoftDelete') THEN
    ALTER TABLE "PhysicalFiles" ADD "ContentType" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260525134549_AddPhysicalFileMetadataAndSoftDelete') THEN
    ALTER TABLE "PhysicalFiles" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260525134549_AddPhysicalFileMetadataAndSoftDelete') THEN
    ALTER TABLE "PhysicalFiles" ADD "IsDeleted" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260525134549_AddPhysicalFileMetadataAndSoftDelete') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260525134549_AddPhysicalFileMetadataAndSoftDelete', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260531111755_UpdateFormIdRelationToAcceptNullForArchiveRecored') THEN
    ALTER TABLE "ArchiveRecordTemplateValues" DROP CONSTRAINT "FK_ArchiveRecordTemplateValues_ArchiveRecords_ArchiveRecordId1";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260531111755_UpdateFormIdRelationToAcceptNullForArchiveRecored') THEN
    ALTER TABLE "ArchiveRecordTemplateValues" ALTER COLUMN "ArchiveRecordId1" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260531111755_UpdateFormIdRelationToAcceptNullForArchiveRecored') THEN
    ALTER TABLE "ArchiveRecords" ALTER COLUMN "FormId" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260531111755_UpdateFormIdRelationToAcceptNullForArchiveRecored') THEN
    ALTER TABLE "ArchiveRecords" ALTER COLUMN "ArchiveRecordTemplateValues" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260531111755_UpdateFormIdRelationToAcceptNullForArchiveRecored') THEN
    ALTER TABLE "ArchiveRecordTemplateValues" ADD CONSTRAINT "FK_ArchiveRecordTemplateValues_ArchiveRecords_ArchiveRecordId1" FOREIGN KEY ("ArchiveRecordId1") REFERENCES "ArchiveRecords" ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260531111755_UpdateFormIdRelationToAcceptNullForArchiveRecored') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260531111755_UpdateFormIdRelationToAcceptNullForArchiveRecored', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260531141637_AddPhysicalFileIndexesForPaginatedRetrieval') THEN
    DROP INDEX "IX_PhysicalFiles_ArchiveRecordId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260531141637_AddPhysicalFileIndexesForPaginatedRetrieval') THEN
    CREATE INDEX "IX_PhysicalFiles_ArchiveRecordId_CreatedAt" ON "PhysicalFiles" ("ArchiveRecordId", "CreatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260531141637_AddPhysicalFileIndexesForPaginatedRetrieval') THEN
    CREATE INDEX "IX_PhysicalFiles_ArchiveRecordId_IsDeleted_FileExtension_Covering" ON "PhysicalFiles" ("ArchiveRecordId", "IsDeleted", "FileExtension") INCLUDE ("FileSize", "ContentType", "FileName", "CreatedAt", "UpdatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260531141637_AddPhysicalFileIndexesForPaginatedRetrieval') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260531141637_AddPhysicalFileIndexesForPaginatedRetrieval', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601112107_AddRequestrelationsTable') THEN
    CREATE TABLE "RequestRelations" (
        "Id" uuid NOT NULL,
        "SourceRequestId" uuid NOT NULL,
        "TargetRequestId" uuid NOT NULL,
        "RelationType" integer NOT NULL,
        "Notes" text,
        "CreatedByUserId" text,
        "CreatedAt" timestamp with time zone,
        "UpdatedByUserId" text,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_RequestRelations" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RequestRelations_Requests_SourceRequestId" FOREIGN KEY ("SourceRequestId") REFERENCES "Requests" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_RequestRelations_Requests_TargetRequestId" FOREIGN KEY ("TargetRequestId") REFERENCES "Requests" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601112107_AddRequestrelationsTable') THEN
    CREATE UNIQUE INDEX "IX_RequestRelations_SourceRequestId_TargetRequestId_RelationTy~" ON "RequestRelations" ("SourceRequestId", "TargetRequestId", "RelationType");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601112107_AddRequestrelationsTable') THEN
    CREATE INDEX "IX_RequestRelations_TargetRequestId" ON "RequestRelations" ("TargetRequestId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601112107_AddRequestrelationsTable') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260601112107_AddRequestrelationsTable', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601144900_AddArchiveDeletionWorkflow') THEN
    ALTER TABLE "PhysicalFiles" ADD "DeletedByUserId" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601144900_AddArchiveDeletionWorkflow') THEN
    ALTER TABLE "Folders" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601144900_AddArchiveDeletionWorkflow') THEN
    ALTER TABLE "Folders" ADD "DeletedByRequestId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601144900_AddArchiveDeletionWorkflow') THEN
    ALTER TABLE "Folders" ADD "DeletedByUserId" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601144900_AddArchiveDeletionWorkflow') THEN
    ALTER TABLE "Folders" ADD "DepartmentId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601144900_AddArchiveDeletionWorkflow') THEN
    ALTER TABLE "Folders" ADD "IsDeleted" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601144900_AddArchiveDeletionWorkflow') THEN
    ALTER TABLE "ArchiveRecords" ADD "ApprovedByRequestId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601144900_AddArchiveDeletionWorkflow') THEN
    ALTER TABLE "ArchiveRecords" ADD "DeletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601144900_AddArchiveDeletionWorkflow') THEN
    ALTER TABLE "ArchiveRecords" ADD "DeletedByRequestId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601144900_AddArchiveDeletionWorkflow') THEN
    ALTER TABLE "ArchiveRecords" ADD "DeletedByUserId" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601144900_AddArchiveDeletionWorkflow') THEN
    ALTER TABLE "ArchiveRecords" ADD "DepartmentId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601144900_AddArchiveDeletionWorkflow') THEN
    ALTER TABLE "ArchiveRecords" ADD "IsDeleted" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601144900_AddArchiveDeletionWorkflow') THEN
    CREATE TABLE "DeleteArchiveRequests" (
        "Id" uuid NOT NULL,
        "DepartmentId" uuid NOT NULL,
        "TargetType" integer NOT NULL,
        "TargetId" uuid NOT NULL,
        "RequesterId" uuid NOT NULL,
        "ApproverId" uuid NOT NULL,
        "Status" integer NOT NULL,
        "Justification" text NOT NULL,
        "RejectionReason" text,
        "ApprovalNotes" text,
        "TargetSnapshotJson" jsonb NOT NULL,
        "DependenciesSnapshotJson" jsonb NOT NULL,
        "ActivitySnapshotJson" jsonb,
        "SourceFolderId" uuid,
        "TargetDisplayName" text,
        "ApprovedByUserId" uuid,
        "ApprovedAt" timestamp with time zone,
        "ExecutedByUserId" uuid,
        "ExecutedAt" timestamp with time zone,
        "RejectedByUserId" uuid,
        "RejectedAt" timestamp with time zone,
        "RequesterNotificationMessage" text,
        "RequesterNotifiedAt" timestamp with time zone,
        "RowVersion" bytea NOT NULL,
        "CreatedByUserId" text,
        "CreatedAt" timestamp with time zone,
        "UpdatedByUserId" text,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_DeleteArchiveRequests" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_DeleteArchiveRequests_Departments_DepartmentId" FOREIGN KEY ("DepartmentId") REFERENCES "Departments" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_DeleteArchiveRequests_Users_ApproverId" FOREIGN KEY ("ApproverId") REFERENCES "Users" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_DeleteArchiveRequests_Users_RequesterId" FOREIGN KEY ("RequesterId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601144900_AddArchiveDeletionWorkflow') THEN
    CREATE TABLE "DepartmentArchiveLeaders" (
        "Id" uuid NOT NULL,
        "DepartmentId" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "DeletedByUserId" text,
        "CreatedByUserId" text,
        "CreatedAt" timestamp with time zone,
        "UpdatedByUserId" text,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_DepartmentArchiveLeaders" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_DepartmentArchiveLeaders_Departments_DepartmentId" FOREIGN KEY ("DepartmentId") REFERENCES "Departments" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_DepartmentArchiveLeaders_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601144900_AddArchiveDeletionWorkflow') THEN
    CREATE INDEX "IX_Folders_DepartmentId" ON "Folders" ("DepartmentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601144900_AddArchiveDeletionWorkflow') THEN
    CREATE INDEX "IX_ArchiveRecords_DepartmentId" ON "ArchiveRecords" ("DepartmentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601144900_AddArchiveDeletionWorkflow') THEN
    CREATE INDEX "IX_DeleteArchiveRequests_ApproverId" ON "DeleteArchiveRequests" ("ApproverId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601144900_AddArchiveDeletionWorkflow') THEN
    CREATE INDEX "IX_DeleteArchiveRequests_DepartmentId_TargetType_TargetId_Stat~" ON "DeleteArchiveRequests" ("DepartmentId", "TargetType", "TargetId", "Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601144900_AddArchiveDeletionWorkflow') THEN
    CREATE INDEX "IX_DeleteArchiveRequests_RequesterId" ON "DeleteArchiveRequests" ("RequesterId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601144900_AddArchiveDeletionWorkflow') THEN
    CREATE UNIQUE INDEX "IX_DepartmentArchiveLeaders_DepartmentId_UserId" ON "DepartmentArchiveLeaders" ("DepartmentId", "UserId") WHERE "IsDeleted" = false;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601144900_AddArchiveDeletionWorkflow') THEN
    CREATE INDEX "IX_DepartmentArchiveLeaders_UserId" ON "DepartmentArchiveLeaders" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601144900_AddArchiveDeletionWorkflow') THEN
    ALTER TABLE "ArchiveRecords" ADD CONSTRAINT "FK_ArchiveRecords_Departments_DepartmentId" FOREIGN KEY ("DepartmentId") REFERENCES "Departments" ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601144900_AddArchiveDeletionWorkflow') THEN
    ALTER TABLE "Folders" ADD CONSTRAINT "FK_Folders_Departments_DepartmentId" FOREIGN KEY ("DepartmentId") REFERENCES "Departments" ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260601144900_AddArchiveDeletionWorkflow') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260601144900_AddArchiveDeletionWorkflow', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260604182728_archive-leader') THEN
    ALTER TABLE "PhysicalFiles" ADD "EditArchiveRequestId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260604182728_archive-leader') THEN
    CREATE TABLE "EditArchiveRequests" (
        "Id" uuid NOT NULL,
        "DepartmentId" uuid NOT NULL,
        "ArchiveRecordId" uuid NOT NULL,
        "RequesterId" uuid NOT NULL,
        "ApproverId" uuid,
        "Status" integer NOT NULL,
        "Justification" text NOT NULL,
        "RequestedChangesJson" jsonb NOT NULL,
        "OriginalSnapshotJson" jsonb NOT NULL,
        "RejectionReason" text,
        "ApprovalNotes" text,
        "ApprovedByUserId" uuid,
        "ApprovedAt" timestamp with time zone,
        "RejectedByUserId" uuid,
        "RejectedAt" timestamp with time zone,
        "RowVersion" bytea NOT NULL,
        "CreatedByUserId" text,
        "CreatedAt" timestamp with time zone,
        "UpdatedByUserId" text,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_EditArchiveRequests" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_EditArchiveRequests_ArchiveRecords_ArchiveRecordId" FOREIGN KEY ("ArchiveRecordId") REFERENCES "ArchiveRecords" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_EditArchiveRequests_Departments_DepartmentId" FOREIGN KEY ("DepartmentId") REFERENCES "Departments" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_EditArchiveRequests_Users_ApproverId" FOREIGN KEY ("ApproverId") REFERENCES "Users" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_EditArchiveRequests_Users_RequesterId" FOREIGN KEY ("RequesterId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260604182728_archive-leader') THEN
    CREATE INDEX "IX_PhysicalFiles_EditArchiveRequestId" ON "PhysicalFiles" ("EditArchiveRequestId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260604182728_archive-leader') THEN
    CREATE INDEX "IX_EditArchiveRequests_ApproverId" ON "EditArchiveRequests" ("ApproverId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260604182728_archive-leader') THEN
    CREATE INDEX "IX_EditArchiveRequests_ArchiveRecordId" ON "EditArchiveRequests" ("ArchiveRecordId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260604182728_archive-leader') THEN
    CREATE INDEX "IX_EditArchiveRequests_DepartmentId_ArchiveRecordId_Status" ON "EditArchiveRequests" ("DepartmentId", "ArchiveRecordId", "Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260604182728_archive-leader') THEN
    CREATE INDEX "IX_EditArchiveRequests_RequesterId" ON "EditArchiveRequests" ("RequesterId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260604182728_archive-leader') THEN
    ALTER TABLE "PhysicalFiles" ADD CONSTRAINT "FK_PhysicalFiles_EditArchiveRequests_EditArchiveRequestId" FOREIGN KEY ("EditArchiveRequestId") REFERENCES "EditArchiveRequests" ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260604182728_archive-leader') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260604182728_archive-leader', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260606105725_AddIsQrPage') THEN
    ALTER TABLE "PhysicalFiles" ADD "IsQrPage" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260606105725_AddIsQrPage') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260606105725_AddIsQrPage', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260606110558_UpdateDepartmentArchiveLeaderRelation') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260606110558_UpdateDepartmentArchiveLeaderRelation', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260606141210_UpdateEditArchiveRecoredWorkFlowForDeletion') THEN
    ALTER TABLE "EditArchiveRequests" ADD "RequestedFileDeletionIdsJson" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260606141210_UpdateEditArchiveRecoredWorkFlowForDeletion') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260606141210_UpdateEditArchiveRecoredWorkFlowForDeletion', '10.0.2');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260606191008_RemoveEditArchiveRequestConcurrencyToken') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260606191008_RemoveEditArchiveRequestConcurrencyToken', '10.0.2');
    END IF;
END $EF$;
COMMIT;

