namespace GradingManagementSystem.Core.Entities
{
    public class Grade : BaseEntity
    {
        public int CriteriaId { get; set; } // Foreign Key Of Id In Criteria Table
        public int? StudentId { get; set; } // Foreign Key Of Id In Student Table
        public int? TeamId { get; set; } // Foreign Key Of Id In Team Table

        public int EvaluatorId { get; set; }
        public string Evaluator { get; set; } // Supervisor, Examiner, Admin
        public double GradeValue { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; }



        #region Navigation Properties
        public Criteria Criteria { get; set; }
        public Student Student { get; set; }
        public Team Team { get; set; }
        #endregion
    }
}
