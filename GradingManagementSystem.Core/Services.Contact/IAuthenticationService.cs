using GradingManagementSystem.Core.CustomResponses;
using GradingManagementSystem.Core.DTOs;

namespace GradingManagementSystem.Core.Services.Contact
{
    public interface IAuthenticationService
    {
        Task<ApiResponse> RegisterStudentAsync(StudentRegisterDto model);
        Task<ApiResponse> RegisterDoctorAsync(DoctorRegisterDto model);
        Task<ApiResponse> LoginAsync(LoginDto model);
        Task<ApiResponse> ForgetPasswordAsync(ForgetPasswordDto model);
        Task<ApiResponse> ResetPasswordAsync(ResetPasswordDto model);
        Task<ApiResponse> VerifyEmailByOTPAsync(string otpCode);
        Task<ApiResponse> ResendOtpAsync(string studentEmail);
    }
}
