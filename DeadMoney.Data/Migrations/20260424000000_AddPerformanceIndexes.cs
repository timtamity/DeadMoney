using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeadMoney.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Player.IsRetired — FA list filters retired players
            migrationBuilder.CreateIndex(
                name: "IX_Players_IsRetired",
                schema: "League",
                table: "Players",
                column: "IsRetired");

            // ContractYear(TeamId, Year) composite — cap hit aggregation on roster/cap pages
            migrationBuilder.CreateIndex(
                name: "IX_ContractYears_TeamId_Year",
                schema: "League",
                table: "ContractYears",
                columns: new[] { "TeamId", "Year" });

            // Transaction.OccurredAt — feed ordering (descending pattern)
            migrationBuilder.CreateIndex(
                name: "IX_Transactions_OccurredAt",
                schema: "League",
                table: "Transactions",
                column: "OccurredAt");

            // FaOffer(PlayerId, Status) composite — heat batch query per player
            migrationBuilder.CreateIndex(
                name: "IX_FaOffers_PlayerId_Status",
                schema: "League",
                table: "FaOffers",
                columns: new[] { "PlayerId", "Status" });

            // FaOffer(TeamId, PlayerId) composite — GM's own offer lookup
            migrationBuilder.CreateIndex(
                name: "IX_FaOffers_TeamId_PlayerId",
                schema: "League",
                table: "FaOffers",
                columns: new[] { "TeamId", "PlayerId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Players_IsRetired",         schema: "League", table: "Players");
            migrationBuilder.DropIndex(name: "IX_ContractYears_TeamId_Year", schema: "League", table: "ContractYears");
            migrationBuilder.DropIndex(name: "IX_Transactions_OccurredAt",   schema: "League", table: "Transactions");
            migrationBuilder.DropIndex(name: "IX_FaOffers_PlayerId_Status",  schema: "League", table: "FaOffers");
            migrationBuilder.DropIndex(name: "IX_FaOffers_TeamId_PlayerId",  schema: "League", table: "FaOffers");
        }
    }
}
