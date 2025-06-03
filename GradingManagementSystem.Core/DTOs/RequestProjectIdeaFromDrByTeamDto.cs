using System.ComponentModel.DataAnnotations;

namespace GradingManagementSystem.Core.DTOs
{
    public class RequestProjectIdeaFromDrByTeamDto
    {
        [Required(ErrorMessage = "RequestId is required")]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "TeamLeaderId is required")]
        public int TeamLeaderId { get; set; }

        [Required(ErrorMessage = "TeamId is required")]
        public int TeamId { get; set; }

        [Required(ErrorMessage = "SupervisorId is required")]
        public int DoctorId { get; set; }
    }
}
