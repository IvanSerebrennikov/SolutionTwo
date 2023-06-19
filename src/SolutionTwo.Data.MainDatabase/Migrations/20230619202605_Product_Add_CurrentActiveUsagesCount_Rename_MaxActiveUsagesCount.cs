﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolutionTwo.Data.MainDatabase.Migrations
{
    public partial class Product_Add_CurrentActiveUsagesCount_Rename_MaxActiveUsagesCount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MaxNumberOfSimultaneousUsages",
                table: "Products",
                newName: "MaxActiveUsagesCount");

            migrationBuilder.AddColumn<int>(
                name: "CurrentActiveUsagesCount",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentActiveUsagesCount",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "MaxActiveUsagesCount",
                table: "Products",
                newName: "MaxNumberOfSimultaneousUsages");
        }
    }
}
