using GradingManagementSystem.Core.Entities.Identity;

namespace GradingManagementSystem.Core.Entities
{
    public class Evaluation : BaseEntity
    {
        public int? ScheduleId { get; set; }
        public int CriteriaId { get; set; }
        public int EvaluatorId { get; set; } // DoctorId or AdminId
        public string EvaluatorRole { get; set; } // "Admin", "Supervisor", or "Examiner"
        public int? StudentId { get; set; }
        public int TeamId { get; set; }
        public double Grade { get; set; }
        public bool AdminEvaluation { get; set; } = false;
        public DateTime EvaluationDate { get; set; } = DateTime.Now;
        public DateTime? LastUpdatedAt { get; set; } = null;


        #region Navigation properties
        public Criteria Criteria { get; set; }
        public Schedule Schedule { get; set; }
        public AppUser EvaluatorUser { get; set; } // AppUser to handle both doctors and admins
        public Student Student { get; set; }
        public Team Team { get; set; }
        #endregion
    }
}
