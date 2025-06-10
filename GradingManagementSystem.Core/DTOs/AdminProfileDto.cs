namespace GradingManagementSystem.Core.DTOs
{
    public class AdminProfileDto
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string? Role { get; set; }
        public string? ProfilePicture { get; set; }
    }
}
