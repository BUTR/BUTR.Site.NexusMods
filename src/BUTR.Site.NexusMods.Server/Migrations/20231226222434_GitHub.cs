using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BUTR.Site.NexusMods.Server.Migrations
{
    /// <inheritdoc />
    public partial class GitHub : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nexusmods_user_to_integration_github",
                schema: "nexusmods_user",
                columns: table => new
                {
                    nexusmods_user_to_github_id = table.Column<int>(type: "integer", nullable: false),
                    github_user_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexusmods_user_to_integration_github", x => x.nexusmods_user_to_github_id);
                    table.UniqueConstraint("AK_nexusmods_user_to_integration_github_github_user_id", x => x.github_user_id);
                    table.ForeignKey(
                        name: "FK_nexusmods_user_to_integration_github_nexusmods_user_nexusmo~",
                        column: x => x.nexusmods_user_to_github_id,
                        principalSchema: "nexusmods_user",
                        principalTable: "nexusmods_user",
                        principalColumn: "nexusmods_user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "integration_github_tokens",
                schema: "integration",
                columns: table => new
                {
                    integration_github_tokens_id = table.Column<int>(type: "integer", nullable: false),
                    github_user_id = table.Column<string>(type: "text", nullable: false),
                    access_token = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_integration_github_tokens", x => x.integration_github_tokens_id);
                    table.ForeignKey(
                        name: "FK_integration_github_tokens_nexusmods_user_integration_github~",
                        column: x => x.integration_github_tokens_id,
                        principalSchema: "nexusmods_user",
                        principalTable: "nexusmods_user",
                        principalColumn: "nexusmods_user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_integration_github_tokens_nexusmods_user_to_integration_git~",
                        column: x => x.github_user_id,
                        principalSchema: "nexusmods_user",
                        principalTable: "nexusmods_user_to_integration_github",
                        principalColumn: "github_user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_integration_github_tokens_github_user_id",
                schema: "integration",
                table: "integration_github_tokens",
                column: "github_user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "integration_github_tokens",
                schema: "integration");

            migrationBuilder.DropTable(
                name: "nexusmods_user_to_integration_github",
                schema: "nexusmods_user");
        }
    }
}
