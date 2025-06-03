using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace GradingManagementSystem.Core.DTOs
{
    public class TemporaryUserDto
    {
        [Required(ErrorMessage = "FullName Is Required.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email Is Required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Specialty Is Required.")]
        public string Specialty { get; set; }

        [Required(ErrorMessage = "ProfilePicture Is Required.")]
        public IFormFile? ProfilePicture { get; set; }

        [Required(ErrorMessage = "Password Is Required.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#])[A-Za-z\d@$!%*?&#]{8,50}$",
            ErrorMessage = "Password must contain at least 1 uppercase, 1 lowercase, 1 digit, and 1 special character and At Least 8 Characters.")]
        public string Password { get; set; }
    }
}
