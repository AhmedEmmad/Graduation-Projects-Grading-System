using GradingManagementSystem.Core.DTOs;
using GradingManagementSystem.Core.CustomResponses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GradingManagementSystem.Core.Services.Contact;
using GradingManagementSystem.Core.Entities.Identity;
using GradingManagementSystem.Core.Entities;
using GradingManagementSystem.Core;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace GradingManagementSystem.APIs.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class UserProfileController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;
        private readonly IUnitOfWork _unitOfWork;

        public UserProfileController(IUserProfileService userProfileService, IUnitOfWork unitOfWork)
        {
            _userProfileService = userProfileService;
            _unitOfWork = unitOfWork;
        }

        // Updated
        [HttpGet("GetProfile")]
        [Authorize(Roles = "Student,Doctor,Admin")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiResponse(401, "Unauthorized user."));
            var user = await _unitOfWork.Repository<AppUser>().FindAsync(u => u.Id == userId);


            if (user == null)
                return NotFound(new ApiResponse(404, "User not found."));

            var userProfile = new UserProfileDto
            {
                Email = user.Email,
                ProfilePicture = user.ProfilePicture
            };

            var doctor = await _unitOfWork.Repository<Doctor>().FindAsync(d => d.AppUserId == userId);
            if (doctor != null)
            {
                userProfile.Role = "Doctor";
                userProfile.FullName = doctor.FullName;
                userProfile.Id = doctor.Id;

                return Ok(new ApiResponse(200, "Doctor Profile Retrieved Successfully.", userProfile ));
            }

            var student = await _unitOfWork.Repository<Student>().FindAsync(s => s.AppUserId == userId);
            if (student != null)
            {
                userProfile.Role = "Student";
                userProfile.FullName = student.FullName;
                userProfile.Id = student.Id;
                userProfile.TeamId = student.TeamId;
                userProfile.LeaderOfTeamId = student.LeaderOfTeamId;
                userProfile.InTeam = student.InTeam;

                return Ok(new ApiResponse(200, "Student Profile Retrieved Successfully.",userProfile ));
            }

            var admin = await _unitOfWork.Repository<Admin>().FindAsync(a => a.AppUserId == userId);
            if (admin != null)
            {
                userProfile.Role = "Admin";
                userProfile.FullName = admin.FullName;
                userProfile.Id = admin.Id;
                return Ok(new ApiResponse(200, "Admin Profile Retrieved Successfully.", userProfile ) );
            }

            return NotFound(new ApiResponse(404, "User role not found."));
        }

        [HttpPut("ChangeUsername/{newUsername}")]
        [Authorize(Roles = "Student, Doctor, Admin")]
        public async Task<IActionResult> ChangeUsername(string newUsername)
        {
            if (string.IsNullOrEmpty(newUsername))
                return BadRequest(new ApiResponse(400, "NewUsername invalid.", new { IsSuccess = false }));

            var userId = User.FindFirst("UserId")?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiResponse(401, "Unauthorized user.", new { IsSuccess = false }));
            if (string.IsNullOrEmpty(userRole))
                return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));

            var response = await _userProfileService.ChangeUsernameAsync(newUsername, userId, userRole);

            if (response.StatusCode == 404)
                return NotFound(response);
            if (response.StatusCode == 400)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPut("ChangeEmail/{newEmail}")]
        [Authorize(Roles = "Student, Doctor, Admin")]
        public async Task<IActionResult> ChangeEmail(string newEmail)
        {
            if (string.IsNullOrEmpty(newEmail))
                return BadRequest(new ApiResponse(400, "New email is invalid.", new { IsSuccess = false }));
            var userId = User.FindFirst("UserId")?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiResponse(401, "Unauthorized user.", new { IsSuccess = false }));
            if (string.IsNullOrEmpty(userRole))
                return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));
            var response = await _userProfileService.ChangeEmailAsync(newEmail, userId, userRole);
            if (response.StatusCode == 404)
                return NotFound(response);
            if (response.StatusCode == 400)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPut("ChangePassword")]
        [Authorize(Roles = "Student, Doctor, Admin")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            if (string.IsNullOrEmpty(model.CurrentPassword) || string.IsNullOrEmpty(model.NewPassword))
                return BadRequest(new ApiResponse(400, "Current password or new password is invalid.", new { IsSuccess = false }));

            var userId = User.FindFirst("UserId")?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiResponse(401, "Unauthorized user."));
            if (string.IsNullOrEmpty(userRole))
                return Unauthorized(new ApiResponse(401, "Unauthorized access."));

            var response = await _userProfileService.ChangePasswordAsync(model.CurrentPassword, model.NewPassword, userId);

            if (response.StatusCode == 404)
                return NotFound(response);
            if (response.StatusCode == 400)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPut("ChangeProfilePicture")]
        [Authorize(Roles = "Student, Doctor, Admin")]
        public async Task<IActionResult> ChangeProfilePicture([FromForm] ChangeProfilePictureDto model)
        {
            if (model.ProfilePicture == null || model.ProfilePicture.Length == 0)
                return BadRequest(new ApiResponse(400, "Profile picture is invalid.", new { IsSuccess = false }));

            var userId = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiResponse(401, "Unauthorized user.", new { IsSuccess = false }));

            var response = await _userProfileService.ChangeProfilePictureAsync(model.ProfilePicture, userId);

            if (response.StatusCode == 404)
                return NotFound(response);
            if (response.StatusCode == 400)
                return BadRequest(response);

            return Ok(response);
        }
    }
}
