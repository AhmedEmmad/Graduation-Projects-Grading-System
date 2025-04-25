using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GradingManagementSystem.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePropertiesOfFinalProjectIdeasTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PostedBy",
                table: "FinalProjectIdeas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProjectName",
                table: "FinalProjectIdeas",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PostedBy",
                table: "FinalProjectIdeas");

            migrationBuilder.DropColumn(
                name: "ProjectName",
                table: "FinalProjectIdeas");
        }
    }
}
