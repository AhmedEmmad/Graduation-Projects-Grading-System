namespace GradingManagementSystem.Core.Entities
{
    public class AcademicAppointment : BaseEntity
    {
        public string Year { get; set; } // Format: "2023-2024"

        // First Semester
        public DateOnly FirstTermStart { get; set; }
        public DateOnly FirstTermEnd { get; set; }

        // Second Semester
        public DateOnly SecondTermStart { get; set; }
        public DateOnly SecondTermEnd { get; set; }

        public string Status { get; set; } = "Inactive"; // "Active" or "Inactive"
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastUpdatedAt { get; set; } = null;


        #region Navigation Properties
        public ICollection<Criteria> Criterias { get; set; } = new HashSet<Criteria>();
        #endregion
    }
}