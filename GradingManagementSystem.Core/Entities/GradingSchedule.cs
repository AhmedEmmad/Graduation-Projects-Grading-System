namespace GradingManagementSystem.Core.Entities
{
    public class GradingSchedule : BaseEntity
    {
        public DateTime ScheduleDate { get; set; }


        #region Navigation Properties
        public ICollection<Committee> Committees { get; set; } = new HashSet<Committee>();
        #endregion
    }
}
