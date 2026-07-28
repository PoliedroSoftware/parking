using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Blazor.Migrators.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddWashQueueFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedDelivery",
                table: "CarWashes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QueueNumber",
                table: "CarWashes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedDelivery",
                table: "CarWashes");

            migrationBuilder.DropColumn(
                name: "QueueNumber",
                table: "CarWashes");
        }
    }
}
