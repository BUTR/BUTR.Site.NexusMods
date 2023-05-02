using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BUTR.Site.NexusMods.Server.Migrations
{
    /// <inheritdoc />
    public partial class Renamings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nexusmods_mod_manual_link_nexusmods_users",
                columns: table => new
                {
                    nexusmods_mod_id = table.Column<int>(type: "integer", nullable: false),
                    allowed_nexusmods_user_ids = table.Column<int[]>(type: "integer[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("nexusmods_mod_manual_link_nexusmods_users_pkey", x => x.nexusmods_mod_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nexusmods_mod_manual_link_nexusmods_users");
        }
    }
}
