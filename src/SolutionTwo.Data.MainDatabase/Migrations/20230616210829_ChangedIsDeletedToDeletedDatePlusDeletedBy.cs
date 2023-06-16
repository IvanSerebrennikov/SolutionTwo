using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolutionTwo.Data.MainDatabase.Migrations
{
    public partial class ChangedIsDeletedToDeletedDatePlusDeletedBy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDateTimeUtc",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Tenants",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDateTimeUtc",
                table: "Tenants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE Users SET DeletedDateTimeUtc = DATEFROMPARTS(1, 1, 1), DeletedBy = '00000000-0000-0000-0000-000000000000' WHERE IsDeleted = 1");
            
            migrationBuilder.Sql(
                "UPDATE Tenants SET DeletedDateTimeUtc = DATEFROMPARTS(1, 1, 1), DeletedBy = '00000000-0000-0000-0000-000000000000' WHERE IsDeleted = 1");
            
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Tenants");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Tenants",
                type: "bit",
                nullable: false,
                defaultValue: false);
            
            migrationBuilder.Sql(
                "UPDATE Users SET IsDeleted = 1 WHERE DeletedDateTimeUtc IS NOT NULL");
            
            migrationBuilder.Sql(
                "UPDATE Tenants SET IsDeleted = 1 WHERE DeletedDateTimeUtc IS NOT NULL");
            
            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeletedDateTimeUtc",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "DeletedDateTimeUtc",
                table: "Tenants");
        }
    }
}
