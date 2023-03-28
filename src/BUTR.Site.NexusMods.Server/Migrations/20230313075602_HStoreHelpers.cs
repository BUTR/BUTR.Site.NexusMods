using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

using System.Collections.Generic;

#nullable disable

namespace BUTR.Site.NexusMods.Server.Migrations
{
    /// <inheritdoc />
    public partial class HStoreHelpers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (!migrationBuilder.IsNpgsql())
                return;
            
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
            migrationBuilder.Sql("""
CREATE OR REPLACE FUNCTION hstore_has_keys_values(IN "@hstore" hstore, IN "@keys" text[], IN "@operator" text, IN "@values" text[])
    RETURNS boolean
    AS $func$
    DECLARE 
	    "@i" integer;
	    "@result" boolean;
    BEGIN
	    "@result" := false;
	    FOR "@i" IN 1..array_length("@keys", 1) LOOP
		    IF "@operator" = '=' THEN
			    "@result" := "@hstore" -> "@keys"[ "@i" ] = "@values"[ "@i" ];
		    ELSIF "@operator" = '<>' THEN
			    "@result" := "@hstore" -> "@keys"[ "@i" ] <> "@values"[ "@i" ];
		    ELSIF "@operator" = '>' THEN
			    "@result" := "@hstore" -> "@keys"[ "@i" ] > "@values"[ "@i" ];
		    ELSIF "@operator" = '>=' THEN
			    "@result" := "@hstore" -> "@keys"[ "@i" ] >= "@values"[ "@i" ];
		    ELSIF "@operator" = '<' THEN
			    "@result" := "@hstore" -> "@keys"[ "@i" ] < "@values"[ "@i" ];
		    ELSIF "@operator" = '<=' THEN
			    "@result" := "@hstore" -> "@keys"[ "@i" ] <= "@values"[ "@i" ];
		    ELSIF "@operator" = 'LIKE' THEN
			    "@result" := "@hstore" -> "@keys"[ "@i" ] LIKE "@values"[ "@i" ];
		    ELSE
			    "@result" := "@hstore" -> "@keys"[ "@i" ] = "@values"[ "@i" ];
		    END IF;
		    IF "@result" THEN
			    RETURN "@result";
		    END IF;
	    END LOOP;
	    RETURN "@result";
    END;
$func$ LANGUAGE plpgsql;
""");
            migrationBuilder.Sql("""
CREATE INDEX IF NOT EXISTS crash_report_entity_mod_id_to_version_idx ON crash_report_entity USING gin(mod_id_to_version)
""");
            migrationBuilder.Sql("""
CREATE INDEX IF NOT EXISTS crash_report_entity_mod_ids_idx ON crash_report_entity USING gin(mod_ids)
""");
            migrationBuilder.Sql("""
CREATE INDEX IF NOT EXISTS crash_report_entity_involved_mod_ids_idx ON crash_report_entity USING gin(involved_mod_ids)
""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (!migrationBuilder.IsNpgsql())
                return;
            
            migrationBuilder.Sql("""
DROP FUNCTION hstore_has_key_value;
""");
            migrationBuilder.Sql("""
DROP FUNCTION hstore_has_keys_values;
""");
            migrationBuilder.Sql("""
DROP INDEX crash_report_entity_mod_id_to_version_idx;
""");
            migrationBuilder.Sql("""
DROP INDEX crash_report_entity_mod_ids_idx;
""");
            migrationBuilder.Sql("""
DROP INDEX crash_report_entity_involved_mod_ids_idx;
""");
        }
    }
}