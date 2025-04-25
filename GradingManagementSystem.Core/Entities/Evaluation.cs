namespace GradingManagementSystem.Core.Entities
{
    public class Evaluation : BaseEntity
    {
        public int CriteriaId { get; set; } // Foreign Key Of Id In Criteria Table
        public int ExaminerId { get; set; } // Foreign Key Of Id In Examiner Table
        public int Score { get; set; }
        public DateTime EvaluationDate { get; set; } = DateTime.Now;


        #region Navigation Properties
        public Criteria Criteria { get; set; }
        public Examiner Examiner { get; set; }
        #endregion
    }
}
