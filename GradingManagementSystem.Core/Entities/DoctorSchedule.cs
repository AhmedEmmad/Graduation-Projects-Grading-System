namespace GradingManagementSystem.Core.Entities
{
    public class DoctorSchedule : BaseEntity
    {
        public int ScheduleId { get; set; } // FK Of Id In Schedule Table
        public int DoctorId { get; set; } // FK Of Id In Doctor Table
        public string DoctorRole { get; set; }

        #region Navigation Properties
        public Schedule Schedule { get; set; }
        public Doctor Doctor { get; set; }
        #endregion
    }


}
