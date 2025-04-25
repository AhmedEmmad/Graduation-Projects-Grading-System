namespace GradingManagementSystem.Core.DTOs
{
    public class DoctorScheduleDto
    {
        public int ScheduleId { get; set; }
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public string ProjectName { get; set; }
        public string ProjectDescription { get; set; }
        public DateTime ScheduleDate { get; set; }
        public string DoctorRole { get; set; } // Supervisor Or Examiner In This Schedule
        public string PostedBy { get; set; } // Doctor Or Team Idea
        public string SupervisorName { get; set; } // Supervisor Of This Team
        public string SupervisorProfilePicture { get; set; }
        public List<TeamMemberDto> TeamMembers { get; set; }
        public List<ExaminerDto> Examiners { get; set; }

    }
}
