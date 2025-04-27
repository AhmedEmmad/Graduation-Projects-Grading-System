using System.ComponentModel.DataAnnotations;

namespace GradingManagementSystem.Core.DTOs
{
    public class CreateAcademicAppointment
    {
        [Required(ErrorMessage = "Academic Year is required")]
        public string Year { get; set; } // Like "2023-2024" Or "2024-2025"

        [Required(ErrorMessage = "First Term Start Date is required")]
        public DateOnly FirstTermStart { get; set; }

        [Required(ErrorMessage = "First Term End Date is required")]
        public DateOnly FirstTermEnd { get; set; }

        [Required(ErrorMessage = "Second Term Start Date is required")]
        public DateOnly SecondTermStart { get; set; }

        [Required(ErrorMessage = "Second Term End Date is required")]
        public DateOnly SecondTermEnd { get; set; }
    }
}
