namespace GradingManagementSystem.Core.Entities
{
    public class Evaluation : BaseEntity
    {
        public int? ScheduleId { get; set; } // Foreign Key Of Id In Schedule Table
        public int? CriteriaId { get; set; } // Foreign Key Of Id In Criteria Table
        public int? DoctorEvaluatorId { get; set; } // DoctorId // Foreign Key Of Id In Doctor Table
        public int? AdminEvaluatorId { get; set; } // AdminId // Foreign Key Of Id In Admin Table
        public string EvaluatorRole { get; set; } // "Admin", "Supervisor" or "Examiner"
        public int? StudentId { get; set; } // Foreign Key Of Id In Student Table
        public int? TeamId { get; set; } // Foreign Key Of Id In Team Table
        public int? AcademicAppointmentId { get; set; } // Foreign Key Of Id In AcademicAppointment Table
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
        public AcademicAppointment AcademicAppointment { get; set; }
        #endregion
    }
}
