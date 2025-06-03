using System.ComponentModel.DataAnnotations;

namespace GradingManagementSystem.Core.DTOs
{
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "Password is required.")]
        public string? NewPassword { get; set; }

        [Required(ErrorMessage = "ConfirmPassword is required.")]
        public string? ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Token is required.")]
        public string? Token { get; set; }
    }
}
