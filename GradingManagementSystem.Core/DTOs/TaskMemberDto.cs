namespace GradingManagementSystem.Core.DTOs
{
    public class TaskMemberDto
    {
        public int TaskId { get; set; }
        public string? TaskName { get; set; }
        public int StudentId { get; set; }
        public string? StudentName { get; set; }
        public string? StudentProfilePicture { get; set; }
        public int TeamId { get; set; }
        public string? TeamName { get; set; }
        public string Status { get; set; }
    }
}
