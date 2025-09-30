using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IbraHabra.NET.Infra.Migrations
{
    /// <inheritdoc />
    public partial class changedbname : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_role_roles_RoleId",
                schema: "identity",
                table: "user_role");

            migrationBuilder.DropForeignKey(
                name: "FK_user_role_Users.Commands_UserId",
                schema: "identity",
                table: "user_role");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_role",
                schema: "identity",
                table: "user_role");

            migrationBuilder.RenameTable(
                name: "user_role",
                schema: "identity",
                newName: "user_roles",
                newSchema: "identity");

            migrationBuilder.RenameIndex(
                name: "IX_user_role_RoleId",
                schema: "identity",
                table: "user_roles",
                newName: "IX_user_roles_RoleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_roles",
                schema: "identity",
                table: "user_roles",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.AddForeignKey(
                name: "FK_user_roles_roles_RoleId",
                schema: "identity",
                table: "user_roles",
                column: "RoleId",
                principalSchema: "identity",
                principalTable: "roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_roles_Users.Commands_UserId",
                schema: "identity",
                table: "user_roles",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "Users.Commands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_roles_roles_RoleId",
                schema: "identity",
                table: "user_roles");

            migrationBuilder.DropForeignKey(
                name: "FK_user_roles_Users.Commands_UserId",
                schema: "identity",
                table: "user_roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_roles",
                schema: "identity",
                table: "user_roles");

            migrationBuilder.RenameTable(
                name: "user_roles",
                schema: "identity",
                newName: "user_role",
                newSchema: "identity");

            migrationBuilder.RenameIndex(
                name: "IX_user_roles_RoleId",
                schema: "identity",
                table: "user_role",
                newName: "IX_user_role_RoleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_role",
                schema: "identity",
                table: "user_role",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.AddForeignKey(
                name: "FK_user_role_roles_RoleId",
                schema: "identity",
                table: "user_role",
                column: "RoleId",
                principalSchema: "identity",
                principalTable: "roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_role_Users.Commands_UserId",
                schema: "identity",
                table: "user_role",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "Users.Commands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
