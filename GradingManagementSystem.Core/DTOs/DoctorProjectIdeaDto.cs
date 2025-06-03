namespace GradingManagementSystem.Core.DTOs
{
    public class DoctorProjectIdeaDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string Status { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
    }
}
