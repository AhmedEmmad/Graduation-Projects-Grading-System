namespace GradingManagementSystem.Core.DTOs
{
    public class SubmitAdminEvaluationDto
    {
        public int? TeamId { get; set; }
        public int? StudentId { get; set; }
        public List<GradeItemDto> Grades { get; set; }
    }
}
