using Microsoft.EntityFrameworkCore.Migrations;

namespace FridgeServer.Migrations
{
    public partial class userGrocery_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFriend_Users_Userid",
                table: "UserFriend");

            migrationBuilder.DropForeignKey(
                name: "FK_userGroceries_Users_Userid",
                table: "userGroceries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserFriend",
                table: "UserFriend");

            migrationBuilder.RenameTable(
                name: "UserFriend",
                newName: "userFriends");

            migrationBuilder.RenameIndex(
                name: "IX_UserFriend_Userid",
                table: "userFriends",
                newName: "IX_userFriends_Userid");

            migrationBuilder.AlterColumn<string>(
                name: "owner",
                table: "userGroceries",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "Userid",
                table: "userGroceries",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Userid",
                table: "userFriends",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_userFriends",
                table: "userFriends",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_userFriends_Users_Userid",
                table: "userFriends",
                column: "Userid",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_userGroceries_Users_Userid",
                table: "userGroceries",
                column: "Userid",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_userFriends_Users_Userid",
                table: "userFriends");

            migrationBuilder.DropForeignKey(
                name: "FK_userGroceries_Users_Userid",
                table: "userGroceries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_userFriends",
                table: "userFriends");

            migrationBuilder.RenameTable(
                name: "userFriends",
                newName: "UserFriend");

            migrationBuilder.RenameIndex(
                name: "IX_userFriends_Userid",
                table: "UserFriend",
                newName: "IX_UserFriend_Userid");

            migrationBuilder.AlterColumn<int>(
                name: "owner",
                table: "userGroceries",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Userid",
                table: "userGroceries",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "Userid",
                table: "UserFriend",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserFriend",
                table: "UserFriend",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFriend_Users_Userid",
                table: "UserFriend",
                column: "Userid",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_userGroceries_Users_Userid",
                table: "userGroceries",
                column: "Userid",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
