namespace GradingManagementSystem.Core.Entities
{
    public class AcademicAppointment : BaseEntity
    {
        public string Year { get; set; } // Format: "2023-2024"

        // First Semester
        public DateTime FirstTermStart { get; set; }
        public DateTime FirstTermEnd { get; set; }

        // Second Semester
        public DateTime SecondTermStart { get; set; }
        public DateTime SecondTermEnd { get; set; }

        public string Status { get; set; } = "Inactive"; // "Active" or "Inactive"
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastUpdatedAt { get; set; } = null;


        #region Navigation Properties
        public ICollection<Criteria> Criterias { get; set; } = new HashSet<Criteria>();
        #endregion
    }
}