using System.ComponentModel.DataAnnotations;

namespace GradingManagementSystem.Core.DTOs
{
    public class ProjectIdeaFromTeamDto
    {
        [Required(ErrorMessage = "Project Name/Title Is Required")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Project Description Is Required")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "TeamId Is Required")]
        public int? TeamId { get; set; }
    }
}
