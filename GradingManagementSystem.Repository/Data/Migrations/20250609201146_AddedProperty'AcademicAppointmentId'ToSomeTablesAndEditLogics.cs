using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GradingManagementSystem.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedPropertyAcademicAppointmentIdToSomeTablesAndEditLogics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AcademicAppointmentId",
                table: "TeamsRequestDoctorProjectIdeas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AcademicAppointmentId",
                table: "Teams",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AcademicAppointmentId",
                table: "TeamProjectIdeas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AcademicAppointmentId",
                table: "Tasks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AcademicAppointmentId",
                table: "Students",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "AcademicAppointmentId",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AcademicAppointmentId",
                table: "Invitations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AcademicAppointmentId",
                table: "FinalProjectIdeas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AcademicAppointmentId",
                table: "Evaluations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AcademicAppointmentId",
                table: "DoctorProjectIdeas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamsRequestDoctorProjectIdeas_AcademicAppointmentId",
                table: "TeamsRequestDoctorProjectIdeas",
                column: "AcademicAppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_AcademicAppointmentId",
                table: "Teams",
                column: "AcademicAppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamProjectIdeas_AcademicAppointmentId",
                table: "TeamProjectIdeas",
                column: "AcademicAppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_AcademicAppointmentId",
                table: "Tasks",
                column: "AcademicAppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_AcademicAppointmentId",
                table: "Students",
                column: "AcademicAppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_AcademicAppointmentId",
                table: "Notifications",
                column: "AcademicAppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_AcademicAppointmentId",
                table: "Invitations",
                column: "AcademicAppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_FinalProjectIdeas_AcademicAppointmentId",
                table: "FinalProjectIdeas",
                column: "AcademicAppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_AcademicAppointmentId",
                table: "Evaluations",
                column: "AcademicAppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorProjectIdeas_AcademicAppointmentId",
                table: "DoctorProjectIdeas",
                column: "AcademicAppointmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorProjectIdeas_AcademicAppointments_AcademicAppointmentId",
                table: "DoctorProjectIdeas",
                column: "AcademicAppointmentId",
                principalTable: "AcademicAppointments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Evaluations_AcademicAppointments_AcademicAppointmentId",
                table: "Evaluations",
                column: "AcademicAppointmentId",
                principalTable: "AcademicAppointments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FinalProjectIdeas_AcademicAppointments_AcademicAppointmentId",
                table: "FinalProjectIdeas",
                column: "AcademicAppointmentId",
                principalTable: "AcademicAppointments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Invitations_AcademicAppointments_AcademicAppointmentId",
                table: "Invitations",
                column: "AcademicAppointmentId",
                principalTable: "AcademicAppointments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AcademicAppointments_AcademicAppointmentId",
                table: "Notifications",
                column: "AcademicAppointmentId",
                principalTable: "AcademicAppointments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_AcademicAppointments_AcademicAppointmentId",
                table: "Students",
                column: "AcademicAppointmentId",
                principalTable: "AcademicAppointments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_AcademicAppointments_AcademicAppointmentId",
                table: "Tasks",
                column: "AcademicAppointmentId",
                principalTable: "AcademicAppointments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamProjectIdeas_AcademicAppointments_AcademicAppointmentId",
                table: "TeamProjectIdeas",
                column: "AcademicAppointmentId",
                principalTable: "AcademicAppointments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_AcademicAppointments_AcademicAppointmentId",
                table: "Teams",
                column: "AcademicAppointmentId",
                principalTable: "AcademicAppointments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamsRequestDoctorProjectIdeas_AcademicAppointments_AcademicAppointmentId",
                table: "TeamsRequestDoctorProjectIdeas",
                column: "AcademicAppointmentId",
                principalTable: "AcademicAppointments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorProjectIdeas_AcademicAppointments_AcademicAppointmentId",
                table: "DoctorProjectIdeas");

            migrationBuilder.DropForeignKey(
                name: "FK_Evaluations_AcademicAppointments_AcademicAppointmentId",
                table: "Evaluations");

            migrationBuilder.DropForeignKey(
                name: "FK_FinalProjectIdeas_AcademicAppointments_AcademicAppointmentId",
                table: "FinalProjectIdeas");

            migrationBuilder.DropForeignKey(
                name: "FK_Invitations_AcademicAppointments_AcademicAppointmentId",
                table: "Invitations");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AcademicAppointments_AcademicAppointmentId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_AcademicAppointments_AcademicAppointmentId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_AcademicAppointments_AcademicAppointmentId",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamProjectIdeas_AcademicAppointments_AcademicAppointmentId",
                table: "TeamProjectIdeas");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_AcademicAppointments_AcademicAppointmentId",
                table: "Teams");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamsRequestDoctorProjectIdeas_AcademicAppointments_AcademicAppointmentId",
                table: "TeamsRequestDoctorProjectIdeas");

            migrationBuilder.DropIndex(
                name: "IX_TeamsRequestDoctorProjectIdeas_AcademicAppointmentId",
                table: "TeamsRequestDoctorProjectIdeas");

            migrationBuilder.DropIndex(
                name: "IX_Teams_AcademicAppointmentId",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_TeamProjectIdeas_AcademicAppointmentId",
                table: "TeamProjectIdeas");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_AcademicAppointmentId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Students_AcademicAppointmentId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_AcademicAppointmentId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Invitations_AcademicAppointmentId",
                table: "Invitations");

            migrationBuilder.DropIndex(
                name: "IX_FinalProjectIdeas_AcademicAppointmentId",
                table: "FinalProjectIdeas");

            migrationBuilder.DropIndex(
                name: "IX_Evaluations_AcademicAppointmentId",
                table: "Evaluations");

            migrationBuilder.DropIndex(
                name: "IX_DoctorProjectIdeas_AcademicAppointmentId",
                table: "DoctorProjectIdeas");

            migrationBuilder.DropColumn(
                name: "AcademicAppointmentId",
                table: "TeamsRequestDoctorProjectIdeas");

            migrationBuilder.DropColumn(
                name: "AcademicAppointmentId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "AcademicAppointmentId",
                table: "TeamProjectIdeas");

            migrationBuilder.DropColumn(
                name: "AcademicAppointmentId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "AcademicAppointmentId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "AcademicAppointmentId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "AcademicAppointmentId",
                table: "Invitations");

            migrationBuilder.DropColumn(
                name: "AcademicAppointmentId",
                table: "FinalProjectIdeas");

            migrationBuilder.DropColumn(
                name: "AcademicAppointmentId",
                table: "Evaluations");

            migrationBuilder.DropColumn(
                name: "AcademicAppointmentId",
                table: "DoctorProjectIdeas");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Notifications",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Title",
                table: "Notifications",
                column: "Title",
                unique: true);
        }
    }
}
