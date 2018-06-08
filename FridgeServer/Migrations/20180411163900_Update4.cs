using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace FridgeServer.Migrations
{
    public partial class Update4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Details");

            migrationBuilder.AlterColumn<long>(
                name: "LifeTime",
                table: "MoreInformations",
                nullable: true,
                oldClrType: typeof(long));

            migrationBuilder.AlterColumn<long>(
                name: "Date",
                table: "MoreInformations",
                nullable: true,
                oldClrType: typeof(long));

            migrationBuilder.AddColumn<int>(
                name: "No",
                table: "MoreInformations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "typeOfNo",
                table: "MoreInformations",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "No",
                table: "MoreInformations");

            migrationBuilder.DropColumn(
                name: "typeOfNo",
                table: "MoreInformations");

            migrationBuilder.AlterColumn<long>(
                name: "LifeTime",
                table: "MoreInformations",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Date",
                table: "MoreInformations",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Details",
                columns: table => new
                {
                    DetailsId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    GroceryId = table.Column<int>(nullable: true),
                    No = table.Column<int>(nullable: false),
                    typeOfNo = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Details", x => x.DetailsId);
                    table.ForeignKey(
                        name: "FK_Details_Grocery_GroceryId",
                        column: x => x.GroceryId,
                        principalTable: "Grocery",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Details_GroceryId",
                table: "Details",
                column: "GroceryId");
        }
    }
}
