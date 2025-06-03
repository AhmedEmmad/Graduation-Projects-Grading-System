namespace GradingManagementSystem.Core.Entities
{
    public class Invitation : BaseEntity
    {
        public int? TeamId { get; set; } // Foreign Key Of Id In Team Table
        public int? LeaderId { get; set; } // Foreign Key Of Id In Student Table
        public int? StudentId { get; set; } // Foreign Key Of Id In Student Table

        public string? Status { get; set; } = StatusType.Pending.ToString();
        public DateTime SentDate { get; set; } = DateTime.Now;
        public DateTime? RespondedDate { get; set; } = null;


        #region Navigation Properties
        public Team Team { get; set; }
        public Student Leader { get; set; }
        public Student Student { get; set; }
        #endregion
    }
}
