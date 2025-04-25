using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GradingManagementSystem.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedSupervisorIdInFinalProjectIdeaTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SupervisorId",
                table: "FinalProjectIdeas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinalProjectIdeas_SupervisorId",
                table: "FinalProjectIdeas",
                column: "SupervisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_FinalProjectIdeas_Doctors_SupervisorId",
                table: "FinalProjectIdeas",
                column: "SupervisorId",
                principalTable: "Doctors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinalProjectIdeas_Doctors_SupervisorId",
                table: "FinalProjectIdeas");

            migrationBuilder.DropIndex(
                name: "IX_FinalProjectIdeas_SupervisorId",
                table: "FinalProjectIdeas");

            migrationBuilder.DropColumn(
                name: "SupervisorId",
                table: "FinalProjectIdeas");
        }
    }
}
