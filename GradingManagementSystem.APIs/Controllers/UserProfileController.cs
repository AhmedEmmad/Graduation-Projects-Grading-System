using GradingManagementSystem.Core.DTOs;
using GradingManagementSystem.Core.CustomResponses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GradingManagementSystem.Core.Services.Contact;
using GradingManagementSystem.Core;
using GradingManagementSystem.Repository.Data.DbContexts;

namespace GradingManagementSystem.APIs.Controllers
{    
    [ApiController]
    [Route("api/[controller]")]
    public class UserProfileController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly GradingManagementSystemDbContext _dbContext;

        public UserProfileController(IUserProfileService userProfileService, IUnitOfWork unitOfWork, GradingManagementSystemDbContext dbContext)
        {
            _userProfileService = userProfileService;
            _unitOfWork = unitOfWork;
            _dbContext = dbContext;
        }

        [HttpGet("GetProfile")]
        [Authorize(Roles = "Student, Doctor, Admin")]
        public async Task<IActionResult> GetProfile()
        {
            //var userTimezone = _Request.Headers["X-Timezone"].ToString();
            //var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(userTimezone);
            //var userTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneInfo);

            var timezoneId = Request.Headers["X-User-Timezone"].FirstOrDefault();
            if (string.IsNullOrEmpty(timezoneId))
                return BadRequest(new ApiResponse(400, "Timezone is invalid.", new { IsSuccess = false }));


            var userId = User.FindFirst("UserId")?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiResponse(401, "Unauthorized user.", new { IsSuccess = false }));
            if (string.IsNullOrEmpty(userRole))
                return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));

            var profileResponse = await _userProfileService.GetUserProfileAsync(userId, userRole, timezoneId);

            if (profileResponse.StatusCode == 401)
                return Unauthorized(profileResponse);
            if (profileResponse.StatusCode == 404)
                return NotFound(profileResponse);
            if (profileResponse.StatusCode == 400)
                return BadRequest(profileResponse);

            return Ok(profileResponse);
        }

        [HttpPut("ChangeUsername")]
        [Authorize(Roles = "Student, Doctor, Admin")]
        public async Task<IActionResult> ChangeUsername([FromBody] ChangeUsernameDto model)
        {
            if (model is null)
                return BadRequest(new ApiResponse(400, "Invalid input data.", new { IsSuccess = false }));
            if (string.IsNullOrEmpty(model.NewUsername))
                return BadRequest(new ApiResponse(400, "NewUsername invalid.", new { IsSuccess = false }));

            var userId = User.FindFirst("UserId")?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiResponse(401, "Unauthorized user.", new { IsSuccess = false }));
            if (string.IsNullOrEmpty(userRole))
                return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));

            var responseResult = await _userProfileService.ChangeUsernameAsync(model.NewUsername, userId, userRole);

            if (responseResult.StatusCode == 400)
                return BadRequest(responseResult);
            if (responseResult.StatusCode == 401)
                return Unauthorized(responseResult);
            if (responseResult.StatusCode == 404)
                return NotFound(responseResult);

            return Ok(responseResult);
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
                return Unauthorized(new ApiResponse(401, "Unauthorized user.", new { IsSuccess = false }));
            if (string.IsNullOrEmpty(userRole))
                return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));

            var responseResult = await _userProfileService.ChangePasswordAsync(model.CurrentPassword, model.NewPassword, userId);

            if (responseResult.StatusCode == 400)
                return BadRequest(responseResult);
            if (responseResult.StatusCode == 401)
                return Unauthorized(responseResult);
            if (responseResult.StatusCode == 404)
                return NotFound(responseResult);

            return Ok(responseResult);
        }

        [HttpPut("ChangeProfilePicture")]
        [Authorize(Roles = "Student, Doctor, Admin")]
        public async Task<IActionResult> ChangeProfilePicture([FromForm] ChangeProfilePictureDto model)
        {
            if (model.ProfilePicture == null || model.ProfilePicture.Length == 0)
                return BadRequest(new ApiResponse(400, "Profile picture is invalid.", new { IsSuccess = false }));

            var userId = User.FindFirst("UserId")?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiResponse(401, "Unauthorized user.", new { IsSuccess = false }));
            if (string.IsNullOrEmpty(userRole))
                return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));

            var response = await _userProfileService.ChangeProfilePictureAsync(model.ProfilePicture, userId, userRole);

            if (response.StatusCode == 404)
                return NotFound(response);
            if (response.StatusCode == 400)
                return BadRequest(response);

            return Ok(response);
        }
    }
}
