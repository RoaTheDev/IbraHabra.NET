using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IbraHabra.NET.Infra.Migrations
{
    /// <inheritdoc />
    public partial class changeIdxClientdb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_oauth_applications_ProjectId_ClientId",
                schema: "identity",
                table: "oauth_applications");

            migrationBuilder.CreateIndex(
                name: "IX_oauth_applications_ProjectId_ClientType",
                schema: "identity",
                table: "oauth_applications",
                columns: new[] { "ProjectId", "ClientType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_oauth_applications_ProjectId_ClientType",
                schema: "identity",
                table: "oauth_applications");

            migrationBuilder.CreateIndex(
                name: "IX_oauth_applications_ProjectId_ClientId",
                schema: "identity",
                table: "oauth_applications",
                columns: new[] { "ProjectId", "ClientId" });
        }
    }
}
