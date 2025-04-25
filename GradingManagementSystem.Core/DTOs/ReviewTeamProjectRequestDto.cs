using System.ComponentModel.DataAnnotations;

namespace GradingManagementSystem.Core.DTOs
{
    public class ReviewTeamProjectRequestDto
    {
        [Required(ErrorMessage = "RequestId is required")]
        public int RequestId { get; set; }

        [Required(ErrorMessage = "NewStatus is required")]
        public string NewStatus { get; set; }

        [Required(ErrorMessage = "DoctorId is required")]
        public int DoctorId { get; set; }
    }
}
