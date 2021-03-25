using Microsoft.EntityFrameworkCore.Migrations;

namespace Mmcc.Bot.Database.Migrations
{
    public partial class AddTags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    TagName = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: false),
                    TagDescription = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Content = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: false),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: false),
                    LastModifiedAt = table.Column<long>(type: "bigint", nullable: true),
                    CreatedByDiscordId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    LastModifiedByDiscordId = table.Column<ulong>(type: "bigint unsigned", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => new { x.GuildId, x.TagName });
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tags_GuildId",
                table: "Tags",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_TagName",
                table: "Tags",
                column: "TagName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tags");
        }
    }
}
