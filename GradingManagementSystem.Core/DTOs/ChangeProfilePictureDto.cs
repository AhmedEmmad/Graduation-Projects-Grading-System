using Microsoft.AspNetCore.Http;

namespace GradingManagementSystem.Core.DTOs
{
    public class ChangeProfilePictureDto
    {
        public IFormFile ProfilePicture { get; set; }
    }
}
