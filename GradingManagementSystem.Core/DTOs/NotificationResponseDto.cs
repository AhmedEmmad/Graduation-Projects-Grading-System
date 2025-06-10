namespace GradingManagementSystem.Core.DTOs
{
    public class NotificationResponseDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime SentAt { get; set; }
        public string? Role { get; set; }
        public bool IsReadFromAdmin { get; set; }
        public bool IsReadFromDoctor { get; set; }
        public bool IsReadFromStudent { get; set; }
        public int? AdminId { get; set; }
    }
}
