using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace FridgeServer.Migrations
{
    public partial class GroceryUpdate1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "Timeout",
                table: "Grocery",
                nullable: true,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Grocery",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentCount",
                table: "Grocery",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DueCount",
                table: "Grocery",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "basic",
                table: "Grocery",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentCount",
                table: "Grocery");

            migrationBuilder.DropColumn(
                name: "DueCount",
                table: "Grocery");

            migrationBuilder.DropColumn(
                name: "basic",
                table: "Grocery");

            migrationBuilder.AlterColumn<int>(
                name: "Timeout",
                table: "Grocery",
                nullable: true,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Grocery",
                nullable: true,
                oldClrType: typeof(string));
        }
    }
}
