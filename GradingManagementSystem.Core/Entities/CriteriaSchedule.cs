namespace GradingManagementSystem.Core.Entities
{
    public class CriteriaSchedule : BaseEntity
    {
        public int CriteriaId { get; set; }
        public int ScheduleId { get; set; }
        public int MaxGrade { get; set; }

        public Criteria Criteria { get; set; }
        public Schedule Schedule { get; set; }
    }
}
