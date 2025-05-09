namespace GradingManagementSystem.Core.DTOs
{
    public class DoctorScheduleDto
    {
        public int? ScheduleId { get; set; }
        public string? Status { get; set; }
        public DateTime ScheduleDate { get; set; }
        public int? TeamId { get; set; }
        public string? TeamName { get; set; }
        public int? TeamLeaderId { get; set; }
        public string? TeamLeaderName { get; set; }
        public string? Specialty { get; set; }
        public int? ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public string? ProjectDescription { get; set; }
        public string? DoctorRole { get; set; } // Supervisor Or Examiner In This Schedule
        public string? PostedBy { get; set; } // Doctor Or Team Idea
        public int? SupervisorId { get; set; }
        public string? SupervisorName { get; set; } // Supervisor Of This Team
        public List<TeamMemberDto> TeamMembers { get; set; }
        public List<ExaminerDto> Examiners { get; set; }
    }
}
