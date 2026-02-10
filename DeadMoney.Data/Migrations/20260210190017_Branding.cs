using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeadMoney.Data.Migrations
{
    /// <inheritdoc />
    public partial class Branding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Nickname",
                schema: "League",
                table: "Teams",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "City",
                schema: "League",
                table: "Teams",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Abbreviation",
                schema: "League",
                table: "Teams",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                schema: "League",
                table: "Teams",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "League",
                table: "Teams",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PrimaryColor",
                schema: "League",
                table: "Teams",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondaryColor",
                schema: "League",
                table: "Teams",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "League",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "LogoUrl", "Name", "PrimaryColor", "SecondaryColor" },
                values: new object[] { null, "", null, null });

            migrationBuilder.UpdateData(
                schema: "League",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "LogoUrl", "Name", "PrimaryColor", "SecondaryColor" },
                values: new object[] { null, "", null, null });

            migrationBuilder.UpdateData(
                schema: "League",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "LogoUrl", "Name", "PrimaryColor", "SecondaryColor" },
                values: new object[] { null, "", null, null });

            migrationBuilder.UpdateData(
                schema: "League",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "LogoUrl", "Name", "PrimaryColor", "SecondaryColor" },
                values: new object[] { null, "", null, null });

            migrationBuilder.UpdateData(
                schema: "League",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "LogoUrl", "Name", "PrimaryColor", "SecondaryColor" },
                values: new object[] { null, "", null, null });

            migrationBuilder.UpdateData(
                schema: "League",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "LogoUrl", "Name", "PrimaryColor", "SecondaryColor" },
                values: new object[] { null, "", null, null });

            migrationBuilder.UpdateData(
                schema: "League",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "LogoUrl", "Name", "PrimaryColor", "SecondaryColor" },
                values: new object[] { null, "", null, null });

            migrationBuilder.UpdateData(
                schema: "League",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "LogoUrl", "Name", "PrimaryColor", "SecondaryColor" },
                values: new object[] { null, "", null, null });

            migrationBuilder.UpdateData(
                schema: "League",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "LogoUrl", "Name", "PrimaryColor", "SecondaryColor" },
                values: new object[] { null, "", null, null });

            migrationBuilder.UpdateData(
                schema: "League",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "LogoUrl", "Name", "PrimaryColor", "SecondaryColor" },
                values: new object[] { null, "", null, null });

            migrationBuilder.UpdateData(
                schema: "League",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "LogoUrl", "Name", "PrimaryColor", "SecondaryColor" },
                values: new object[] { null, "", null, null });

            migrationBuilder.UpdateData(
                schema: "League",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "LogoUrl", "Name", "PrimaryColor", "SecondaryColor" },
                values: new object[] { null, "", null, null });

            migrationBuilder.UpdateData(
                schema: "League",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "LogoUrl", "Name", "PrimaryColor", "SecondaryColor" },
                values: new object[] { null, "", null, null });

            migrationBuilder.UpdateData(
                schema: "League",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "LogoUrl", "Name", "PrimaryColor", "SecondaryColor" },
                values: new object[] { null, "", null, null });

            migrationBuilder.UpdateData(
                schema: "League",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "LogoUrl", "Name", "PrimaryColor", "SecondaryColor" },
                values: new object[] { null, "", null, null });

            migrationBuilder.UpdateData(
                schema: "League",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "LogoUrl", "Name", "PrimaryColor", "SecondaryColor" },
                values: new object[] { null, "", null, null });

            migrationBuilder.UpdateData(
                schema: "League",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "LogoUrl", "Name", "PrimaryColor", "SecondaryColor" },
                values: new object[] { null, "", null, null });

            migrationBuilder.UpdateData(
                schema: "League",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "LogoUrl", "Name", "PrimaryColor", "SecondaryColor" },
                values: new object[] { null, "", null, null });

            migrationBuilder.UpdateData(
                schema: "League",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "LogoUrl", "Name", "PrimaryColor", "SecondaryColor" },
                values: new object[] { null, "", null, null });

            migrationBuilder.UpdateData(
                schema: "League",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "LogoUrl", "Name", "PrimaryColor", "SecondaryColor" },
                values: new object[] { null, "", null, null });

            migrationBuilder.UpdateData(
                schema: "League",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "LogoUrl", "Name", "PrimaryColor", "SecondaryColor" },
                values: new object[] { null, "", null, null });

            migrationBuilder.UpdateData(
                schema: "League",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "LogoUrl", "Name", "PrimaryColor", "SecondaryColor" },
                values: new object[] { null, "", null, null });

            migrationBuilder.UpdateData(
                schema: "League",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "LogoUrl", "Name", "PrimaryColor", "SecondaryColor" },
                values: new object[] { null, "", null, null });

            migrationBuilder.UpdateData(
                schema: "League",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "LogoUrl", "Name", "PrimaryColor", "SecondaryColor" },
                values: new object[] { null, "", null, null });

            migrationBuilder.UpdateData(
                schema: "League",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "LogoUrl", "Name", "PrimaryColor", "SecondaryColor" },
                values: new object[] { null, "", null, null });

            migrationBuilder.UpdateData(
                schema: "League",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "LogoUrl", "Name", "PrimaryColor", "SecondaryColor" },
                values: new object[] { null, "", null, null });

            migrationBuilder.UpdateData(
                schema: "League",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "LogoUrl", "Name", "PrimaryColor", "SecondaryColor" },
                values: new object[] { null, "", null, null });

            migrationBuilder.UpdateData(
                schema: "League",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "LogoUrl", "Name", "PrimaryColor", "SecondaryColor" },
                values: new object[] { null, "", null, null });

            migrationBuilder.UpdateData(
                schema: "League",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "LogoUrl", "Name", "PrimaryColor", "SecondaryColor" },
                values: new object[] { null, "", null, null });

            migrationBuilder.UpdateData(
                schema: "League",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "LogoUrl", "Name", "PrimaryColor", "SecondaryColor" },
                values: new object[] { null, "", null, null });

            migrationBuilder.UpdateData(
                schema: "League",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "LogoUrl", "Name", "PrimaryColor", "SecondaryColor" },
                values: new object[] { null, "", null, null });

            migrationBuilder.UpdateData(
                schema: "League",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "LogoUrl", "Name", "PrimaryColor", "SecondaryColor" },
                values: new object[] { null, "", null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoUrl",
                schema: "League",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "League",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "PrimaryColor",
                schema: "League",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "SecondaryColor",
                schema: "League",
                table: "Teams");

            migrationBuilder.AlterColumn<string>(
                name: "Nickname",
                schema: "League",
                table: "Teams",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "City",
                schema: "League",
                table: "Teams",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Abbreviation",
                schema: "League",
                table: "Teams",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);
        }
    }
}
