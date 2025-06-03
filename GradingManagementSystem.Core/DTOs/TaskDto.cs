using GradingManagementSystem.Core.Entities;

namespace GradingManagementSystem.Core.DTOs
{
    public class TaskDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Deadline { get; set; }
        public DateTime StartTime { get; set; }
        public string Status { get; set; }
        public int SupervisorId { get; set; }
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public ICollection<TaskMemberDto> TaskMembers { get; set; } = new HashSet<TaskMemberDto>();
    }
}
