namespace GradingManagementSystem.Core.DTOs
{
    public class TeamWithMembersDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public bool HasProject { get; set; }
        public int? LeaderId { get; set; }
        public int? SupervisorId { get; set; }
        public string? Specialty { get; set; }   
        public List<TeamMemberDto> Members { get; set; } = new();
    }
}
