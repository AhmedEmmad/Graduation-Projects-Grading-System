using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GradingManagementSystem.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixedRelationshipBetweenCriteriaAndTeam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CriteriaTeam");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CriteriaTeam",
                columns: table => new
                {
                    CriteriasId = table.Column<int>(type: "int", nullable: false),
                    TeamsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CriteriaTeam", x => new { x.CriteriasId, x.TeamsId });
                    table.ForeignKey(
                        name: "FK_CriteriaTeam_Criterias_CriteriasId",
                        column: x => x.CriteriasId,
                        principalTable: "Criterias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CriteriaTeam_Teams_TeamsId",
                        column: x => x.TeamsId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CriteriaTeam_TeamsId",
                table: "CriteriaTeam",
                column: "TeamsId");
        }
    }
}
