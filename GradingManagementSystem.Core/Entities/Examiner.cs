namespace GradingManagementSystem.Core.Entities
{
    public class Examiner : BaseEntity
    {
        public string Name { get; set; }


        #region Navigation Properties
        public ICollection<Evaluation> Evaluations { get; set; } = new HashSet<Evaluation>();
        #endregion
    }
}
