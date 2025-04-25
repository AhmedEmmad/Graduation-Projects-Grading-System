namespace GradingManagementSystem.Core.DTOs
{
    public class CreateScheduleDto
    {
        public int TeamId { get; set; }
        public DateTime ScheduleDate { get; set; }
        public List<int> CommitteeDoctorIds { get; set; }
    }
}
