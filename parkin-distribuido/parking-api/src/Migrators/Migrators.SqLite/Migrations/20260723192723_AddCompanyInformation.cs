using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Blazor.Migrators.SqLite.Migrations;

public partial class AddCompanyInformation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "CompanyInformation",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                DisplayName = table.Column<string>(type: "TEXT", maxLength: 180, nullable: false),
                TradeName = table.Column<string>(type: "TEXT", maxLength: 180, nullable: false),
                TaxId = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                Address = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                Phone = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                FooterText = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                CreatedById = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                LastModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                LastModifiedById = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true)
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
