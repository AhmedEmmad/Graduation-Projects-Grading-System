namespace GradingManagementSystem.Core.DTOs
{
    public class TaskCreateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Deadline { get; set; }
        public int SupervisorId { get; set; }
        public int TeamId { get; set; }
        public List<int> StudentIds { get; set; } = new();
    }
}
