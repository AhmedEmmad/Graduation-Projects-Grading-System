using System.ComponentModel.DataAnnotations;

namespace GradingManagementSystem.Core.DTOs
{
   
        public class ChangeUsernameDto
        {
            [Required(ErrorMessage = "Name/Title Is Required")]
            public string NewUsername { get; set; }
        }
    
}
