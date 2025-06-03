namespace GradingManagementSystem.Core.DTOs
{
    public class SubmitEvaluationDto
    {
        public int? ScheduleId { get; set; }
        public int? TeamId { get; set; }
        public int? StudentId { get; set; }
        public List<GradeItemDto> Grades { get; set; }
    }
}
