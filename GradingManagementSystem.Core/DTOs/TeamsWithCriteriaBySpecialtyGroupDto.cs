namespace GradingManagementSystem.Core.DTOs
{
    public class TeamsWithCriteriaBySpecialtyGroupDto
    {
        public string? Specialty { get; set; }
        public List<CriteriaDto> Criterias { get; set; }
        public List<TeamWithCriteriaDto> Teams { get; set; }
    }
}
