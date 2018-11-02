using Microsoft.EntityFrameworkCore.Migrations;

namespace FridgeServer.Migrations
{
    public partial class userGrocery_3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "Users",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "excludeids",
                table: "userGroceries",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "email",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "excludeids",
                table: "userGroceries");
        }
    }
}
