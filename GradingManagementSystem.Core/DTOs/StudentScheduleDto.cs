namespace GradingManagementSystem.Core.DTOs
{
    public class StudentScheduleDto
    {
        public int ScheduleId { get; set; }
        public int? TeamId { get; set; }
        public string? TeamName { get; set; }
        public string? ProjectName { get; set; }
        public string? ProjectDescription { get; set; }
        public DateTime ScheduleDate { get; set; }
        public string? Status { get; set; }
        public int? SupervisorId { get; set; }
        public string? SupervisorName { get; set; }
        public string? SupervisorProfilePicture { get; set; }
        public string? PostedBy { get; set; }
        public List<TeamMemberDto> TeamMembers { get; set; }
    }
}