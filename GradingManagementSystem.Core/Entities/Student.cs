using GradingManagementSystem.Core.Entities.Identity;

namespace GradingManagementSystem.Core.Entities
{
    public class Student : BaseEntity
    {
        public string? FullName { get; set; }
        //public string? ArabicFullName { get; set; }
        public string? Email { get; set; }
        public string? Specialty { get; set; }
        public bool InTeam { get; set; } = false;
        public DateTime EnrollmentDate { get; set; } = DateTime.Now.AddHours(1);

        public string? AppUserId { get; set; } // Foreign Key Of Id In AspNetUsers Table
        public int? TeamId { get; set; } = null; // Foreign Key Of Id In Team Table
        public int? LeaderOfTeamId { get; set; } = null; // Foreign Key Of Id In Team Table To Represent Leader Of Team
        public int? AcademicAppointmentId { get; set; } // Foreign Key Of Id In AcademicAppointment Table


        #region Navigation properties
        public AppUser AppUser { get; set; }
        public ICollection<TeamProjectIdea> TeamProjectIdeas { get; set; }
        public Team Team { get; set; }
        public Team LeaderOfTeam { get; set; }
        public ICollection<TeamRequestDoctorProjectIdea> TeamsRequestDoctorProjectIdeas { get; set; }
        public ICollection<TaskMember> TaskMembers { get; set; } = new HashSet<TaskMember>();
        public ICollection<Evaluation> Evaluations { get; set; } = new HashSet<Evaluation>();
        public AcademicAppointment AcademicAppointment { get; set; }
        #endregion
    }
}
