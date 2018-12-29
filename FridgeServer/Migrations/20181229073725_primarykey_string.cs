using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FridgeServer.Migrations
{
    public partial class primarykey_string : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_moreInformations_userGroceries_Groceryid",
                table: "moreInformations");

            migrationBuilder.DropIndex(
                name: "IX_moreInformations_Groceryid",
                table: "moreInformations");

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "userGroceries",
                nullable: false,
                oldClrType: typeof(int))
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "userFriends",
                nullable: false,
                oldClrType: typeof(int))
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "moreInformations",
                nullable: false,
                oldClrType: typeof(int))
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<string>(
                name: "Groceryid1",
                table: "moreInformations",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_moreInformations_Groceryid1",
                table: "moreInformations",
                column: "Groceryid1");

            migrationBuilder.AddForeignKey(
                name: "FK_moreInformations_userGroceries_Groceryid1",
                table: "moreInformations",
                column: "Groceryid1",
                principalTable: "userGroceries",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_moreInformations_userGroceries_Groceryid1",
                table: "moreInformations");

            migrationBuilder.DropIndex(
                name: "IX_moreInformations_Groceryid1",
                table: "moreInformations");

            migrationBuilder.DropColumn(
                name: "Groceryid1",
                table: "moreInformations");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "userGroceries",
                nullable: false,
                oldClrType: typeof(string))
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "userFriends",
                nullable: false,
                oldClrType: typeof(string))
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "moreInformations",
                nullable: false,
                oldClrType: typeof(string))
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.CreateIndex(
                name: "IX_moreInformations_Groceryid",
                table: "moreInformations",
                column: "Groceryid");

            migrationBuilder.AddForeignKey(
                name: "FK_moreInformations_userGroceries_Groceryid",
                table: "moreInformations",
                column: "Groceryid",
                principalTable: "userGroceries",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
