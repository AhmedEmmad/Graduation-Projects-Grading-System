namespace GradingManagementSystem.Core.DTOs
{
    public class CriteriaDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int MaxGrade { get; set; }
        public string Evaluator { get; set; } // "Admin" Or "Supervisor" Or "Examiner"
        public string GivenTo { get; set; } // "Student" Or "Team"
        public string Specialty { get; set; }
        public string Year { get; set; }
        public string Term { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
