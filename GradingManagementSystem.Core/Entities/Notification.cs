using GradingManagementSystem.Core.Entities.Identity;

namespace GradingManagementSystem.Core.Entities
{
    // storing instructions.
    public class Notification : BaseEntity
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Role { get; set; } // Doctors or Students or All
        public bool IsRead { get; set; } = false;
        public DateTime SentAt { get; set; } = DateTime.Now;
        
        public int? AdminId { get; set; } // Foreign Key Of Id In Admin Table


        #region Navigation Properties
        public Admin Admin { get; set; }
        #endregion
    }
    
}
