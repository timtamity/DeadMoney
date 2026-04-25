using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeadMoney.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDraftSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Create DraftProspects (referenced by DraftPicks)
            migrationBuilder.CreateTable(
                name: "DraftProspects",
                schema: "League",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName    = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName     = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PositionId   = table.Column<int>(type: "int", nullable: false),
                    Year         = table.Column<int>(type: "int", nullable: false),
                    College      = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Height       = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Weight       = table.Column<int>(type: "int", nullable: true),
                    Age          = table.Column<int>(type: "int", nullable: true),
                    IsDrafted    = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftProspects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DraftProspects_Positions_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "League",
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DraftProspects_PositionId",
                schema: "League",
                table: "DraftProspects",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_DraftProspects_Year",
                schema: "League",
                table: "DraftProspects",
                column: "Year");

            // 2. Add new columns to DraftPicks
            migrationBuilder.AddColumn<int>(
                name: "DraftProspectId",
                schema: "League",
                table: "DraftPicks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SelectedAt",
                schema: "League",
                table: "DraftPicks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DraftPicks_DraftProspectId",
                schema: "League",
                table: "DraftPicks",
                column: "DraftProspectId");

            migrationBuilder.AddForeignKey(
                name: "FK_DraftPicks_DraftProspects_DraftProspectId",
                schema: "League",
                table: "DraftPicks",
                column: "DraftProspectId",
                principalSchema: "League",
                principalTable: "DraftProspects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // 3. Create DraftSessions (references DraftPicks)
            migrationBuilder.CreateTable(
                name: "DraftSessions",
                schema: "League",
                columns: table => new
                {
                    Id            = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year          = table.Column<int>(type: "int", nullable: false),
                    Status        = table.Column<int>(type: "int", nullable: false),
                    CurrentPickId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DraftSessions_DraftPicks_CurrentPickId",
                        column: x => x.CurrentPickId,
                        principalSchema: "League",
                        principalTable: "DraftPicks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DraftSessions_CurrentPickId",
                schema: "League",
                table: "DraftSessions",
                column: "CurrentPickId");

            // 4. Create DraftRoundClocks
            migrationBuilder.CreateTable(
                name: "DraftRoundClocks",
                schema: "League",
                columns: table => new
                {
                    Id              = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DraftSessionId  = table.Column<int>(type: "int", nullable: false),
                    Round           = table.Column<int>(type: "int", nullable: false),
                    SecondsPerPick  = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftRoundClocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DraftRoundClocks_DraftSessions_DraftSessionId",
                        column: x => x.DraftSessionId,
                        principalSchema: "League",
                        principalTable: "DraftSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DraftRoundClocks_DraftSessionId_Round",
                schema: "League",
                table: "DraftRoundClocks",
                columns: new[] { "DraftSessionId", "Round" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "DraftRoundClocks", schema: "League");
            migrationBuilder.DropTable(name: "DraftSessions", schema: "League");

            migrationBuilder.DropForeignKey(name: "FK_DraftPicks_DraftProspects_DraftProspectId", schema: "League", table: "DraftPicks");
            migrationBuilder.DropIndex(name: "IX_DraftPicks_DraftProspectId", schema: "League", table: "DraftPicks");
            migrationBuilder.DropColumn(name: "DraftProspectId", schema: "League", table: "DraftPicks");
            migrationBuilder.DropColumn(name: "SelectedAt",      schema: "League", table: "DraftPicks");

            migrationBuilder.DropTable(name: "DraftProspects", schema: "League");
        }
    }
}
