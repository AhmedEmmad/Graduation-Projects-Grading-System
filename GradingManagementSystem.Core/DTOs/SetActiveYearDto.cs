using System.ComponentModel.DataAnnotations;

namespace GradingManagementSystem.Core.DTOs
{
    public class SetActiveYearDto
    {
        [Required(ErrorMessage = "AppointmentId is required.")]
        public int AppointmentId { get; set; }
    }
}
