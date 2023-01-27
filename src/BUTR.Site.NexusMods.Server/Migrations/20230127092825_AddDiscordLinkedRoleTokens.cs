using Microsoft.EntityFrameworkCore.Migrations;

using System;

#nullable disable

namespace BUTR.Site.NexusMods.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscordLinkedRoleTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "discord_linked_role_tokens",
                columns: table => new
                {
                    userid = table.Column<string>(name: "user_id", type: "text", nullable: false),
                    refreshtoken = table.Column<string>(name: "refresh_token", type: "text", nullable: false),
                    accesstoken = table.Column<string>(name: "access_token", type: "text", nullable: false),
                    accesstokenexpiresat = table.Column<DateTimeOffset>(name: "access_token_expires_at", type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("discord_linked_role_tokens_pkey", x => x.userid);
                });

            migrationBuilder.CreateTable(
                name: "nexusmods_to_discord",
                columns: table => new
                {
                    nexusmodsuserid = table.Column<int>(name: "nexusmods_user_id", type: "integer", nullable: false),
                    discorduserid = table.Column<string>(name: "discord_user_id", type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("nexusmods_to_discord_pkey", x => x.nexusmodsuserid);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "discord_linked_role_tokens");

            migrationBuilder.DropTable(
                name: "nexusmods_to_discord");
        }
    }
}