using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Blazor.Migrators.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class RemoveWashOperatorDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_wash_operators_document_number",
                table: "wash_operators");

            migrationBuilder.DropColumn(
                name: "document_number",
                table: "wash_operators");

            migrationBuilder.DropColumn(
                name: "notes",
                table: "wash_operators");

            migrationBuilder.DropColumn(
                name: "phone_number",
                table: "wash_operators");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "document_number",
                table: "wash_operators",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "notes",
                table: "wash_operators",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "phone_number",
                table: "wash_operators",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_wash_operators_document_number",
                table: "wash_operators",
                column: "document_number");
        }
    }
}
