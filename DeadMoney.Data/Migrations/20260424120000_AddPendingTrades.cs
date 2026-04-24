using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeadMoney.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingTrades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PendingTrades",
                schema: "League",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamAId = table.Column<int>(type: "int", nullable: false),
                    TeamBId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ProposedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProposedByUserId = table.Column<int>(type: "int", nullable: true),
                    ProposedByUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RespondedByUserId = table.Column<int>(type: "int", nullable: true),
                    RespondedByUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RespondedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedByUserId = table.Column<int>(type: "int", nullable: true),
                    ReviewedByUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingTrades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PendingTrades_Teams_TeamAId",
                        column: x => x.TeamAId,
                        principalSchema: "League",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PendingTrades_Teams_TeamBId",
                        column: x => x.TeamBId,
                        principalSchema: "League",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PendingTradeAssets",
                schema: "League",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PendingTradeId = table.Column<int>(type: "int", nullable: false),
                    SendingTeamId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: true),
                    DraftPickId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingTradeAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PendingTradeAssets_PendingTrades_PendingTradeId",
                        column: x => x.PendingTradeId,
                        principalSchema: "League",
                        principalTable: "PendingTrades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PendingTradeAssets_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "League",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PendingTradeAssets_DraftPicks_DraftPickId",
                        column: x => x.DraftPickId,
                        principalSchema: "League",
                        principalTable: "DraftPicks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PendingTrades_Status",
                schema: "League",
                table: "PendingTrades",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PendingTrades_TeamAId_Status",
                schema: "League",
                table: "PendingTrades",
                columns: new[] { "TeamAId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PendingTrades_TeamBId_Status",
                schema: "League",
                table: "PendingTrades",
                columns: new[] { "TeamBId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PendingTradeAssets_PendingTradeId",
                schema: "League",
                table: "PendingTradeAssets",
                column: "PendingTradeId");

            migrationBuilder.CreateIndex(
                name: "IX_PendingTradeAssets_PlayerId",
                schema: "League",
                table: "PendingTradeAssets",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PendingTradeAssets_DraftPickId",
                schema: "League",
                table: "PendingTradeAssets",
                column: "DraftPickId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PendingTradeAssets",
                schema: "League");

            migrationBuilder.DropTable(
                name: "PendingTrades",
                schema: "League");
        }
    }
}
