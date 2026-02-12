using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeadMoney.Data.Migrations
{
    /// <inheritdoc />
    public partial class ContractImportAdjustments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVoidYear",
                schema: "League",
                table: "ContractYears",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                schema: "League",
                table: "ContractYears",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractYears_TeamId",
                schema: "League",
                table: "ContractYears",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContractYears_Teams_TeamId",
                schema: "League",
                table: "ContractYears",
                column: "TeamId",
                principalSchema: "League",
                principalTable: "Teams",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContractYears_Teams_TeamId",
                schema: "League",
                table: "ContractYears");

            migrationBuilder.DropIndex(
                name: "IX_ContractYears_TeamId",
                schema: "League",
                table: "ContractYears");

            migrationBuilder.DropColumn(
                name: "IsVoidYear",
                schema: "League",
                table: "ContractYears");

            migrationBuilder.DropColumn(
                name: "TeamId",
                schema: "League",
                table: "ContractYears");
        }
    }
}
