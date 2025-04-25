namespace GradingManagementSystem.Core.Entities
{
    public class SupervisorGrading
    {
        public int GradingCriteriaId { get; set; } // FK Of Id In GradingCriteria Table
        public int StudentId { get; set; } // FK Of Id In Student Table
        public int DoctorId { get; set; } // FK Of Id In Doctor Table
        
        public double Marks { get; set; }
        

        #region Navigation Properties
        public GradingCriteria GradingCriteria { get; set; }
        public Student Student { get; set; }
        public Doctor Doctor { get; set; }
        #endregion
    }
}
