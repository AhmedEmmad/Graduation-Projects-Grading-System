using GradingManagementSystem.Core.CustomResponses;
using Microsoft.AspNetCore.Http;

namespace GradingManagementSystem.Core.Services.Contact
{
    public interface IUserProfileService
    {
        Task<ApiResponse> GetUserProfileAsync(string userId, string userRole, string timezoneId);
        Task<ApiResponse> ChangeUsernameAsync(string newUsername, string userId, string userRole);
        Task<ApiResponse> ChangePasswordAsync(string oldPassword, string newPassword, string userId);
        Task<ApiResponse> ChangeProfilePictureAsync(IFormFile newProfilePicture, string userId, string userRole);
    }
}
