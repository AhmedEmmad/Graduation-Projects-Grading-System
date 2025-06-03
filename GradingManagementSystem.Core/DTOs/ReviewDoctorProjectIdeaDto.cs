using System.ComponentModel.DataAnnotations;

namespace GradingManagementSystem.Core.DTOs
{
    public class ReviewDoctorProjectIdeaDto
    {
        [Required(ErrorMessage = "RequestId is required")]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "NewStatus is required")]
        public string NewStatus { get; set; }
    }
}
