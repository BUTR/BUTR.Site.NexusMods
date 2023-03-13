using Microsoft.EntityFrameworkCore.Migrations;

using System.Collections.Generic;

#nullable disable

namespace BUTR.Site.NexusMods.Server.Migrations
{
    /// <inheritdoc />
    public partial class CrashReportEntityMods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Dictionary<string, string>>(
                name: "mod_id_to_version",
                table: "crash_report_entity",
                type: "hstore",
                nullable: false,
                defaultValue: new Dictionary<string, string>() { { "ignore", "ignore" } });
            migrationBuilder.Sql("""
CREATE OR REPLACE FUNCTION hstore_has_key_value(IN "@hstore" hstore, IN "@key" text, IN "@operator" text, IN "@value" text)
    RETURNS boolean
	AS $func$
	SELECT
		CASE 
			WHEN "@operator" = '=' THEN "@hstore" -> "@key" = "@value"
			WHEN "@operator" = '<>' THEN "@hstore" -> "@key" <> "@value" 
			WHEN "@operator" = '>' THEN "@hstore" -> "@key" > "@value" 
			WHEN "@operator" = '>=' THEN "@hstore" -> "@key" >= "@value" 
			WHEN "@operator" = '<' THEN "@hstore" -> "@key" < "@value" 
			WHEN "@operator" = '<=' THEN "@hstore" -> "@key" <= "@value"
			WHEN "@operator" = 'LIKE' THEN "@hstore" -> "@key" LIKE "@value"
			ELSE "@hstore" -> "@key" = "@value"
		END
$func$ LANGUAGE sql;
""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "mod_id_to_version",
                table: "crash_report_entity");
            migrationBuilder.Sql("""
DROP FUNCTION hstore_has_key_value;
""");
        }
    }
}