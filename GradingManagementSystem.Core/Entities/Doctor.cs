using GradingManagementSystem.Core.Entities.Identity;

namespace GradingManagementSystem.Core.Entities
{
    public class Doctor : BaseEntity
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTime EnrollmentDate { get; set; } = DateTime.Now.AddHours(1);

        public string AppUserId { get; set; } // Foreign Key Of Id In AspNetUsers Table


        #region Navigation Properties
        public AppUser AppUser { get; set; }
        public ICollection<DoctorProjectIdea> DoctorProjectIdeas { get; set; }
        public ICollection<Team> Teams { get; set; } = new HashSet<Team>();
        public ICollection<TeamRequestDoctorProjectIdea> TeamsRequestDoctorProjectIdeas { get; set; }
        public ICollection<TaskItem> Tasks { get; set; } = new HashSet<TaskItem>();
        public ICollection<CommitteeDoctorSchedule> DoctorSchedules { get; set; } = new HashSet<CommitteeDoctorSchedule>();
        public ICollection<Evaluation> Evaluations { get; set; } = new HashSet<Evaluation>();
        #endregion
    }
}
