using GradingManagementSystem.Core.Entities;

namespace GradingManagementSystem.Core.DTOs
{
    public class TeamDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool HasProject { get; set; }
        public int LeaderId { get; set; }
        public string LeaderName { get; set; }
        public int? SupervisorId { get; set; }
        public string SupervisorName { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string ProjectDescription { get; set; }
        public ICollection<Student> TeamMembers { get; set; } = new HashSet<Student>();
    }
}
