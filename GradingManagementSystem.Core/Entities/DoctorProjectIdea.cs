﻿namespace GradingManagementSystem.Core.Entities
{
    public class DoctorProjectIdea : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime SubmissionDate { get; set; } = DateTime.Now.AddHours(1);
        public string Status { get; set; } = StatusType.Pending.ToString();
        public bool Taken { get; set; } = false;

        public int DoctorId { get; set; } // Foreign Key Of Id In Doctor Table
        public int? AcademicAppointmentId { get; set; } // Foreign Key Of Id In AcademicAppointment Table


        #region Navigation Properties
        public Doctor Doctor { get; set; }
        public ICollection<TeamRequestDoctorProjectIdea> TeamsRequestDoctorProjectIdeas { get; set; }
        public AcademicAppointment AcademicAppointment { get; set; }
        #endregion
    }
}
