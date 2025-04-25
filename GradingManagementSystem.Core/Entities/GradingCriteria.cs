namespace GradingManagementSystem.Core.Entities
{
    public class GradingCriteria : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int MaxGrade { get; set; }
        public string ForEntity { get; set; }


        #region Navigation Properties 
        //public ICollection<CommitteeGrading> CommitteeGradings { get; set; } = new HashSet<CommitteeGrading>();
        public ICollection<SupervisorGrading> SupervisorGradings { get; set; } = new HashSet<SupervisorGrading>();
        #endregion
    }
}
