using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolutionTwo.Data.MainDatabase.Migrations
{
    public partial class AddedTenants : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TenantEntity",
                table: "TenantEntity");

            migrationBuilder.RenameTable(
                name: "TenantEntity",
                newName: "Tenants");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tenants",
                table: "Tenants",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Tenants",
                table: "Tenants");

            migrationBuilder.RenameTable(
                name: "Tenants",
                newName: "TenantEntity");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TenantEntity",
                table: "TenantEntity",
                column: "Id");
        }
    }
}
