using GradingManagementSystem.Core;
using GradingManagementSystem.Core.CustomResponses;
using GradingManagementSystem.Core.DTOs;
using GradingManagementSystem.Core.Entities;
using GradingManagementSystem.Core.Entities.Identity;
using GradingManagementSystem.Repository.Data.DbContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using IAuthenticationService = GradingManagementSystem.Core.Services.Contact.IAuthenticationService;

namespace GradingManagementSystem.APIs.Controllers
{
    // This class handles HTTP requests and responses
    [Route("api/Auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authService;
        private readonly GradingManagementSystemDbContext _dbContext;
        private readonly UserManager<AppUser> _userManager; // Provides the Helper Methods for User Management
        private readonly IUnitOfWork _unitOfWork;

        public AuthenticationController(IAuthenticationService authService,
                                        UserManager<AppUser> userManager,
                                        GradingManagementSystemDbContext dbContext,
                                        IUnitOfWork unitOfWork)
        {
            _authService = authService;
            _userManager = userManager;
            _dbContext = dbContext;
            _unitOfWork = unitOfWork;
        }

        // Student Registration/Creation Flow/Logic
        [HttpPost("StudentRegister")]
        public async Task<IActionResult> StudentRegister([FromForm] StudentRegisterDto model)
        {
            if (model is null)
                return BadRequest(new ApiResponse(400, "Invalid input data."));

            var returnedRespone = await _authService.RegisterStudentAsync(model);

            if (returnedRespone.StatusCode == 400)
                return BadRequest(new { statusCode = returnedRespone.StatusCode, message = returnedRespone.Message, data = returnedRespone.Data });

            return Ok(returnedRespone);
        }

        // Doctor Registration Flow
        [HttpPost("DoctorRegister")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DoctorRegister([FromBody] DoctorRegisterDto model)
        {
            if (model is null)
                return BadRequest(new ApiResponse(400, "Invalid input data."));

            ApiResponse returnedRespone = await _authService.RegisterDoctorAsync(model);

            if (returnedRespone.StatusCode == 400)
                return BadRequest(new { statusCode = returnedRespone.StatusCode, message = returnedRespone.Message, data = returnedRespone.Data  });

            return Ok(returnedRespone);
        }

        // User Login Flow
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (model is null)
                return BadRequest(new ApiResponse(400, "Invalid input data."));

            ApiResponse returnedRespone = await _authService.LoginAsync(model);

            if (returnedRespone.StatusCode == 400)
                return BadRequest(new { statusCode = returnedRespone.StatusCode, message = returnedRespone.Message, data = returnedRespone.Data });

            if(returnedRespone.StatusCode == 401)
                return Unauthorized(new { statusCode = returnedRespone.StatusCode, message = returnedRespone.Message, data = returnedRespone.Data });

            return Ok(returnedRespone);
        }

        // User ForgetPassword Flow
        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordDto model)
        {
            if (model is null)
                return BadRequest(new ApiResponse(400, "Invalid input data."));

            ApiResponse returnedRespone = await _authService.ForgetPasswordAsync(model);

            if (returnedRespone.StatusCode == 400)
                return BadRequest(new { statusCode = returnedRespone.StatusCode, message = returnedRespone.Message, data = returnedRespone.Data });

            return Ok(returnedRespone);
        }

        // User ResetPassword Flow
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            if (model is null)
                return BadRequest(new ApiResponse(400, "Invalid input data."));

            ApiResponse returnedRespone = await _authService.ResetPasswordAsync(model);

            if (returnedRespone.StatusCode == 400)
                return BadRequest(new { statusCode = returnedRespone.StatusCode, message = returnedRespone.Message, data = returnedRespone.Data });

            return Ok(returnedRespone);
        }

        [HttpPost("EmailVerificationByOtp/{otpCode}")]
        public async Task<IActionResult> VerifyEmailByOTP(string otpCode)
        {
            var result = await _authService.VerifyEmailByOTPAsync(otpCode);

            if (result.StatusCode == 400)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
