namespace GradingManagementSystem.Core.DTOs
{
    public class StudentProfileDto
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? ProfilePicture { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public int? TeamId { get; set; }
        public bool HasProject { get; set; }
        public int? LeaderOfTeamId { get; set; }
        public bool InTeam { get; set; }
        public string? Specialty { get; set; }
        public string? CurrentAcademicYear { get; set; }
        public string? CurrentAcademicTerm { get; set; }
    }

}
