namespace GradingManagementSystem.Core.DTOs
{
    public class ExistingTeamEvaluationsDto
    {
        public int TeamId { get; set; }
        public int ScheduleId { get; set; }
        public int? DoctorId { get; set; } = null;
    }
}
