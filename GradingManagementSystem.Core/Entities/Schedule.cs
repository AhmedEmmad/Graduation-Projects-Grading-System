namespace GradingManagementSystem.Core.Entities
{
    public class Schedule : BaseEntity
    {
        public int TeamId { get; set; } // Foreign Key Of Id In Team Table
        public DateTime ScheduleDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdatedAt { get; set; } = null;
        public bool IsActive { get; set; } = true; // property to soft delete/disable schedule
        public string Status { get; set; } = "Upcoming"; // "Upcoming", "InProgress", "Completed"
        public int AcademicAppointmentId { get; set; }


        #region Navigation Properties
        public Team Team { get; set; }
        public ICollection<CommitteeDoctorSchedule> CommitteeDoctorSchedules { get; set; } = new HashSet<CommitteeDoctorSchedule>();
        public ICollection<Evaluation> Evaluations { get; set; } = new HashSet<Evaluation>();
        public AcademicAppointment AcademicAppointment { get; set; }
        #endregion
    }
}