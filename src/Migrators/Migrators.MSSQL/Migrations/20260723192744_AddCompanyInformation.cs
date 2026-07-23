using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Blazor.Migrators.MSSQL.Migrations;

public partial class AddCompanyInformation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "CompanyInformation",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                DisplayName = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                TradeName = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                TaxId = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                Phone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                FooterText = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                LastModifiedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_CompanyInformation", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_CompanyInformation_IsActive",
            table: "CompanyInformation",
            column: "IsActive");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "CompanyInformation");
    }
}
