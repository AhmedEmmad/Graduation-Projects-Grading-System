using System.ComponentModel.DataAnnotations;

namespace GradingManagementSystem.Core.DTOs
{
    public class ProjectIdeaFromDrDto
    {
        [Required(ErrorMessage = "Project title is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Project description is required.")]
        public string Description { get; set; }
    }
}
