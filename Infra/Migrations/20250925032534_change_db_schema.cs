using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IbraHabra.NET.Infra.Migrations
{
    /// <inheritdoc />
    public partial class change_db_schema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_oauth_authorizations_OpenIddictApplications_ApplicationId",
                schema: "identity",
                table: "oauth_authorizations");

            migrationBuilder.DropForeignKey(
                name: "FK_oauth_tokens_OpenIddictApplications_ApplicationId",
                schema: "identity",
                table: "oauth_tokens");

            migrationBuilder.DropForeignKey(
                name: "FK_roles_oauth_application_OpenIddictEntityFrameworkCoreApplic~",
                schema: "identity",
                table: "roles");

            migrationBuilder.DropTable(
                name: "OpenIddictEntityFrameworkCoreToken<Guid>");

            migrationBuilder.DropTable(
                name: "OpenIddictEntityFrameworkCoreAuthorization<Guid>");

            migrationBuilder.DropTable(
                name: "oauth_application",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "OpenIddictEntityFrameworkCoreApplication<Guid>");

            migrationBuilder.DropIndex(
                name: "IX_roles_OpenIddictEntityFrameworkCoreApplication<Guid, Role, ~",
                schema: "identity",
                table: "roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OpenIddictApplications",
                table: "OpenIddictApplications");

            migrationBuilder.DropColumn(
                name: "OpenIddictEntityFrameworkCoreApplication<Guid, Role, OpenIddic~",
                schema: "identity",
                table: "roles");

            migrationBuilder.RenameTable(
                name: "OpenIddictApplications",
                newName: "oauth_applications",
                newSchema: "identity");

            migrationBuilder.RenameIndex(
                name: "IX_OpenIddictApplications_ClientId",
                schema: "identity",
                table: "oauth_applications",
                newName: "IX_oauth_applications_ClientId");

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectRoleId1",
                schema: "realms",
                table: "project_members",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ClientType",
                schema: "identity",
                table: "oauth_applications",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "identity",
                table: "oauth_applications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                schema: "identity",
                table: "oauth_applications",
                type: "character varying(55)",
                maxLength: 55,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "identity",
                table: "oauth_applications",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinPasswordLength",
                schema: "identity",
                table: "oauth_applications",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                schema: "identity",
                table: "oauth_applications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequireDigit",
                schema: "identity",
                table: "oauth_applications",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequireEmailVerification",
                schema: "identity",
                table: "oauth_applications",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequireMfa",
                schema: "identity",
                table: "oauth_applications",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequireNonAlphanumeric",
                schema: "identity",
                table: "oauth_applications",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequirePkce",
                schema: "identity",
                table: "oauth_applications",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequireUppercase",
                schema: "identity",
                table: "oauth_applications",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "identity",
                table: "oauth_applications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_oauth_applications",
                schema: "identity",
                table: "oauth_applications",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_project_members_ProjectRoleId1",
                schema: "realms",
                table: "project_members",
                column: "ProjectRoleId1");

            migrationBuilder.CreateIndex(
                name: "IX_oauth_applications_ProjectId_ClientType",
                schema: "identity",
                table: "oauth_applications",
                columns: new[] { "ProjectId", "ClientType" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Client_MinPasswordLength",
                schema: "identity",
                table: "oauth_applications",
                sql: "\"MinPasswordLength\" >= 6");

            migrationBuilder.AddForeignKey(
                name: "FK_oauth_applications_projects_ProjectId",
                schema: "identity",
                table: "oauth_applications",
                column: "ProjectId",
                principalSchema: "realms",
                principalTable: "projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_oauth_authorizations_oauth_applications_ApplicationId",
                schema: "identity",
                table: "oauth_authorizations",
                column: "ApplicationId",
                principalSchema: "identity",
                principalTable: "oauth_applications",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_oauth_tokens_oauth_applications_ApplicationId",
                schema: "identity",
                table: "oauth_tokens",
                column: "ApplicationId",
                principalSchema: "identity",
                principalTable: "oauth_applications",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_project_members_project_roles_ProjectRoleId1",
                schema: "realms",
                table: "project_members",
                column: "ProjectRoleId1",
                principalSchema: "realms",
                principalTable: "project_roles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_oauth_applications_projects_ProjectId",
                schema: "identity",
                table: "oauth_applications");

            migrationBuilder.DropForeignKey(
                name: "FK_oauth_authorizations_oauth_applications_ApplicationId",
                schema: "identity",
                table: "oauth_authorizations");

            migrationBuilder.DropForeignKey(
                name: "FK_oauth_tokens_oauth_applications_ApplicationId",
                schema: "identity",
                table: "oauth_tokens");

            migrationBuilder.DropForeignKey(
                name: "FK_project_members_project_roles_ProjectRoleId1",
                schema: "realms",
                table: "project_members");

            migrationBuilder.DropIndex(
                name: "IX_project_members_ProjectRoleId1",
                schema: "realms",
                table: "project_members");

            migrationBuilder.DropPrimaryKey(
                name: "PK_oauth_applications",
                schema: "identity",
                table: "oauth_applications");

            migrationBuilder.DropIndex(
                name: "IX_oauth_applications_ProjectId_ClientType",
                schema: "identity",
                table: "oauth_applications");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Client_MinPasswordLength",
                schema: "identity",
                table: "oauth_applications");

            migrationBuilder.DropColumn(
                name: "ProjectRoleId1",
                schema: "realms",
                table: "project_members");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "identity",
                table: "oauth_applications");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                schema: "identity",
                table: "oauth_applications");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "identity",
                table: "oauth_applications");

            migrationBuilder.DropColumn(
                name: "MinPasswordLength",
                schema: "identity",
                table: "oauth_applications");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                schema: "identity",
                table: "oauth_applications");

            migrationBuilder.DropColumn(
                name: "RequireDigit",
                schema: "identity",
                table: "oauth_applications");

            migrationBuilder.DropColumn(
                name: "RequireEmailVerification",
                schema: "identity",
                table: "oauth_applications");

            migrationBuilder.DropColumn(
                name: "RequireMfa",
                schema: "identity",
                table: "oauth_applications");

            migrationBuilder.DropColumn(
                name: "RequireNonAlphanumeric",
                schema: "identity",
                table: "oauth_applications");

            migrationBuilder.DropColumn(
                name: "RequirePkce",
                schema: "identity",
                table: "oauth_applications");

            migrationBuilder.DropColumn(
                name: "RequireUppercase",
                schema: "identity",
                table: "oauth_applications");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "identity",
                table: "oauth_applications");

            migrationBuilder.RenameTable(
                name: "oauth_applications",
                schema: "identity",
                newName: "OpenIddictApplications");

            migrationBuilder.RenameIndex(
                name: "IX_oauth_applications_ClientId",
                table: "OpenIddictApplications",
                newName: "IX_OpenIddictApplications_ClientId");

            migrationBuilder.AddColumn<Guid>(
                name: "OpenIddictEntityFrameworkCoreApplication<Guid, Role, OpenIddic~",
                schema: "identity",
                table: "roles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ClientType",
                table: "OpenIddictApplications",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddPrimaryKey(
                name: "PK_OpenIddictApplications",
                table: "OpenIddictApplications",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "oauth_application",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationType = table.Column<string>(type: "text", nullable: true),
                    ClientId = table.Column<string>(type: "text", nullable: true),
                    ClientSecret = table.Column<string>(type: "text", nullable: true),
                    ClientType = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyToken = table.Column<string>(type: "text", nullable: true),
                    ConsentType = table.Column<string>(type: "text", nullable: true),
                    Discriminator = table.Column<string>(type: "character varying(144)", maxLength: 144, nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    DisplayNames = table.Column<string>(type: "text", nullable: true),
                    JsonWebKeySet = table.Column<string>(type: "text", nullable: true),
                    Permissions = table.Column<string>(type: "text", nullable: true),
                    PostLogoutRedirectUris = table.Column<string>(type: "text", nullable: true),
                    Properties = table.Column<string>(type: "text", nullable: true),
                    RedirectUris = table.Column<string>(type: "text", nullable: true),
                    Requirements = table.Column<string>(type: "text", nullable: true),
                    Settings = table.Column<string>(type: "text", nullable: true),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true),
                    MinPasswordLength = table.Column<int>(type: "integer", nullable: true),
                    RequireDigit = table.Column<bool>(type: "boolean", nullable: true),
                    RequireEmailVerification = table.Column<bool>(type: "boolean", nullable: true),
                    RequireMfa = table.Column<bool>(type: "boolean", nullable: true),
                    RequireNonAlphanumeric = table.Column<bool>(type: "boolean", nullable: true),
                    RequirePkce = table.Column<bool>(type: "boolean", nullable: true),
                    RequireUppercase = table.Column<bool>(type: "boolean", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_oauth_application", x => x.Id);
                    table.CheckConstraint("CK_Client_MinPasswordLength", "\"MinPasswordLength\" >= 6");
                    table.ForeignKey(
                        name: "FK_oauth_application_projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "realms",
                        principalTable: "projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictEntityFrameworkCoreApplication<Guid>",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationType = table.Column<string>(type: "text", nullable: true),
                    ClientId = table.Column<string>(type: "text", nullable: true),
                    ClientSecret = table.Column<string>(type: "text", nullable: true),
                    ClientType = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyToken = table.Column<string>(type: "text", nullable: true),
                    ConsentType = table.Column<string>(type: "text", nullable: true),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    DisplayNames = table.Column<string>(type: "text", nullable: true),
                    JsonWebKeySet = table.Column<string>(type: "text", nullable: true),
                    Permissions = table.Column<string>(type: "text", nullable: true),
                    PostLogoutRedirectUris = table.Column<string>(type: "text", nullable: true),
                    Properties = table.Column<string>(type: "text", nullable: true),
                    RedirectUris = table.Column<string>(type: "text", nullable: true),
                    Requirements = table.Column<string>(type: "text", nullable: true),
                    Settings = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenIddictEntityFrameworkCoreApplication<Guid>", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictEntityFrameworkCoreAuthorization<Guid>",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConcurrencyToken = table.Column<string>(type: "text", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Properties = table.Column<string>(type: "text", nullable: true),
                    Scopes = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true),
                    Subject = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenIddictEntityFrameworkCoreAuthorization<Guid>", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpenIddictEntityFrameworkCoreAuthorization<Guid>_OpenIddict~",
                        column: x => x.ApplicationId,
                        principalTable: "OpenIddictEntityFrameworkCoreApplication<Guid>",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictEntityFrameworkCoreToken<Guid>",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    AuthorizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConcurrencyToken = table.Column<string>(type: "text", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OpenIddictEntityFrameworkCoreApplicationGuidRoleOpenIddic = table.Column<Guid>(name: "OpenIddictEntityFrameworkCoreApplication<Guid, Role, OpenIddic~", type: "uuid", nullable: true),
                    Payload = table.Column<string>(type: "text", nullable: true),
                    Properties = table.Column<string>(type: "text", nullable: true),
                    RedemptionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReferenceId = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true),
                    Subject = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenIddictEntityFrameworkCoreToken<Guid>", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpenIddictEntityFrameworkCoreToken<Guid>_OpenIddictEntityFr~",
                        column: x => x.ApplicationId,
                        principalTable: "OpenIddictEntityFrameworkCoreApplication<Guid>",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OpenIddictEntityFrameworkCoreToken<Guid>_OpenIddictEntityF~1",
                        column: x => x.AuthorizationId,
                        principalTable: "OpenIddictEntityFrameworkCoreAuthorization<Guid>",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OpenIddictEntityFrameworkCoreToken<Guid>_oauth_application_~",
                        column: x => x.OpenIddictEntityFrameworkCoreApplicationGuidRoleOpenIddic,
                        principalSchema: "identity",
                        principalTable: "oauth_application",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_roles_OpenIddictEntityFrameworkCoreApplication<Guid, Role, ~",
                schema: "identity",
                table: "roles",
                column: "OpenIddictEntityFrameworkCoreApplication<Guid, Role, OpenIddic~");

            migrationBuilder.CreateIndex(
                name: "IX_oauth_application_ProjectId_ClientType",
                schema: "identity",
                table: "oauth_application",
                columns: new[] { "ProjectId", "ClientType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictEntityFrameworkCoreAuthorization<Guid>_Applicatio~",
                table: "OpenIddictEntityFrameworkCoreAuthorization<Guid>",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictEntityFrameworkCoreToken<Guid>_ApplicationId",
                table: "OpenIddictEntityFrameworkCoreToken<Guid>",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictEntityFrameworkCoreToken<Guid>_AuthorizationId",
                table: "OpenIddictEntityFrameworkCoreToken<Guid>",
                column: "AuthorizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictEntityFrameworkCoreToken<Guid>_OpenIddictEntityFr~",
                table: "OpenIddictEntityFrameworkCoreToken<Guid>",
                column: "OpenIddictEntityFrameworkCoreApplication<Guid, Role, OpenIddic~");

            migrationBuilder.AddForeignKey(
                name: "FK_oauth_authorizations_OpenIddictApplications_ApplicationId",
                schema: "identity",
                table: "oauth_authorizations",
                column: "ApplicationId",
                principalTable: "OpenIddictApplications",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_oauth_tokens_OpenIddictApplications_ApplicationId",
                schema: "identity",
                table: "oauth_tokens",
                column: "ApplicationId",
                principalTable: "OpenIddictApplications",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_roles_oauth_application_OpenIddictEntityFrameworkCoreApplic~",
                schema: "identity",
                table: "roles",
                column: "OpenIddictEntityFrameworkCoreApplication<Guid, Role, OpenIddic~",
                principalSchema: "identity",
                principalTable: "oauth_application",
                principalColumn: "Id");
        }
    }
}
