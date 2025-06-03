namespace GradingManagementSystem.Core.DTOs
{
    public class DoctorTeamsWithProjectsDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int TeamId { get; set; }
        public int LeaderId { get; set; }
        public string LeaderName { get; set; }
        public string TeamName { get; set; }

    }
}
