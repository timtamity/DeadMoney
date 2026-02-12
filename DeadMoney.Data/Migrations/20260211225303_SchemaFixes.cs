using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeadMoney.Data.Migrations
{
    /// <inheritdoc />
    public partial class SchemaFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Players_PlayerId",
                schema: "League",
                table: "Contracts");

            migrationBuilder.RenameColumn(
                name: "MiscBonuses",
                schema: "League",
                table: "ContractYears",
                newName: "PerGameRosterBonus");

            migrationBuilder.AddColumn<decimal>(
                name: "CapNumber",
                schema: "League",
                table: "ContractYears",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CashPaid",
                schema: "League",
                table: "ContractYears",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OtherBonus",
                schema: "League",
                table: "ContractYears",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalGuaranteed",
                schema: "League",
                table: "Contracts",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalValue",
                schema: "League",
                table: "Contracts",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Players_PlayerId",
                schema: "League",
                table: "Contracts",
                column: "PlayerId",
                principalSchema: "League",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Players_PlayerId",
                schema: "League",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "CapNumber",
                schema: "League",
                table: "ContractYears");

            migrationBuilder.DropColumn(
                name: "CashPaid",
                schema: "League",
                table: "ContractYears");

            migrationBuilder.DropColumn(
                name: "OtherBonus",
                schema: "League",
                table: "ContractYears");

            migrationBuilder.DropColumn(
                name: "TotalGuaranteed",
                schema: "League",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "TotalValue",
                schema: "League",
                table: "Contracts");

            migrationBuilder.RenameColumn(
                name: "PerGameRosterBonus",
                schema: "League",
                table: "ContractYears",
                newName: "MiscBonuses");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Players_PlayerId",
                schema: "League",
                table: "Contracts",
                column: "PlayerId",
                principalSchema: "League",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
