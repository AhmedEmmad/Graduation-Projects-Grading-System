namespace GradingManagementSystem.Core.DTOs
{
    public class TeamUnderSpecialtyForEvaluationDto
    {
        public string? Specialty { get; set; }
        public List<TeamWithCriteriaDto> Teams { get; set; } = new List<TeamWithCriteriaDto>();
    }
}
