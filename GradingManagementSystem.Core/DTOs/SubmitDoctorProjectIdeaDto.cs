using System.ComponentModel.DataAnnotations;

namespace GradingManagementSystem.Core.DTOs
{
    public class SubmitDoctorProjectIdeaDto
    {
        [Required(ErrorMessage = "Project Title is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Project Description is required.")]
        public string Description { get; set; }
    }
}
