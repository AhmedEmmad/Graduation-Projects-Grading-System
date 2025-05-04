namespace GradingManagementSystem.Core.Entities
{
    public class TaskMember : BaseEntity
    {
        public int TaskId { get; set; } // FK Of Id In Task Table
        public int StudentId { get; set; } // FK Of Id In Student Table
        public int TeamId { get; set; } // FK Of Id In Team Table

        public string Status { get; set; } = StatusType.Pending.ToString();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? FinishedAt { get; set; }


        #region Navigation Properties
        public TaskItem Task { get; set; }
        public Student Student { get; set; }
        public Team Team { get; set; }
        #endregion
    }
}
