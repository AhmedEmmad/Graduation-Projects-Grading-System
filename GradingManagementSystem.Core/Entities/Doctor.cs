using GradingManagementSystem.Core.Entities.Identity;

namespace GradingManagementSystem.Core.Entities
{
    public class Doctor : BaseEntity
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTime EnrollmentDate { get; set; } = DateTime.Now;

        public string AppUserId { get; set; } // FK Of Id In AspNetUsers Table


        #region Navigation Properties
        public AppUser AppUser { get; set; }
        public ICollection<DoctorProjectIdea> DoctorProjectIdeas { get; set; }
        public ICollection<Team> Teams { get; set; } = new HashSet<Team>();
        public ICollection<TeamRequestDoctorProjectIdea> TeamsRequestDoctorProjectIdeas { get; set; }
        public ICollection<TaskItem> Tasks { get; set; } = new HashSet<TaskItem>();
        public ICollection<DoctorSchedule> DoctorSchedules { get; set; } = new HashSet<DoctorSchedule>();
        public ICollection<SupervisorGrading> SupervisorGradings { get; set; } = new HashSet<SupervisorGrading>();
        public ICollection<DoctorCommittee> DoctorCommittees { get; set; } = new HashSet<DoctorCommittee>();
        public ICollection<Schedule> Schedules { get; set; } = new HashSet<Schedule>();
        #endregion
    }
}
