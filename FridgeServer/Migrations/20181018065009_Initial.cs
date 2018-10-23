using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FridgeServer.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    firstname = table.Column<string>(nullable: true),
                    lastname = table.Column<string>(nullable: true),
                    username = table.Column<string>(nullable: true),
                    passwordHash = table.Column<byte[]>(nullable: true),
                    passwordSalt = table.Column<byte[]>(nullable: true),
                    secretId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "UserFriend",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    friendUsername = table.Column<string>(nullable: true),
                    friendUserId = table.Column<int>(nullable: false),
                    friendEncryptedCode = table.Column<string>(nullable: true),
                    AreFriends = table.Column<bool>(nullable: false),
                    Userid = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFriend", x => x.id);
                    table.ForeignKey(
                        name: "FK_UserFriend_Users_Userid",
                        column: x => x.Userid,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "userGroceries",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(nullable: false),
                    basic = table.Column<bool>(nullable: false),
                    timeout = table.Column<long>(nullable: true),
                    groceryOrBought = table.Column<bool>(nullable: false),
                    owner = table.Column<int>(nullable: false),
                    Userid = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userGroceries", x => x.id);
                    table.ForeignKey(
                        name: "FK_userGroceries_Users_Userid",
                        column: x => x.Userid,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "moreInformations",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    date = table.Column<long>(nullable: true),
                    bought = table.Column<bool>(nullable: false),
                    lifeTime = table.Column<long>(nullable: true),
                    no = table.Column<int>(nullable: true),
                    typeOfNo = table.Column<string>(nullable: true),
                    Groceryid = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_moreInformations", x => x.id);
                    table.ForeignKey(
                        name: "FK_moreInformations_userGroceries_Groceryid",
                        column: x => x.Groceryid,
                        principalTable: "userGroceries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_moreInformations_Groceryid",
                table: "moreInformations",
                column: "Groceryid");

            migrationBuilder.CreateIndex(
                name: "IX_UserFriend_Userid",
                table: "UserFriend",
                column: "Userid");

            migrationBuilder.CreateIndex(
                name: "IX_userGroceries_Userid",
                table: "userGroceries",
                column: "Userid");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "moreInformations");

            migrationBuilder.DropTable(
                name: "UserFriend");

            migrationBuilder.DropTable(
                name: "userGroceries");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
