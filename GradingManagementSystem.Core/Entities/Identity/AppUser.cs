using Microsoft.AspNetCore.Identity;

namespace GradingManagementSystem.Core.Entities.Identity
{
    // This Class Is Core Identity User For Authentication And Authorization
    public class AppUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Specialty { get; set; }


        #region Navigation Properties
        public Admin Admin { get; set; }
        public Doctor Doctor { get; set; }
        public Student Student { get; set; }
        #endregion
    }
}
