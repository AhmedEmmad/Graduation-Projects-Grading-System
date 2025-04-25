using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GradingManagementSystem.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedTeamIdPropertyInFinalProjectIdeasTableI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinalProjectIdeas_Teams_TeamId",
                table: "FinalProjectIdeas");

            migrationBuilder.AlterColumn<int>(
                name: "TeamId",
                table: "FinalProjectIdeas",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_FinalProjectIdeas_Teams_TeamId",
                table: "FinalProjectIdeas",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinalProjectIdeas_Teams_TeamId",
                table: "FinalProjectIdeas");

            migrationBuilder.AlterColumn<int>(
                name: "TeamId",
                table: "FinalProjectIdeas",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FinalProjectIdeas_Teams_TeamId",
                table: "FinalProjectIdeas",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
