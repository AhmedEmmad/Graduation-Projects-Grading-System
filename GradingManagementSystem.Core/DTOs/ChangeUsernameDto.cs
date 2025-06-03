using System.ComponentModel.DataAnnotations;

namespace GradingManagementSystem.Core.DTOs
{
    public class ChangeUsernameDto
    {
        [Required(ErrorMessage = "NewUsername Is Required")]
        public string? NewUsername { get; set; }
    }
    
}
