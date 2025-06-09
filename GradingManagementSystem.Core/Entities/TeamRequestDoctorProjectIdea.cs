namespace GradingManagementSystem.Core.Entities
{
    public class TeamRequestDoctorProjectIdea : BaseEntity
    {
        public string Status { get; set; } = StatusType.Pending.ToString();
        public DateTime RequestedDate { get; set; } = DateTime.Now;

        public int TeamId { get; set; } // Foreign Key Of Id In Team Table
        public int LeaderId { get; set; } // Foreign Key Of Id In Student Table
        public int DoctorId { get; set; } // Foreign Key Of Id In Doctor Table
        public int DoctorProjectIdeaId { get; set; } // Foreign Key Of Id In DoctorProjectIdea Table
        public int? AcademicAppointmentId { get; set; } // Foreign Key Of Id In AcademicAppointment Table


        #region Navigation Properties
        public Team Team { get; set; }
        public Student Leader { get; set; }
        public Doctor Doctor { get; set; }
        public DoctorProjectIdea DoctorProjectIdea { get; set; }
        public AcademicAppointment AcademicAppointment { get; set; }
        #endregion
    }
}
