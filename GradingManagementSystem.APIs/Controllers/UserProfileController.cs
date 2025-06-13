using GradingManagementSystem.Core.DTOs;
using GradingManagementSystem.Core.CustomResponses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GradingManagementSystem.Core.Services.Contact;

namespace GradingManagementSystem.APIs.Controllers
{    
    [ApiController]
    [Route("api/[controller]")]
    public class UserProfileController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;

        public UserProfileController(IUserProfileService userProfileService)
        {
            _userProfileService = userProfileService;
        }

        // Finished / Reviewed / Tested / Edited / D
        [HttpGet("GetProfile")]
        [Authorize(Roles = "Student, Doctor, Admin")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst("UserId")?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiResponse(401, "Unauthorized user.", new { IsSuccess = false }));
            if (string.IsNullOrEmpty(userRole))
                return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));

            var profileResponse = await _userProfileService.GetUserProfileAsync(userId, userRole);

            if (profileResponse.StatusCode == 401)
                return Unauthorized(profileResponse);
            if (profileResponse.StatusCode == 404)
                return NotFound(profileResponse);
            if (profileResponse.StatusCode == 400)
                return BadRequest(profileResponse);

            return Ok(profileResponse);
        }

        // Finished / Reviewed / Tested / Edited / D
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

        // Finished / Reviewed / Tested / Edited / D
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

        // Finished / Reviewed / Tested / Edited / D
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
