using Microsoft.EntityFrameworkCore.Migrations;

using System.Collections.Generic;

#nullable disable

namespace BUTR.Site.NexusMods.Server.Migrations
{
    /// <inheritdoc />
    public partial class Steam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nexusmods_to_steam",
                columns: table => new
                {
                    nexusmods_user_id = table.Column<int>(type: "integer", nullable: false),
                    steam_user_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("nexusmods_to_steam_pkey", x => x.nexusmods_user_id);
                });

            migrationBuilder.CreateTable(
                name: "steam_linked_role_tokens",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "text", nullable: false),
                    data = table.Column<Dictionary<string, string>>(type: "hstore", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("steam_linked_role_tokens_pkey", x => x.user_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nexusmods_to_steam");

            migrationBuilder.DropTable(
                name: "steam_linked_role_tokens");
        }
    }
}