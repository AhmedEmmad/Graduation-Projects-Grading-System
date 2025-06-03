namespace GradingManagementSystem.Core.DTOs
{
    public class TeamForSettingScheduleDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public bool HasProject { get; set; }
        public int? LeaderId { get; set; }
        public string? LeaderName { get; set; }
        public int? SupervisorId { get; set; }
        public string? SupervisorName { get; set; }
    }
}
