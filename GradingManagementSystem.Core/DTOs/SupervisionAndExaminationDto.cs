using System.Collections;

namespace GradingManagementSystem.Core.DTOs
{
    public class SupervisionAndExaminationDto
    {
        public ICollection SupervisionTeams { get; set; }
        public ICollection ExaminationTeams { get; set; }
    }
}
