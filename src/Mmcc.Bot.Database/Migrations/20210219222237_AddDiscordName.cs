using Microsoft.EntityFrameworkCore.Migrations;

namespace Mmcc.Bot.Database.Migrations
{
    public partial class AddDiscordName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthorDiscordName",
                table: "MemberApplications",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorDiscordName",
                table: "MemberApplications");
        }
    }
}
