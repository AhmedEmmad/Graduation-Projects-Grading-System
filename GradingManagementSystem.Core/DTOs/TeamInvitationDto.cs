namespace GradingManagementSystem.Core.DTOs
{
    public class TeamInvitationDto
    {
        public int InvitationId { get; set; }
        public int? TeamId { get; set; }
        public string? TeamName { get; set; }
        public int? LeaderId { get; set; }
        public string? LeaderName { get; set; }
        public int? StudentId { get; set; }
        public string? StudentName { get; set; }
        public DateTime InvitationSentDate { get; set; }
        public string? InvitationStatus { get; set; }
        public List<TeamMemberDto> TeamMembers { get; set; } = new ();
    }
}
