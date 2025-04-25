namespace GradingManagementSystem.Core.DTOs
{
    public class TeamRequestForDoctorProjectIdeaDto
    {
        public int RequestId { get; set; }
        public string Status { get; set; }
        public DateTime RequestedDate { get; set; }
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public int LeaderId { get; set; }
        public string LeaderName { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public int DoctorProjectIdeaId { get; set; }
        public string DoctorProjectIdeaName { get; set; }
        public List<TeamMemberDto> TeamMembers { get; set; } = new();
    }
}
