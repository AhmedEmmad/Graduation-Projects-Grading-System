namespace GradingManagementSystem.Core.Entities
{
    public class StudentTotalGrade : BaseEntity
    {
        public int? StudentId { get; set; }
        public int? TeamId { get; set; }
        public double? Total { get; set; }
    }
}
