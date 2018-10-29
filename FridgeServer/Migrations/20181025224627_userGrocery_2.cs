using Microsoft.EntityFrameworkCore.Migrations;

namespace FridgeServer.Migrations
{
    public partial class userGrocery_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "category",
                table: "userGroceries",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ownerid",
                table: "userGroceries",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "style",
                table: "userGroceries",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "category",
                table: "userGroceries");

            migrationBuilder.DropColumn(
                name: "ownerid",
                table: "userGroceries");

            migrationBuilder.DropColumn(
                name: "style",
                table: "userGroceries");
        }
    }
}
