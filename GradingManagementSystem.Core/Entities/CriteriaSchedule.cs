namespace GradingManagementSystem.Core.Entities
{
    public class CriteriaSchedule : BaseEntity
    {
        public int CriteriaId { get; set; } // Foreign Key Of Id In Criteria Table
        public int ScheduleId { get; set; } // Foreign Key Of Id In Schedule Table

        public int MaxGrade { get; set; }


        #region Navigation Properties
        public Criteria Criteria { get; set; }
        public Schedule Schedule { get; set; }
        #endregion
    }
}
