using GradingManagementSystem.Core.Entities.Identity;

namespace GradingManagementSystem.Core.Entities
{
    public class Admin : BaseEntity
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTime EnrollmentDate { get; set; } = DateTime.Now;

        public string AppUserId { get; set; } // Foreign Key Of Id In AspNetUsers Table


        #region Navigation Properties
        public AppUser AppUser { get; set; }
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<Evaluation> Evaluations { get; set; }
        #endregion
    }
}
