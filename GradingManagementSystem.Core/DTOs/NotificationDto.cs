using System.ComponentModel.DataAnnotations;

namespace GradingManagementSystem.Core.DTOs
{
    public class NotificationDto
    {
        [Required(ErrorMessage = "Title is required.")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Role is required.")]
        public string? Role { get; set; }
    }

}
