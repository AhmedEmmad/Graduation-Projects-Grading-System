namespace GradingManagementSystem.Core.DTOs
{
    public class FinalProjectIdeaDto
    {
        public int ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public string? ProjectDescription { get; set; }
        public int? SupervisorId { get; set; }
        public string? SupervisorName { get; set; }
        public int? TeamId { get; set; }
        public string? TeamName { get; set; }
        public string? PostedBy { get; set; }
    }
}
