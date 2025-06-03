namespace GradingManagementSystem.Core.Entities
{
    public class CommitteeDoctorSchedule : BaseEntity
    {
        public int? ScheduleId { get; set; }
        public int? DoctorId { get; set; }
        public string? DoctorRole { get; set; } // "Supervisor" or "Examiner"
        public bool HasCompletedEvaluation { get; set; } = false; // Track evaluation status


        #region Navigation Properties
        public Schedule Schedule { get; set; }
        public Doctor Doctor { get; set; }
        #endregion
    }
}