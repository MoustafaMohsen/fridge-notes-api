using Microsoft.EntityFrameworkCore.Migrations;

namespace FridgeServer.Migrations
{
    public partial class IdentityUser_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "userGroceries");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "userFriends");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "userGroceries",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "userFriends",
                nullable: true);
        }
    }
}
