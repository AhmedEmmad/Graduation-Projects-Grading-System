using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GradingManagementSystem.Core.Entities
{
    public class FinalProjectIdea
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ProjectId { get; set; }
        
        public string? ProjectName { get; set; }
        public string? ProjectDescription { get; set; }
        public int? TeamRequestDoctorProjectIdeaId { get; set; }
        public int? TeamProjectIdeaId { get; set; }
        public int? SupervisorId { get; set; }
        public int? TeamId { get; set; }
        
        public string? PostedBy { get; set; }


        #region navigation Properties
        public TeamProjectIdea TeamProjectIdea { get; set; }
        public TeamRequestDoctorProjectIdea TeamRequestDoctorProjectIdea { get; set; }
        public Doctor Supervisor { get; set; }
        public Team Team { get; set; }
        #endregion
    }
}
