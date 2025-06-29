﻿namespace GradingManagementSystem.Core.Entities
{
    public class Criteria : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int MaxGrade { get; set; }
        public string Evaluator { get; set; } // "Admin" Or "Supervisor" Or "Examiner"
        public string GivenTo { get; set; } // "Student" Or "Team"
        public string Specialty { get; set; }
        public string Year { get; set; }
        public string Term { get; set; } // Format: "First-Term"
        public DateTime CreatedAt { get; set; } = DateTime.Now.AddHours(1);
        public DateTime? LastUpdatedAt { get; set; } = null;
        public bool IsActive { get; set; } = true;

        public int AcademicAppointmentId { get; set; } // Foreign Key Of Id In AcademicAppointment Table


        #region Navigation Properties
        public AcademicAppointment AcademicAppointment { get; set; }
        public ICollection<Schedule> Schedules { get; set; } = new HashSet<Schedule>();
        public ICollection<Evaluation> Evaluations { get; set; } = new HashSet<Evaluation>();
        //public ICollection<Team> Teams { get; set; } = new HashSet<Team>();
        public ICollection<CriteriaSchedule> CriteriaSchedules { get; set; } = new List<CriteriaSchedule>();
        #endregion
    }
}
