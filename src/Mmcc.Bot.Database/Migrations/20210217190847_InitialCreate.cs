using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Mmcc.Bot.Database.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MemberApplications",
                columns: table => new
                {
                    MemberApplicationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GuildId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ChannelId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    MessageId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    AuthorDiscordId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    AppStatus = table.Column<int>(type: "int(1)", nullable: false),
                    AppTime = table.Column<long>(type: "bigint", nullable: false),
                    MessageContent = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    ImageUrl = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberApplications", x => x.MemberApplicationId);
                });

            migrationBuilder.CreateTable(
                name: "ModerationActions",
                columns: table => new
                {
                    ModerationActionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ModerationActionType = table.Column<int>(type: "int(1)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ExpiryDate = table.Column<long>(type: "bigint", nullable: true),
                    UserDiscordId = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    UserIgn = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: true),
                    Reason = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationActions", x => x.ModerationActionId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MemberApplications_AuthorDiscordId",
                table: "MemberApplications",
                column: "AuthorDiscordId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberApplications_GuildId",
                table: "MemberApplications",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationActions_ModerationActionType",
                table: "ModerationActions",
                column: "ModerationActionType");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationActions_UserDiscordId",
                table: "ModerationActions",
                column: "UserDiscordId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationActions_UserIgn",
                table: "ModerationActions",
                column: "UserIgn");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemberApplications");

            migrationBuilder.DropTable(
                name: "ModerationActions");
        }
    }
}
