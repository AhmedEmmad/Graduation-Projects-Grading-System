namespace GradingManagementSystem.Core.Entities
{
    public class Team : BaseEntity
    {
        public string? Name { get; set; }
        public bool HasProject { get; set; } = false;
        public string? Specialty { get; set; }

        public int? LeaderId { get; set; } // Foreign Key Of Id In Student Table
        public int? SupervisorId { get; set; } = null; // Foreign Key Of Id In Doctor Table
        public int? AcademicAppointmentId { get; set; } // Foreign Key Of Id In AcademicAppointment Table


        #region Navigation Properties
        public ICollection<TeamProjectIdea> TeamProjectIdeas { get; set; } = new HashSet<TeamProjectIdea>();
        public Student Leader { get; set; }
        public ICollection<Student> Students { get; set; } = new HashSet<Student>();
        public Doctor Supervisor { get; set; }
        public ICollection<TeamRequestDoctorProjectIdea> TeamsRequestDoctorProjectIdeas { get; set; } = new HashSet<TeamRequestDoctorProjectIdea>();
        public ICollection<TaskItem> Tasks { get; set; } = new HashSet<TaskItem>();
        public ICollection<TaskMember> TaskMembers { get; set; } = new HashSet<TaskMember>();
        public ICollection<Schedule> Schedules { get; set; }
        public FinalProjectIdea FinalProjectIdea { get; set; }
        //public ICollection<Criteria> Criterias { get; set; } = new HashSet<Criteria>();
        public ICollection<Evaluation> Evaluations { get; set; } = new HashSet<Evaluation>();
        public AcademicAppointment AcademicAppointment { get; set; }
        #endregion
    }
}
