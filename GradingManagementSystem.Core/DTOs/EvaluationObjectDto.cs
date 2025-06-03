namespace GradingManagementSystem.Core.DTOs
{
    public class EvaluationObjectDto
    {
        public int? EvaluationId { get; set; }
        public int? CriteriaId { get; set; }
        public string CriteriaName { get; set; }
        public string CriteriaDescription { get; set; }
        public double Grade { get; set; }
        public DateTime EvaluationDate { get; set; }
        public string EvaluatorRole { get; set; }
        public int? DoctorEvaluatorId { get; set; }
        public int? AdminEvaluatorId { get; set; }
        public int? TeamId { get; set; }
        public int? StudentId { get; set; }
        public int? ScheduleId { get; set; }
    }
}
