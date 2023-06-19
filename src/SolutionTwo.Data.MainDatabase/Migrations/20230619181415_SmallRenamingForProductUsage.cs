using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolutionTwo.Data.MainDatabase.Migrations
{
    public partial class SmallRenamingForProductUsage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReleaseDateTimeUtc",
                table: "ProductUsages",
                newName: "ReleasedDateTimeUtc");

            migrationBuilder.RenameColumn(
                name: "IsForceRelease",
                table: "ProductUsages",
                newName: "IsForceReleased");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReleasedDateTimeUtc",
                table: "ProductUsages",
                newName: "ReleaseDateTimeUtc");

            migrationBuilder.RenameColumn(
                name: "IsForceReleased",
                table: "ProductUsages",
                newName: "IsForceRelease");
        }
    }
}
