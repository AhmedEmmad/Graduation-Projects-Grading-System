namespace GradingManagementSystem.Core.DTOs
{
    public class EvaluationResultDto
    {
        public int CriteriaId { get; set; }
        public string CriteriaName { get; set; }
        public double Grade { get; set; }
        public int MaxGrade { get; set; }
    }
}
