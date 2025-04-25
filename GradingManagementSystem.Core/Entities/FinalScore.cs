namespace GradingManagementSystem.Core.Entities
{
    public class FinalScore : BaseEntity
    {
        public int TeamId { get; set; } // Foreign Key Of Id In Team Table

        public double Score { get; set; } 
        public DateTime CalculatedAt { get; set; } = DateTime.Now;


        #region Navigation Properties
        public Team Team { get; set; }
        #endregion
    }
}
