namespace GradingManagementSystem.Core.Entities
{
    public class DoctorCommittee : BaseEntity
    {
        public int DoctorId { get; set; } // Foreign Key of Id in Doctor table
        public int CommitteeId { get; set; } // Foreign Key of Id in Committee table


        #region Navigation Properties
        public Doctor Doctor { get; set; }
        public Committee Committee { get; set; }
        #endregion
    }
}
