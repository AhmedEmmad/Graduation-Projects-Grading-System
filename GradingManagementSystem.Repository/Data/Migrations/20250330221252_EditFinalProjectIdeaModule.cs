using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GradingManagementSystem.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class EditFinalProjectIdeaModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "FinalProjectIdeas");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "FinalProjectIdeas");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "FinalProjectIdeas");

            migrationBuilder.DropColumn(
                name: "SupervisorName",
                table: "FinalProjectIdeas");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "FinalProjectIdeas");

            migrationBuilder.DropColumn(
                name: "TeamLeaderId",
                table: "FinalProjectIdeas");

            migrationBuilder.DropColumn(
                name: "TeamLeaderName",
                table: "FinalProjectIdeas");

            migrationBuilder.DropColumn(
                name: "TeamName",
                table: "FinalProjectIdeas");

            migrationBuilder.RenameColumn(
                name: "SupervisorId",
                table: "FinalProjectIdeas",
                newName: "TeamRequestDoctorProjectIdeaId");

            migrationBuilder.AddColumn<int>(
                name: "TeamProjectIdeaId",
                table: "FinalProjectIdeas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinalProjectIdeas_TeamProjectIdeaId",
                table: "FinalProjectIdeas",
                column: "TeamProjectIdeaId");

            migrationBuilder.CreateIndex(
                name: "IX_FinalProjectIdeas_TeamRequestDoctorProjectIdeaId",
                table: "FinalProjectIdeas",
                column: "TeamRequestDoctorProjectIdeaId");

            migrationBuilder.AddForeignKey(
                name: "FK_FinalProjectIdeas_TeamProjectIdeas_TeamProjectIdeaId",
                table: "FinalProjectIdeas",
                column: "TeamProjectIdeaId",
                principalTable: "TeamProjectIdeas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FinalProjectIdeas_TeamsRequestDoctorProjectIdeas_TeamRequestDoctorProjectIdeaId",
                table: "FinalProjectIdeas",
                column: "TeamRequestDoctorProjectIdeaId",
                principalTable: "TeamsRequestDoctorProjectIdeas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinalProjectIdeas_TeamProjectIdeas_TeamProjectIdeaId",
                table: "FinalProjectIdeas");

            migrationBuilder.DropForeignKey(
                name: "FK_FinalProjectIdeas_TeamsRequestDoctorProjectIdeas_TeamRequestDoctorProjectIdeaId",
                table: "FinalProjectIdeas");

            migrationBuilder.DropIndex(
                name: "IX_FinalProjectIdeas_TeamProjectIdeaId",
                table: "FinalProjectIdeas");

            migrationBuilder.DropIndex(
                name: "IX_FinalProjectIdeas_TeamRequestDoctorProjectIdeaId",
                table: "FinalProjectIdeas");

            migrationBuilder.DropColumn(
                name: "TeamProjectIdeaId",
                table: "FinalProjectIdeas");

            migrationBuilder.RenameColumn(
                name: "TeamRequestDoctorProjectIdeaId",
                table: "FinalProjectIdeas",
                newName: "SupervisorId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "FinalProjectIdeas",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "FinalProjectIdeas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "FinalProjectIdeas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SupervisorName",
                table: "FinalProjectIdeas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "FinalProjectIdeas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TeamLeaderId",
                table: "FinalProjectIdeas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TeamLeaderName",
                table: "FinalProjectIdeas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TeamName",
                table: "FinalProjectIdeas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
