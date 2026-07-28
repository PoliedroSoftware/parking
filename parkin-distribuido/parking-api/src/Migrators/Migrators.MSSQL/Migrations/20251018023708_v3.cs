using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Blazor.Migrators.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class v3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_Members_MemberId",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_MemberId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "MemberId",
                table: "Vehicles");

            migrationBuilder.CreateTable(
                name: "MemberVehicles",
                columns: table => new
                {
                    MemberId = table.Column<int>(type: "int", nullable: false),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberVehicles", x => new { x.MemberId, x.VehicleId });
                    table.ForeignKey(
                        name: "FK_MemberVehicles_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MemberVehicles_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MemberVehicles_VehicleId",
                table: "MemberVehicles",
                column: "VehicleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemberVehicles");

            migrationBuilder.AddColumn<int>(
                name: "MemberId",
                table: "Vehicles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_MemberId",
                table: "Vehicles",
                column: "MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_Members_MemberId",
                table: "Vehicles",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id");
        }
    }
}
