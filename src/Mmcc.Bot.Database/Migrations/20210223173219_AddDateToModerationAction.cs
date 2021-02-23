using Microsoft.EntityFrameworkCore.Migrations;

namespace Mmcc.Bot.Database.Migrations
{
    public partial class AddDateToModerationAction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Date",
                table: "ModerationActions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "ModerationActions");
        }
    }
}
