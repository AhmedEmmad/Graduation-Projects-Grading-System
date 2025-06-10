using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GradingManagementSystem.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class EditedAllDateTimeAndAddedPropertiesToNotificationTableAndEditInItsLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Schedules",
                newName: "IsGraded");

            migrationBuilder.RenameColumn(
                name: "IsRead",
                table: "Notifications",
                newName: "IsReadFromStudent");

            migrationBuilder.AddColumn<bool>(
                name: "IsReadFromAdmin",
                table: "Notifications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsReadFromDoctor",
                table: "Notifications",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsReadFromAdmin",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "IsReadFromDoctor",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "IsGraded",
                table: "Schedules",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "IsReadFromStudent",
                table: "Notifications",
                newName: "IsRead");
        }
    }
}
