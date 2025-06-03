using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace GradingManagementSystem.Core.DTOs
{
    public class StudentRegisterDto
    {
            [Required(ErrorMessage = "FullName is required.")]
            public string? FullName { get; set; }

            [Required(ErrorMessage = "Email is required.")]
            [EmailAddress(ErrorMessage = "Invalid email address format.")]
            public string? Email { get; set; }

            [Required(ErrorMessage = "Specialty is required.")]
            public string? Specialty { get; set; }
            
            [Required(ErrorMessage = "ProfilePicture is required.")]
            public IFormFile? ProfilePicture { get; set; }

            [Required(ErrorMessage = "Password is required.")] // (?=.*[!@#$%^&*?])
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_])[A-Za-z\d\W_]{8,50}$",
                ErrorMessage = "Password must contain at least 1 uppercase letter, 1 lowercase letter, 1 digit, and 1 special character and At Least 8 Characters.")]
            public string? Password { get; set; }
    }
}
