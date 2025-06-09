using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GradingManagementSystem.Core.Entities
{
    public class FinalProjectIdea
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ProjectId { get; set; }
        
        public string? ProjectName { get; set; }
        public string? ProjectDescription { get; set; }
        public int? TeamRequestDoctorProjectIdeaId { get; set; } // Foreign Key Of Id In TeamRequestDoctorProjectIdea Table
        public int? TeamProjectIdeaId { get; set; } // Foreign Key Of Id In TeamProjectIdea Table
        public int? SupervisorId { get; set; } // Foreign Key Of Id In Doctor Table
        public int? TeamId { get; set; } // Foreign Key Of Id In Team Table
        public int? AcademicAppointmentId { get; set; } // Foreign Key Of Id In AcademicAppointment Table


        public string? PostedBy { get; set; }


        #region navigation Properties
        public TeamProjectIdea TeamProjectIdea { get; set; }
        public TeamRequestDoctorProjectIdea TeamRequestDoctorProjectIdea { get; set; }
        public Doctor Supervisor { get; set; }
        public Team Team { get; set; }
        public AcademicAppointment AcademicAppointment { get; set; }
        #endregion
    }
}
