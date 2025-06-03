namespace GradingManagementSystem.Core.DTOs
{
    public class AcademicYearAppointmentDto
    {
        public int Id { get; set; }
        public string? Year { get; set; }
        public DateTime FirstTermStart { get; set; }
        public DateTime FirstTermEnd { get; set; }
        public DateTime SecondTermStart { get; set; }
        public DateTime SecondTermEnd { get; set; }
        public string? Status { get; set; }
    }
}
