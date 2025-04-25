namespace GradingManagementSystem.Core.Entities
{
    public class Notification : BaseEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public NotificationRole Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;
        
        public int? AdminId { get; set; } // Foreign Key Of Id In Admin Table


        #region Navigation Properties
        public Admin Admin { get; set; }
        #endregion
    }
    
}
