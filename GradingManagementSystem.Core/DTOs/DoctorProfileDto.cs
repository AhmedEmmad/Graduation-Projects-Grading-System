using GradingManagementSystem.Core.Entities;

namespace GradingManagementSystem.Core.DTOs
{
    public class DoctorProfileDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string Role { get; set; }
        public IEnumerable<TeammDto> Teams { get; set; }
    }
}
