using System.ComponentModel.DataAnnotations;

namespace GradingManagementSystem.Core.DTOs
{
    public class AdminDoctorRegisterDto
    {
        [Required(ErrorMessage = "FullName is required.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#])[A-Za-z\d@$!%*?&#]{8,50}$",
            ErrorMessage = "Password must contain at least 1 uppercase, 1 lowercase, 1 digit, and 1 special character and At Least 8 Characters.")]
        public string Password { get; set; }
    }
}
