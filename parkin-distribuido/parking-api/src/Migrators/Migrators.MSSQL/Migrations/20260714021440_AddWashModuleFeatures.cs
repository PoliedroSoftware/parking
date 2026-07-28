using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Blazor.Migrators.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddWashModuleFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ChargeToMonthly",
                table: "CarWashes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionTotal",
                table: "CarWashes",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "MemberId",
                table: "CarWashes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CarWashAdditionals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CarWashId = table.Column<int>(type: "int", nullable: false),
                    AdditionalName = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarWashAdditionals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarWashAdditionals_CarWashes_CarWashId",
                        column: x => x.CarWashId,
                        principalTable: "CarWashes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CarWashOperators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CarWashId = table.Column<int>(type: "int", nullable: false),
                    OperatorName = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Commission = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarWashOperators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarWashOperators_CarWashes_CarWashId",
                        column: x => x.CarWashId,
                        principalTable: "CarWashes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WashAdditionals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WashAdditionals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WashServicePrices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ServiceType = table.Column<int>(type: "int", nullable: false),
                    VehicleType = table.Column<int>(type: "int", nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WashServicePrices", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarWashes_MemberId",
                table: "CarWashes",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_CarWashAdditionals_CarWashId",
                table: "CarWashAdditionals",
                column: "CarWashId");

            migrationBuilder.CreateIndex(
                name: "IX_CarWashOperators_CarWashId",
                table: "CarWashOperators",
                column: "CarWashId");

            migrationBuilder.AddForeignKey(
                name: "FK_CarWashes_Members_MemberId",
                table: "CarWashes",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CarWashes_Members_MemberId",
                table: "CarWashes");

            migrationBuilder.DropTable(
                name: "CarWashAdditionals");

            migrationBuilder.DropTable(
                name: "CarWashOperators");

            migrationBuilder.DropTable(
                name: "WashAdditionals");

            migrationBuilder.DropTable(
                name: "WashServicePrices");

            migrationBuilder.DropIndex(
                name: "IX_CarWashes_MemberId",
                table: "CarWashes");

            migrationBuilder.DropColumn(
                name: "ChargeToMonthly",
                table: "CarWashes");

            migrationBuilder.DropColumn(
                name: "CommissionTotal",
                table: "CarWashes");

            migrationBuilder.DropColumn(
                name: "MemberId",
                table: "CarWashes");
        }
    }
}
