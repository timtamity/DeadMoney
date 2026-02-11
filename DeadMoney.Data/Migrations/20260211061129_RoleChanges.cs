using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DeadMoney.Data.Migrations
{
    /// <inheritdoc />
    public partial class RoleChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserRolePositions",
                schema: "Auth",
                columns: table => new
                {
                    PositionsCode = table.Column<string>(type: "nvarchar(10)", nullable: false),
                    UserRolesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRolePositions", x => new { x.PositionsCode, x.UserRolesId });
                    table.ForeignKey(
                        name: "FK_UserRolePositions_Positions_PositionsCode",
                        column: x => x.PositionsCode,
                        principalSchema: "League",
                        principalTable: "Positions",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRolePositions_UserRoles_UserRolesId",
                        column: x => x.UserRolesId,
                        principalSchema: "Auth",
                        principalTable: "UserRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "Auth",
                table: "Roles",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "", "Commissioner" },
                    { 2, "", "Agent" },
                    { 3, "", "GM" },
                    { 4, "", "Assistant GM" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserRolePositions_UserRolesId",
                schema: "Auth",
                table: "UserRolePositions",
                column: "UserRolesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserRolePositions",
                schema: "Auth");

            migrationBuilder.DeleteData(
                schema: "Auth",
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "Auth",
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "Auth",
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "Auth",
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
