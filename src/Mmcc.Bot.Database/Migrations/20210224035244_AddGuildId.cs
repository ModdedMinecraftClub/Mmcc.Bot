using Microsoft.EntityFrameworkCore.Migrations;

namespace Mmcc.Bot.Database.Migrations
{
    public partial class AddGuildId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "GuildId",
                table: "ModerationActions",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "ModerationActions");
        }
    }
}
