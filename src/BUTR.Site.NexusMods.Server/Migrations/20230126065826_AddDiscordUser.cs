using Microsoft.EntityFrameworkCore.Migrations;

using System;

#nullable disable

namespace BUTR.Site.NexusMods.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscordUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "discord_user",
                columns: table => new
                {
                    userid = table.Column<int>(name: "user_id", type: "integer", nullable: false),
                    accesstoken = table.Column<string>(name: "access_token", type: "text", nullable: false),
                    refreshtoken = table.Column<string>(name: "refresh_token", type: "text", nullable: false),
                    expiresat = table.Column<DateTimeOffset>(name: "expires_at", type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("discord_user_pkey", x => x.userid);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "discord_user");
        }
    }
}