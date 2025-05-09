using System.ComponentModel.DataAnnotations;

namespace GradingManagementSystem.Core.DTOs
{
    public class CreateScheduleDto
    {
        [Required(ErrorMessage = "TeamId is required.")]
        public int? TeamId { get; set; }

        [Required(ErrorMessage = "ScheduleDate is required.")]
        public DateTime ScheduleDate { get; set; }

        [Required(ErrorMessage = "CommitteeDoctorIds are required.")]
        public List<int> CommitteeDoctorIds { get; set; } = new();
    }
}
