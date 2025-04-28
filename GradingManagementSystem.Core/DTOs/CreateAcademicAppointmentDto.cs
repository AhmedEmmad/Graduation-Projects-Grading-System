using System.ComponentModel.DataAnnotations;

namespace GradingManagementSystem.Core.DTOs
{
    public class CreateAcademicAppointmentDto
    {
        [Required(ErrorMessage = "Academic Year is required")]
        public string Year { get; set; } // Like "2023-2024" Or "2024-2025"

        [Required(ErrorMessage = "First Term Start Date is required")]
        public DateTime FirstTermStart { get; set; }

        [Required(ErrorMessage = "First Term End Date is required")]
        public DateTime FirstTermEnd { get; set; }

        [Required(ErrorMessage = "Second Term Start Date is required")]
        public DateTime SecondTermStart { get; set; }

        [Required(ErrorMessage = "Second Term End Date is required")]
        public DateTime SecondTermEnd { get; set; }
    }
}
