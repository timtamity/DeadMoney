using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DeadMoney.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialInstance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "League");

            migrationBuilder.EnsureSchema(
                name: "Auth");

            migrationBuilder.CreateTable(
                name: "LeagueSettings",
                schema: "League",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    BaseSalaryCap = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeagueSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Positions",
                schema: "League",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "Auth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                schema: "League",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Abbreviation = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Nickname = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Conference = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Division = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PrimaryColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    SecondaryColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CarryoverCap = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "Auth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiscordId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AvatarUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Bio = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ThemePreference = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UseTeamColorsAsAccent = table.Column<bool>(type: "bit", nullable: false),
                    TimeZoneId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                schema: "League",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GsisId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OtcId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PfrId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PositionId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: true),
                    IsRetired = table.Column<bool>(type: "bit", nullable: false),
                    College = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BirthDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Age = table.Column<int>(type: "int", nullable: true),
                    Height = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Weight = table.Column<int>(type: "int", nullable: true),
                    DraftYear = table.Column<int>(type: "int", nullable: true),
                    DraftRound = table.Column<int>(type: "int", nullable: true),
                    DraftPick = table.Column<int>(type: "int", nullable: true),
                    YearsExp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Number = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HeadshotUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Players_Positions_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "League",
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Players_Teams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "League",
                        principalTable: "Teams",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                schema: "Auth",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    PositionId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId, x.PositionId, x.TeamId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Positions_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "League",
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "Auth",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Teams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "League",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "Auth",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Contracts",
                schema: "League",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    TotalValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalGuaranteed = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SigningBonus = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsModifiedBySim = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contracts_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "League",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContractYears",
                schema: "League",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: true),
                    IsVoidYear = table.Column<bool>(type: "bit", nullable: false),
                    BaseSalary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SigningBonusProration = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RosterBonus = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OptionBonusProration = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    WorkoutBonus = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OtherBonus = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PerGameRosterBonus = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GuaranteedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CapNumber = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CashPaid = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractYears", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractYears_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalSchema: "League",
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContractYears_Teams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "League",
                        principalTable: "Teams",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                schema: "League",
                table: "Positions",
                columns: new[] { "Id", "Code", "DisplayOrder", "Name", "Unit" },
                values: new object[,]
                {
                    { 1, "QB", 1, "Quarterback", "Offense" },
                    { 2, "RB", 2, "Running Back", "Offense" },
                    { 3, "FB", 3, "Fullback", "Offense" },
                    { 4, "WR", 4, "Wide Receiver", "Offense" },
                    { 5, "TE", 5, "Tight End", "Offense" },
                    { 6, "OT", 6, "Offensive Tackle", "Offense" },
                    { 7, "G", 7, "Offensive Guard", "Offense" },
                    { 8, "C", 8, "Center", "Offense" },
                    { 9, "EDGE", 10, "Edge Defender", "Defense" },
                    { 10, "DT", 11, "Interior Defensive Line", "Defense" },
                    { 11, "LB", 12, "Linebacker", "Defense" },
                    { 12, "CB", 13, "Cornerback", "Defense" },
                    { 13, "S", 14, "Safety", "Defense" },
                    { 14, "K", 20, "Kicker", "SpecialTeams" },
                    { 15, "P", 21, "Punter", "SpecialTeams" },
                    { 16, "LS", 22, "Long Snapper", "SpecialTeams" }
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

            migrationBuilder.InsertData(
                schema: "League",
                table: "Teams",
                columns: new[] { "Id", "Abbreviation", "CarryoverCap", "City", "Conference", "Division", "LogoUrl", "Name", "Nickname", "PrimaryColor", "SecondaryColor" },
                values: new object[,]
                {
                    { 1, "ARI", 0m, "Arizona", "", "", null, "", "Cardinals", null, null },
                    { 2, "ATL", 0m, "Atlanta", "", "", null, "", "Falcons", null, null },
                    { 3, "BAL", 0m, "Baltimore", "", "", null, "", "Ravens", null, null },
                    { 4, "BUF", 0m, "Buffalo", "", "", null, "", "Bills", null, null },
                    { 5, "CAR", 0m, "Carolina", "", "", null, "", "Panthers", null, null },
                    { 6, "CHI", 0m, "Chicago", "", "", null, "", "Bears", null, null },
                    { 7, "CIN", 0m, "Cincinnati", "", "", null, "", "Bengals", null, null },
                    { 8, "CLE", 0m, "Cleveland", "", "", null, "", "Browns", null, null },
                    { 9, "DAL", 0m, "Dallas", "", "", null, "", "Cowboys", null, null },
                    { 10, "DEN", 0m, "Denver", "", "", null, "", "Broncos", null, null },
                    { 11, "DET", 0m, "Detroit", "", "", null, "", "Lions", null, null },
                    { 12, "GB", 0m, "Green Bay", "", "", null, "", "Packers", null, null },
                    { 13, "HOU", 0m, "Houston", "", "", null, "", "Texans", null, null },
                    { 14, "IND", 0m, "Indianapolis", "", "", null, "", "Colts", null, null },
                    { 15, "JAX", 0m, "Jacksonville", "", "", null, "", "Jaguars", null, null },
                    { 16, "KC", 0m, "Kansas City", "", "", null, "", "Chiefs", null, null },
                    { 17, "LV", 0m, "Las Vegas", "", "", null, "", "Raiders", null, null },
                    { 18, "LAC", 0m, "Los Angeles", "", "", null, "", "Chargers", null, null },
                    { 19, "LAR", 0m, "Los Angeles", "", "", null, "", "Rams", null, null },
                    { 20, "MIA", 0m, "Miami", "", "", null, "", "Dolphins", null, null },
                    { 21, "MIN", 0m, "Minnesota", "", "", null, "", "Vikings", null, null },
                    { 22, "NE", 0m, "New England", "", "", null, "", "Patriots", null, null },
                    { 23, "NO", 0m, "New Orleans", "", "", null, "", "Saints", null, null },
                    { 24, "NYG", 0m, "New York", "", "", null, "", "Giants", null, null },
                    { 25, "NYJ", 0m, "New York", "", "", null, "", "Jets", null, null },
                    { 26, "PHI", 0m, "Philadelphia", "", "", null, "", "Eagles", null, null },
                    { 27, "PIT", 0m, "Pittsburgh", "", "", null, "", "Steelers", null, null },
                    { 28, "SF", 0m, "San Francisco", "", "", null, "", "49ers", null, null },
                    { 29, "SEA", 0m, "Seattle", "", "", null, "", "Seahawks", null, null },
                    { 30, "TB", 0m, "Tampa Bay", "", "", null, "", "Buccaneers", null, null },
                    { 31, "TEN", 0m, "Tennessee", "", "", null, "", "Titans", null, null },
                    { 32, "WAS", 0m, "Washington", "", "", null, "", "Commanders", null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_PlayerId",
                schema: "League",
                table: "Contracts",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractYears_ContractId",
                schema: "League",
                table: "ContractYears",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractYears_TeamId",
                schema: "League",
                table: "ContractYears",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_PositionId",
                schema: "League",
                table: "Players",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_TeamId",
                schema: "League",
                table: "Players",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_PositionId",
                schema: "Auth",
                table: "UserRoles",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                schema: "Auth",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_TeamId",
                schema: "Auth",
                table: "UserRoles",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DiscordId",
                schema: "Auth",
                table: "Users",
                column: "DiscordId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractYears",
                schema: "League");

            migrationBuilder.DropTable(
                name: "LeagueSettings",
                schema: "League");

            migrationBuilder.DropTable(
                name: "UserRoles",
                schema: "Auth");

            migrationBuilder.DropTable(
                name: "Contracts",
                schema: "League");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "Auth");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "Auth");

            migrationBuilder.DropTable(
                name: "Players",
                schema: "League");

            migrationBuilder.DropTable(
                name: "Positions",
                schema: "League");

            migrationBuilder.DropTable(
                name: "Teams",
                schema: "League");
        }
    }
}
