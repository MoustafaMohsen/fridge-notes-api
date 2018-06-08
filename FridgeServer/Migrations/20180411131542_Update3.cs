using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace FridgeServer.Migrations
{
    public partial class Update3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentCount",
                table: "Grocery");

            migrationBuilder.DropColumn(
                name: "DueCount",
                table: "Grocery");

            migrationBuilder.DropColumn(
                name: "DueList",
                table: "Grocery");

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

            migrationBuilder.CreateTable(
                name: "MoreInformations",
                columns: table => new
                {
                    MoreInformationsId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Bought = table.Column<bool>(nullable: false),
                    Date = table.Column<long>(nullable: false),
                    GroceryId = table.Column<int>(nullable: true),
                    LifeTime = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoreInformations", x => x.MoreInformationsId);
                    table.ForeignKey(
                        name: "FK_MoreInformations_Grocery_GroceryId",
                        column: x => x.GroceryId,
                        principalTable: "Grocery",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Details_GroceryId",
                table: "Details",
                column: "GroceryId");

            migrationBuilder.CreateIndex(
                name: "IX_MoreInformations_GroceryId",
                table: "MoreInformations",
                column: "GroceryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Details");

            migrationBuilder.DropTable(
                name: "MoreInformations");

            migrationBuilder.AddColumn<int>(
                name: "CurrentCount",
                table: "Grocery",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DueCount",
                table: "Grocery",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DueList",
                table: "Grocery",
                nullable: true);
        }
    }
}
