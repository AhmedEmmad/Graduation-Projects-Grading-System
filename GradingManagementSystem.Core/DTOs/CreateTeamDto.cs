using System.ComponentModel.DataAnnotations;

namespace GradingManagementSystem.Core.DTOs
{
    public class CreateTeamDto
    {
        [Required(ErrorMessage = "Team Name is required.")]
        public string? TeamName { get; set; }
    }
}
