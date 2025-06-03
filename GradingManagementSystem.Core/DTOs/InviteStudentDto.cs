using System.ComponentModel.DataAnnotations;

namespace GradingManagementSystem.Core.DTOs
{
    public class InviteStudentDto
    {
        [Required(ErrorMessage = "LeaderId is required")]
        public int? LeaderId { get; set; }

        [Required(ErrorMessage = "StudentId is required")]
        public int? StudentId { get; set; }

        [Required(ErrorMessage = "TeamId is required")]
        public int? TeamId { get; set; }
    }
}
