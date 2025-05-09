using System.ComponentModel.DataAnnotations;

namespace GradingManagementSystem.Core.DTOs
{
    public class ReviewTeamProjectIdeaDto
    {
        [Required(ErrorMessage = "RequestId is required")]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "NewStatus is required")]
        public string? NewStatus { get; set; }

        [Required(ErrorMessage = "SupervisorId is required")]
        public int? SupervisorId { get; set; }
    }
}
