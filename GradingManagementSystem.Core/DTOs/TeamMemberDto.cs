namespace GradingManagementSystem.Core.DTOs
{
    public class TeamMemberDto
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Specialty { get; set; }
        public bool InTeam { get; set; }
        public string? ProfilePicture { get; set; }
    }
}
