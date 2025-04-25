namespace GradingManagementSystem.Core.Entities
{
    public class CommitteeAssignment : BaseEntity
    {
        public int CommitteeId { get; set; } // Foreign Key of Id in Committee table
        public int TeamId { get; set; } // Foreign Key of Id in Team table


        #region Navigation Properties
        public Committee Committee { get; set; }
        public Team Team { get; set; }
        #endregion
    }
}
