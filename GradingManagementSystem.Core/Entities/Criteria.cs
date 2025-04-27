namespace GradingManagementSystem.Core.Entities
{
    public class Criteria : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int MaxGrade { get; set; }
        public string Evaluator { get; set; } // "Admin" Or "Supervisor" Or "Examiner"
        public string GivenTo { get; set; } // "Student" Or "Team"
        public string Specialty { get; set; }
        public string Year { get; set; }
        public string Term { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastUpdatedAt { get; set; } = null;

        public int? TeamId { get; set; } = null; // Foreign Key Of Id In Team Table
        public int AcademicAppointmentId { get; set; }


        #region Navigation Properties
        public Team Team { get; set; }
        public ICollection<Schedule> Schedules { get; set; } = new HashSet<Schedule>();
        public ICollection<Evaluation> Evaluations { get; set; } = new HashSet<Evaluation>();
        public ICollection<ScheduleCriteria> ScheduleCriterias { get; set; } = new HashSet<ScheduleCriteria>();
        public AcademicAppointment AcademicAppointment { get; set; }
        #endregion
    }
}
