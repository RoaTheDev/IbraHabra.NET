using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IbraHabra.NET.Infra.Migrations
{
    /// <inheritdoc />
    public partial class RemoveChk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_oauth_authorizations_oauth_applications_ApplicationId",
                schema: "identity",
                table: "oauth_authorizations");

            migrationBuilder.DropForeignKey(
                name: "FK_oauth_tokens_oauth_applications_ApplicationId",
                schema: "identity",
                table: "oauth_tokens");

            migrationBuilder.DropIndex(
                name: "IX_oauth_applications_ClientId",
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
                name: "ApplicationType",
                schema: "identity",
                table: "oauth_applications");

            migrationBuilder.DropColumn(
                name: "ClientId",
                schema: "identity",
                table: "oauth_applications");

            migrationBuilder.DropColumn(
                name: "ClientSecret",
                schema: "identity",
                table: "oauth_applications");

            migrationBuilder.DropColumn(
                name: "ClientType",
                schema: "identity",
                table: "oauth_applications");

            migrationBuilder.DropColumn(
                name: "ConcurrencyToken",
                schema: "identity",
                table: "oauth_applications");

            migrationBuilder.DropColumn(
                name: "ConsentType",
                schema: "identity",
                table: "oauth_applications");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                schema: "identity",
                table: "oauth_applications");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                schema: "identity",
                table: "oauth_applications");

            migrationBuilder.DropColumn(
                name: "DisplayNames",
                schema: "identity",
                table: "oauth_applications");

            migrationBuilder.DropColumn(
                name: "JsonWebKeySet",
                schema: "identity",
                table: "oauth_applications");

            migrationBuilder.DropColumn(
                name: "Permissions",
                schema: "identity",
                table: "oauth_applications");

            migrationBuilder.DropColumn(
                name: "PostLogoutRedirectUris",
                schema: "identity",
                table: "oauth_applications");

            migrationBuilder.DropColumn(
                name: "Properties",
                schema: "identity",
                table: "oauth_applications");

            migrationBuilder.DropColumn(
                name: "RedirectUris",
                schema: "identity",
                table: "oauth_applications");

            migrationBuilder.DropColumn(
                name: "Requirements",
                schema: "identity",
                table: "oauth_applications");

            migrationBuilder.DropColumn(
                name: "Settings",
                schema: "identity",
                table: "oauth_applications");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProjectId",
                schema: "identity",
                table: "oauth_applications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "identity",
                table: "oauth_applications",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "identity",
                table: "oauth_applications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "OpenIddictApplications",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ApplicationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ClientId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ClientSecret = table.Column<string>(type: "text", nullable: true),
                    ClientType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConcurrencyToken = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ConsentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    DisplayNames = table.Column<string>(type: "text", nullable: true),
                    JsonWebKeySet = table.Column<string>(type: "text", nullable: true),
                    Permissions = table.Column<string>(type: "text", nullable: true),
                    PostLogoutRedirectUris = table.Column<string>(type: "text", nullable: true),
                    Properties = table.Column<string>(type: "jsonb", nullable: true),
                    RedirectUris = table.Column<string>(type: "text", nullable: true),
                    Requirements = table.Column<string>(type: "text", nullable: true),
                    Settings = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenIddictApplications", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_oauth_applications_ProjectId",
                schema: "identity",
                table: "oauth_applications",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictApplications_ClientId",
                table: "OpenIddictApplications",
                column: "ClientId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_oauth_applications_OpenIddictApplications_Id",
                schema: "identity",
                table: "oauth_applications",
                column: "Id",
                principalTable: "OpenIddictApplications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_oauth_applications_OpenIddictApplications_Id",
                schema: "identity",
                table: "oauth_applications");

            migrationBuilder.DropForeignKey(
                name: "FK_oauth_authorizations_OpenIddictApplications_ApplicationId",
                schema: "identity",
                table: "oauth_authorizations");

            migrationBuilder.DropForeignKey(
                name: "FK_oauth_tokens_OpenIddictApplications_ApplicationId",
                schema: "identity",
                table: "oauth_tokens");

            migrationBuilder.DropTable(
                name: "OpenIddictApplications");

            migrationBuilder.DropIndex(
                name: "IX_oauth_applications_ProjectId",
                schema: "identity",
                table: "oauth_applications");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProjectId",
                schema: "identity",
                table: "oauth_applications",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "identity",
                table: "oauth_applications",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "identity",
                table: "oauth_applications",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationType",
                schema: "identity",
                table: "oauth_applications",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientId",
                schema: "identity",
                table: "oauth_applications",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientSecret",
                schema: "identity",
                table: "oauth_applications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientType",
                schema: "identity",
                table: "oauth_applications",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyToken",
                schema: "identity",
                table: "oauth_applications",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsentType",
                schema: "identity",
                table: "oauth_applications",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                schema: "identity",
                table: "oauth_applications",
                type: "character varying(55)",
                maxLength: 55,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                schema: "identity",
                table: "oauth_applications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisplayNames",
                schema: "identity",
                table: "oauth_applications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JsonWebKeySet",
                schema: "identity",
                table: "oauth_applications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Permissions",
                schema: "identity",
                table: "oauth_applications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostLogoutRedirectUris",
                schema: "identity",
                table: "oauth_applications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Properties",
                schema: "identity",
                table: "oauth_applications",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RedirectUris",
                schema: "identity",
                table: "oauth_applications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Requirements",
                schema: "identity",
                table: "oauth_applications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Settings",
                schema: "identity",
                table: "oauth_applications",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_oauth_applications_ClientId",
                schema: "identity",
                table: "oauth_applications",
                column: "ClientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_oauth_applications_ProjectId_ClientType",
                schema: "identity",
                table: "oauth_applications",
                columns: new[] { "ProjectId", "ClientType" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Client_MinPasswordLength",
                schema: "identity",
                table: "oauth_applications",
                sql: "jsonb_path_exists(\"Properties\", '$.authPolicy.minPasswordLength') AND (\"Properties\"::jsonb->'authPolicy'->>'minPasswordLength')::int >= 6");

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
        }
    }
}
