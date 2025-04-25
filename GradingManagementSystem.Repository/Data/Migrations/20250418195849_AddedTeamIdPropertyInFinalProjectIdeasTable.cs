using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GradingManagementSystem.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedTeamIdPropertyInFinalProjectIdeasTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "FinalProjectIdeas",
                type: "int",
                nullable: true,
                defaultValue: null);

            migrationBuilder.CreateIndex(
                name: "IX_FinalProjectIdeas_TeamId",
                table: "FinalProjectIdeas",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_FinalProjectIdeas_Teams_TeamId",
                table: "FinalProjectIdeas",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinalProjectIdeas_Teams_TeamId",
                table: "FinalProjectIdeas");

            migrationBuilder.DropIndex(
                name: "IX_FinalProjectIdeas_TeamId",
                table: "FinalProjectIdeas");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "FinalProjectIdeas");
        }
    }
}
