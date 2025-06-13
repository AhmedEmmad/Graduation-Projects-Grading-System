namespace GradingManagementSystem.Core.DTOs
{
    public class SubmitEvaluationIIDto
    {
        public int? ScheduleId { get; set; }
        public int? TeamId { get; set; }
        public int? StudentId { get; set; }
        public int? DoctorId { get; set; }
        public List<GradeItemDto> Grades { get; set; }
    }
}
