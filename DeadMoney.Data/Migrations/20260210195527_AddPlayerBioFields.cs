using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeadMoney.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerBioFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Age",
                schema: "League",
                table: "Players",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "College",
                schema: "League",
                table: "Players",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Height",
                schema: "League",
                table: "Players",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Number",
                schema: "League",
                table: "Players",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Weight",
                schema: "League",
                table: "Players",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "YearsExp",
                schema: "League",
                table: "Players",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Age",
                schema: "League",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "College",
                schema: "League",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Height",
                schema: "League",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Number",
                schema: "League",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Weight",
                schema: "League",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "YearsExp",
                schema: "League",
                table: "Players");
        }
    }
}
