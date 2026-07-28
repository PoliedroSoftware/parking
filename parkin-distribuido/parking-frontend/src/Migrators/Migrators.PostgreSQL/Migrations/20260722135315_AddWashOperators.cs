using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CleanArchitecture.Blazor.Migrators.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddWashOperators : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "wash_operators",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    document_number = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by_id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    last_modified_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    last_modified_by_id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wash_operators", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_wash_operators_document_number",
                table: "wash_operators",
                column: "document_number");

            migrationBuilder.CreateIndex(
                name: "ix_wash_operators_name",
                table: "wash_operators",
                column: "name");

            migrationBuilder.Sql("""
                INSERT INTO wash_operators (name, is_active, created_at)
                SELECT DISTINCT btrim(cwo.operator_name), TRUE, CURRENT_TIMESTAMP::timestamp
                FROM car_wash_operators AS cwo
                WHERE cwo.operator_name IS NOT NULL
                  AND btrim(cwo.operator_name) <> ''
                  AND NOT EXISTS (
                      SELECT 1
                      FROM wash_operators AS wo
                      WHERE upper(wo.name) = upper(btrim(cwo.operator_name))
                  );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "wash_operators");
        }
    }
}
