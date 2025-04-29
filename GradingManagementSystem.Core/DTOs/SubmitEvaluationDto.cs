namespace GradingManagementSystem.Core.DTOs
{
    public class SubmitEvaluationDto
    {
        public int ScheduleId { get; set; }
        public int TeamId { get; set; }
        public List<int> StudentIds { get; set; }
        public List<GradeItemDto> Grades { get; set; }
    }
}
