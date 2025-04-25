namespace GradingManagementSystem.Core.DTOs
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public string ProfilePicture { get; set; }
        public int? TeamId { get; set; }
        public int? LeaderOfTeamId { get; set; }
        public bool? InTeam { get; set; }
    }
}
