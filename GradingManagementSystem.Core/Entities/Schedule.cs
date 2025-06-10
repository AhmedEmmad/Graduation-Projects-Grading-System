namespace GradingManagementSystem.Core.Entities
{
    public class Schedule : BaseEntity
    {
        public DateTime ScheduleDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now.AddHours(1);
        public DateTime? LastUpdatedAt { get; set; } = null;
        public string Status { get; set; } = StatusType.Upcoming.ToString(); // "Upcoming", "Finished"
        public bool IsGraded { get; set; } = false;

        public int? TeamId { get; set; } // Foreign Key Of Id In Team Table
        public int? AcademicAppointmentId { get; set; } // Foreign Key Of Id In AcademicAppointment Table


        #region Navigation Properties
        public Team Team { get; set; }
        public ICollection<CommitteeDoctorSchedule> CommitteeDoctorSchedules { get; set; } = new HashSet<CommitteeDoctorSchedule>();
        public ICollection<Evaluation> Evaluations { get; set; } = new HashSet<Evaluation>();
        public AcademicAppointment AcademicAppointment { get; set; }
        public ICollection<Criteria> Criterias { get; set; } = new HashSet<Criteria>();
        public ICollection<CriteriaSchedule> CriteriaSchedules { get; set; } = new List<CriteriaSchedule>();
        #endregion
    }
}