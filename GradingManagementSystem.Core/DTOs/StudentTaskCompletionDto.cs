namespace GradingManagementSystem.Core.DTOs
{
    public class StudentTaskCompletionDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentEmail { get; set; }
        public string StudentProfilePicture { get; set; }
        public int TaskId { get; set; }
        public string TaskName { get; set; }
        public DateTime CompletionDate { get; set; }
        public string TeamName { get; set; }
        public string SupervisorName { get; set; }
        public TimeSpan CompletionTime { get; set; } // Time taken to complete (Deadline - CompletionDate)
        public bool WasLate { get; set; } // Flag if completed after deadline
    }
}
