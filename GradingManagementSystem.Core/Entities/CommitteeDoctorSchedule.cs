namespace GradingManagementSystem.Core.Entities
{
    public class CommitteeDoctorSchedule : BaseEntity
    {
        public int? ScheduleId { get; set; } // Foreign Key Of Id In Schedule Table
        public int? DoctorId { get; set; } // Foreign Key Of Id In Doctor Table

        public string? DoctorRole { get; set; } // "Supervisor" or "Examiner"
        public bool HasCompletedEvaluation { get; set; } = false;


        #region Navigation Properties
        public Schedule Schedule { get; set; }
        public Doctor Doctor { get; set; }
        #endregion
    }
}