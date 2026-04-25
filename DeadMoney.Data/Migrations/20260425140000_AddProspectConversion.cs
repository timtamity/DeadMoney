using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeadMoney.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProspectConversion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConvertedPlayerId",
                schema: "League",
                table: "DraftProspects",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DraftProspects_ConvertedPlayerId",
                schema: "League",
                table: "DraftProspects",
                column: "ConvertedPlayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_DraftProspects_Players_ConvertedPlayerId",
                schema: "League",
                table: "DraftProspects",
                column: "ConvertedPlayerId",
                principalSchema: "League",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DraftProspects_Players_ConvertedPlayerId",
                schema: "League",
                table: "DraftProspects");

            migrationBuilder.DropIndex(
                name: "IX_DraftProspects_ConvertedPlayerId",
                schema: "League",
                table: "DraftProspects");

            migrationBuilder.DropColumn(
                name: "ConvertedPlayerId",
                schema: "League",
                table: "DraftProspects");
        }
    }
}
