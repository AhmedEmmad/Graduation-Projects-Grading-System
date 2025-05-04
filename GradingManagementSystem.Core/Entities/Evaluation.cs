namespace GradingManagementSystem.Core.Entities
{
    public class Evaluation : BaseEntity
    {
        public int? ScheduleId { get; set; }
        public int? CriteriaId { get; set; }
        public int? DoctorEvaluatorId { get; set; } // DoctorId
        public int? AdminEvaluatorId { get; set; } // AdminId
        public string EvaluatorRole { get; set; } // "Admin", "Supervisor" or "Examiner"
        public int? StudentId { get; set; }
        public int? TeamId { get; set; }
        public double Grade { get; set; }
        public DateTime EvaluationDate { get; set; } = DateTime.Now;
        public DateTime? LastUpdatedAt { get; set; } = null;


        #region Navigation properties
        public Criteria Criteria { get; set; }
        public Schedule Schedule { get; set; }
        public Student Student { get; set; }
        public Doctor DoctorEvaluator { get; set; }
        public Admin AdminEvaluator { get; set; }
        public Team Team { get; set; }
        #endregion
    }
}
