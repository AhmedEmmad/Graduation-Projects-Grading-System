namespace GradingManagementSystem.Core.Entities
{
    public class TeamProjectIdea : BaseEntity
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime SubmissionDate { get; set; } = DateTime.Now;
        public string? Status { get; set; } = StatusType.Pending.ToString();

        public int? TeamId { get; set; } // Foreign Key Of Id In Team Table
        public int? LeaderId { get; set; } // Foreign Key Of Id In Student Table


        #region Navigation Properties
        public Team Team { get; set; }
        public Student Leader { get; set; }
        #endregion
    }
}
