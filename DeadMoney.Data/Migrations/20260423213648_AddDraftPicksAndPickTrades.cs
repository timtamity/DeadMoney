using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeadMoney.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDraftPicksAndPickTrades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "PlayerId",
                schema: "League",
                table: "Transactions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "DraftPickId",
                schema: "League",
                table: "Transactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DraftPicks",
                schema: "League",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Round = table.Column<int>(type: "int", nullable: false),
                    PickNumber = table.Column<int>(type: "int", nullable: true),
                    OriginalTeamId = table.Column<int>(type: "int", nullable: false),
                    CurrentTeamId = table.Column<int>(type: "int", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftPicks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DraftPicks_Teams_CurrentTeamId",
                        column: x => x.CurrentTeamId,
                        principalSchema: "League",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DraftPicks_Teams_OriginalTeamId",
                        column: x => x.OriginalTeamId,
                        principalSchema: "League",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_DraftPickId",
                schema: "League",
                table: "Transactions",
                column: "DraftPickId");

            migrationBuilder.CreateIndex(
                name: "IX_DraftPicks_CurrentTeamId",
                schema: "League",
                table: "DraftPicks",
                column: "CurrentTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_DraftPicks_OriginalTeamId",
                schema: "League",
                table: "DraftPicks",
                column: "OriginalTeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_DraftPicks_DraftPickId",
                schema: "League",
                table: "Transactions",
                column: "DraftPickId",
                principalSchema: "League",
                principalTable: "DraftPicks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_DraftPicks_DraftPickId",
                schema: "League",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "DraftPicks",
                schema: "League");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_DraftPickId",
                schema: "League",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "DraftPickId",
                schema: "League",
                table: "Transactions");

            migrationBuilder.AlterColumn<int>(
                name: "PlayerId",
                schema: "League",
                table: "Transactions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
