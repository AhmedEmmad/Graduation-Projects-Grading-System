namespace GradingManagementSystem.Core.DTOs
{
    public class CriteriaObjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int MaxGrade { get; set; }
        public string Evaluator { get; set; }
        public string GivenTo { get; set; }
        public string Specialty { get; set; }
        public string Year { get; set; }
        public string Term { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
    }
}