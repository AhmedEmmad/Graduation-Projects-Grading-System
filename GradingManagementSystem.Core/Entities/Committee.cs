namespace GradingManagementSystem.Core.Entities
{
    public class Committee : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public int? TeamId { get; set; } // Foreign Key Of Id In Team Table


        #region Navigation Properties
        public Team Team { get; set; }
        public ICollection<DoctorCommittee> DoctorCommittees { get; set; } = new HashSet<DoctorCommittee>();
        public ICollection<CommitteeAssignment> CommitteeAssignments { get; set; } = new HashSet<CommitteeAssignment>();
        #endregion
    }
}
