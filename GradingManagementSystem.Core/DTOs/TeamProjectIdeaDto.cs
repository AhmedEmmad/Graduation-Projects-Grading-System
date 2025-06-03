namespace GradingManagementSystem.Core.DTOs
{
    public class TeamProjectIdeaDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string? Status { get; set; }
        public int? TeamId { get; set; }
        public string? TeamName { get; set; }
        public int? LeaderId { get; set; }
        public string? LeaderName { get; set; }
        public int? SupervisorId { get; set; }
        public string? SupervisorName { get; set; }
    }
}
