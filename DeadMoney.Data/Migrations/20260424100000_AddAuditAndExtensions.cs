using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeadMoney.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditAndExtensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Audit columns on Transactions
            migrationBuilder.AddColumn<int>(
                name: "PerformedByUserId",
                schema: "League",
                table: "Transactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PerformedByUserName",
                schema: "League",
                table: "Transactions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            // ExtensionOffers table
            migrationBuilder.CreateTable(
                name: "ExtensionOffers",
                schema: "League",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    OfferedByUserId = table.Column<int>(type: "int", nullable: true),
                    OfferedByUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Years = table.Column<int>(type: "int", nullable: false),
                    TotalValueM = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AnnualValueM = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GuaranteedM = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SigningBonusM = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReviewedByUserId = table.Column<int>(type: "int", nullable: true),
                    ReviewedByUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CounterYears = table.Column<int>(type: "int", nullable: true),
                    CounterTotalValueM = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CounterGuaranteedM = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CounterSigningBonusM = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CounterNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CounteredAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtensionOffers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExtensionOffers_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "League",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExtensionOffers_Teams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "League",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExtensionOffers_PlayerId_Status",
                schema: "League",
                table: "ExtensionOffers",
                columns: new[] { "PlayerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ExtensionOffers_TeamId_PlayerId",
                schema: "League",
                table: "ExtensionOffers",
                columns: new[] { "TeamId", "PlayerId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExtensionOffers",
                schema: "League");

            migrationBuilder.DropColumn(
                name: "PerformedByUserId",
                schema: "League",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "PerformedByUserName",
                schema: "League",
                table: "Transactions");
        }
    }
}
