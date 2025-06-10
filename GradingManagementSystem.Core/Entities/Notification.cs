using GradingManagementSystem.Core.Entities.Identity;

namespace GradingManagementSystem.Core.Entities
{
    public class Notification : BaseEntity
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Role { get; set; } // Doctors or Students or All
        public bool IsReadFromAdmin { get; set; } = false;
        public bool IsReadFromDoctor { get; set; } = false;
        public bool IsReadFromStudent { get; set; } = false;
        public DateTime SentAt { get; set; } = DateTime.Now.AddHours(1);
        
        public int? AdminId { get; set; } // Foreign Key Of Id In Admin Table
        public int? AcademicAppointmentId { get; set; } // Foreign Key Of Id In AcademicAppointment Table


        #region Navigation Properties
        public Admin Admin { get; set; }
        public AcademicAppointment AcademicAppointment { get; set; }
        #endregion
    }
    
}
