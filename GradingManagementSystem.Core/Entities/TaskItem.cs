namespace GradingManagementSystem.Core.Entities
{
    public class TaskItem : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Deadline { get; set; }
        public DateTime StartTime { get; set; } = DateTime.Now;
        public string Status { get; set; } = StatusType.Pending.ToString();

        public int SupervisorId { get; set; } // FK Of Id In Doctor Table
        public int TeamId { get; set; } // FK Of Id In Team Table
        public int? AcademicAppointmentId { get; set; } // Foreign Key Of Id In AcademicAppointment Table


        #region Navigation Properties
        public ICollection<TaskMember> TaskMembers { get; set; } = new HashSet<TaskMember>();
        public Doctor Supervisor { get; set; }
        public Team Team { get; set; }
        public AcademicAppointment AcademicAppointment { get; set; }
        #endregion
    }
}
