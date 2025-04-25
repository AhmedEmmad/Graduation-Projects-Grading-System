namespace GradingManagementSystem.Core.Entities
{
    public class Schedule : BaseEntity
    {
        public int TeamId { get; set; } // Foreign Key Of Id In Team Table
        public DateTime ScheduleDate { get; set; }


        #region Navigation Properties
        public Team Team { get; set; }
        public ICollection<DoctorSchedule> DoctorSchedules { get; set; } = new HashSet<DoctorSchedule>();
        public ICollection<ScheduleCriteria> ScheduleCriterias { get; set; } = new HashSet<ScheduleCriteria>();
        #endregion
    }
}