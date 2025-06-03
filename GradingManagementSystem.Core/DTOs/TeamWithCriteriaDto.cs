namespace GradingManagementSystem.Core.DTOs
{
    public class TeamWithCriteriaDto
    {
        public int TeamId { get; set; }
        public string? TeamName { get; set; }
        public int ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public string? ProjectDescription { get; set; }
        public string? Specialty { get; set; }
        public int? ScheduleId { get; set; }
        public DateTime? ScheduleDate { get; set; }
        public string? ScheduleStatus { get; set; } // "Upcoming", "InProgress", "Completed"
        public List<CriteriaDto> Criterias { get; set; } = new List<CriteriaDto>();
        public List<TeamMemberDto> TeamMembers { get; set; } = new List<TeamMemberDto>();
    }
}
