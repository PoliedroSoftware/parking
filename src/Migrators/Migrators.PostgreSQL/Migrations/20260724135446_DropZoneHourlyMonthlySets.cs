using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Blazor.Migrators.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class DropZoneHourlyMonthlySets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "hourly_sets",
                table: "zones");

            migrationBuilder.DropColumn(
                name: "monthly_sets",
                table: "zones");

            migrationBuilder.AddColumn<int>(
                name: "adjustment",
                table: "zones",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "capacity",
                table: "zones",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "exit_buffer",
                table: "zones",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "grace_period",
                table: "zones",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "lost_ticket_fee",
                table: "zones",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "manual_full",
                table: "zones",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "monthly_deposit",
                table: "zones",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "adjustment",
                table: "zones");

            migrationBuilder.DropColumn(
                name: "capacity",
                table: "zones");

            migrationBuilder.DropColumn(
                name: "exit_buffer",
                table: "zones");

            migrationBuilder.DropColumn(
                name: "grace_period",
                table: "zones");

            migrationBuilder.DropColumn(
                name: "lost_ticket_fee",
                table: "zones");

            migrationBuilder.DropColumn(
                name: "manual_full",
                table: "zones");

            migrationBuilder.DropColumn(
                name: "monthly_deposit",
                table: "zones");

            migrationBuilder.AddColumn<string>(
                name: "hourly_sets",
                table: "zones",
                type: "text",
                maxLength: 2147483647,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "monthly_sets",
                table: "zones",
                type: "text",
                maxLength: 2147483647,
                nullable: false,
                defaultValue: "");
        }
    }
}
