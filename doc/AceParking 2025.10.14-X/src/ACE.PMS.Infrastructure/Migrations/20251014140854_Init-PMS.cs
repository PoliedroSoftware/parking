using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ACE.PMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitPMS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Carparks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppKey = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    MachineCode = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    RegistrationCode = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Name_Code = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Name_En = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    Name_Tc = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Address_En = table.Column<string>(type: "varchar(512)", unicode: false, maxLength: 512, nullable: true),
                    Address_Tc = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CompanyName_En = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    CompanyName_Tc = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ContactPerson = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Fax = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(2)", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2(2)", nullable: true),
                    LastModifiedById = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carparks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Charges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    BeforeContent = table.Column<string>(type: "nvarchar(max)", maxLength: 2147483647, nullable: false),
                    AfterContent = table.Column<string>(type: "nvarchar(max)", maxLength: 2147483647, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(2)", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2(2)", nullable: true),
                    LastModifiedById = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Charges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Holidays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2(2)", nullable: false),
                    Name_En = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    Name_Tc = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(2)", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2(2)", nullable: true),
                    LastModifiedById = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Holidays", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MemberRental",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MemberId = table.Column<int>(type: "int", nullable: false),
                    LicensePlate = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    CardId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    RentalFee = table.Column<decimal>(type: "decimal(9,1)", nullable: false),
                    Deposit = table.Column<decimal>(type: "decimal(9,1)", nullable: false),
                    AmountDue = table.Column<decimal>(type: "decimal(9,1)", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(9,1)", nullable: false),
                    PaymentTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentMethodId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberRental", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Zones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name_Code = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Name_En = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    Name_Tc = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CarparkId = table.Column<int>(type: "int", nullable: false),
                    HolidaySets = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, defaultValue: "1,0,0,0,0,0,1,1"),
                    IsOpenCashbox = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    HourlySets = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    MonthlySets = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(2)", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2(2)", nullable: true),
                    LastModifiedById = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Zones_Carparks_CarparkId",
                        column: x => x.CarparkId,
                        principalTable: "Carparks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Gates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ZoneId = table.Column<int>(type: "int", nullable: false),
                    GateType = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    LaneNo = table.Column<int>(type: "int", nullable: false),
                    IsUpper = table.Column<bool>(type: "bit", nullable: false),
                    IsLefthand = table.Column<bool>(type: "bit", nullable: false),
                    HourlyPermitTypes = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    MonthlyPermitTypes = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(2)", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2(2)", nullable: true),
                    LastModifiedById = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Gates_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpaceGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    ZoneId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(2)", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2(2)", nullable: true),
                    LastModifiedById = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpaceGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpaceGroups_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ServiceCategoryId = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    VehicleTypeId = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ZoneId = table.Column<int>(type: "int", nullable: false),
                    ChargeId = table.Column<int>(type: "int", nullable: true),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    ManualFull = table.Column<bool>(type: "bit", nullable: false),
                    CanRecognizePlate = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(2)", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2(2)", nullable: true),
                    LastModifiedById = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehicles_Charges_ChargeId",
                        column: x => x.ChargeId,
                        principalTable: "Charges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Vehicles_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LicensePlate = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    CardId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    SpaceGroupId = table.Column<int>(type: "int", nullable: true),
                    AllowedZoneIds = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SpaceType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SpaceNo = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    MobileNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(2)", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2(2)", nullable: true),
                    LastModifiedById = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Members_SpaceGroups_SpaceGroupId",
                        column: x => x.SpaceGroupId,
                        principalTable: "SpaceGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Members_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Charge_Id",
                table: "Charges",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Gates_Name",
                table: "Gates",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Gates_ZoneId",
                table: "Gates",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_Holidays_Date",
                table: "Holidays",
                column: "Date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MemberRental_CardId",
                table: "MemberRental",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberRental_LicensePlate",
                table: "MemberRental",
                column: "LicensePlate");

            migrationBuilder.CreateIndex(
                name: "IX_Members_CardId",
                table: "Members",
                column: "CardId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_LicensePlate",
                table: "Members",
                column: "LicensePlate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_SpaceGroupId",
                table: "Members",
                column: "SpaceGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Members_VehicleId",
                table: "Members",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_SpaceGroups_Name",
                table: "SpaceGroups",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpaceGroups_ZoneId",
                table: "SpaceGroups",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_ChargeId",
                table: "Vehicles",
                column: "ChargeId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_Name",
                table: "Vehicles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_ZoneId",
                table: "Vehicles",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_Zones_CarparkId",
                table: "Zones",
                column: "CarparkId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Gates");

            migrationBuilder.DropTable(
                name: "Holidays");

            migrationBuilder.DropTable(
                name: "MemberRental");

            migrationBuilder.DropTable(
                name: "Members");

            migrationBuilder.DropTable(
                name: "SpaceGroups");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "Charges");

            migrationBuilder.DropTable(
                name: "Zones");

            migrationBuilder.DropTable(
                name: "Carparks");
        }
    }
}
