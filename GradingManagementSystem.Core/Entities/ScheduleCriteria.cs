namespace GradingManagementSystem.Core.Entities
{
    public class ScheduleCriteria : BaseEntity
    {
        public int ScheduleId { get; set; } // Foreign Key Of Id In Schedule Table
        public int CriteriaId { get; set; } // Foreign Key Of Id In Criteria Table


        #region Navigation Properties
        public Schedule Schedule { get; set; }
        public Criteria Criteria { get; set; }
        #endregion
    }
}