using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rovio.MatchMaking.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddSchemaConstraintsAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "SessionPlayers",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "ATTENDED",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldDefaultValue: "ATTENDED")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_EndsAt",
                table: "Sessions",
                column: "EndsAt");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_LatencyLevel",
                table: "Sessions",
                column: "LatencyLevel");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_LatencyLevel_EndsAt_JoinedCount",
                table: "Sessions",
                columns: new[] { "LatencyLevel", "EndsAt", "JoinedCount" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Sessions_JoinedCount",
                table: "Sessions",
                sql: "JoinedCount >= 0 AND JoinedCount <= 10");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Sessions_LatencyLevel",
                table: "Sessions",
                sql: "LatencyLevel >= 1 AND LatencyLevel <= 5");

            migrationBuilder.CreateIndex(
                name: "IX_SessionPlayers_PlayerId",
                table: "SessionPlayers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionPlayers_SessionId",
                table: "SessionPlayers",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionPlayers_SessionId_PlayerId",
                table: "SessionPlayers",
                columns: new[] { "SessionId", "PlayerId" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_SessionPlayers_Status",
                table: "SessionPlayers",
                sql: "Status IN ('ATTENDED', 'PLAYED', 'LEFT')");

            migrationBuilder.CreateIndex(
                name: "IX_QueuedPlayers_CreatedAt",
                table: "QueuedPlayers",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_QueuedPlayers_LatencyLevel",
                table: "QueuedPlayers",
                column: "LatencyLevel");

            migrationBuilder.CreateIndex(
                name: "IX_QueuedPlayers_PlayerId",
                table: "QueuedPlayers",
                column: "PlayerId",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_QueuedPlayers_LatencyLevel",
                table: "QueuedPlayers",
                sql: "LatencyLevel >= 1 AND LatencyLevel <= 5");

            migrationBuilder.AddForeignKey(
                name: "FK_SessionPlayers_Sessions_SessionId",
                table: "SessionPlayers",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SessionPlayers_Sessions_SessionId",
                table: "SessionPlayers");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_EndsAt",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_LatencyLevel",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_LatencyLevel_EndsAt_JoinedCount",
                table: "Sessions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Sessions_JoinedCount",
                table: "Sessions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Sessions_LatencyLevel",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_SessionPlayers_PlayerId",
                table: "SessionPlayers");

            migrationBuilder.DropIndex(
                name: "IX_SessionPlayers_SessionId",
                table: "SessionPlayers");

            migrationBuilder.DropIndex(
                name: "IX_SessionPlayers_SessionId_PlayerId",
                table: "SessionPlayers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SessionPlayers_Status",
                table: "SessionPlayers");

            migrationBuilder.DropIndex(
                name: "IX_QueuedPlayers_CreatedAt",
                table: "QueuedPlayers");

            migrationBuilder.DropIndex(
                name: "IX_QueuedPlayers_LatencyLevel",
                table: "QueuedPlayers");

            migrationBuilder.DropIndex(
                name: "IX_QueuedPlayers_PlayerId",
                table: "QueuedPlayers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_QueuedPlayers_LatencyLevel",
                table: "QueuedPlayers");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "SessionPlayers",
                type: "longtext",
                nullable: false,
                defaultValue: "ATTENDED",
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "ATTENDED")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
