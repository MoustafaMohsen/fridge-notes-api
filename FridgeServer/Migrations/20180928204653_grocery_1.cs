using Microsoft.EntityFrameworkCore.Migrations;

namespace FridgeServer.Migrations
{
    public partial class grocery_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Grocery",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: false),
                    basic = table.Column<bool>(nullable: false),
                    Timeout = table.Column<long>(nullable: true),
                    groceryOrBought = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grocery", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MoreInformations",
                columns: table => new
                {
                    MoreInformationsId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<long>(nullable: true),
                    Bought = table.Column<bool>(nullable: false),
                    LifeTime = table.Column<long>(nullable: true),
                    No = table.Column<int>(nullable: true),
                    typeOfNo = table.Column<string>(nullable: true),
                    GroceryId = table.Column<int>(nullable: true)
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
                name: "IX_MoreInformations_GroceryId",
                table: "MoreInformations",
                column: "GroceryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MoreInformations");

            migrationBuilder.DropTable(
                name: "Grocery");
        }
    }
}
