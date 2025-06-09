using GradingManagementSystem.Core;
using GradingManagementSystem.Core.CustomResponses;
using GradingManagementSystem.Core.DTOs;
using GradingManagementSystem.Core.Entities;
using GradingManagementSystem.Core.Entities.Identity;
using GradingManagementSystem.Core.Services.Contact;
using GradingManagementSystem.Repository.Data.DbContexts;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GradingManagementSystem.Service
{
    // This class handles business logic for authentication and user management
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager; // Provides the Helper Methods for User Management
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly GradingManagementSystemDbContext _dbContext;

        public AuthenticationService(IUnitOfWork unitOfWork, 
                                     UserManager<AppUser> userManager, 
                                     IConfiguration configuration,
                                     IEmailService emailService,
                                     SignInManager<AppUser> signInManager,
                                     ITokenService tokenService,
                                     GradingManagementSystemDbContext dbContext)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _configuration = configuration;
            _emailService = emailService;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _dbContext = dbContext;
        }

        // Finished / Tested
        public async Task<ApiResponse> RegisterStudentAsync(StudentRegisterDto model)
        {
            var existingAccount = await _userManager.FindByEmailAsync(model.Email);
            if (existingAccount != null)
                return new ApiResponse(400, $"This email '{model.Email}' is already taken or registered, Please register with another email.", new { IsSuccess = false });

            var existingTemporaryUser = await _dbContext.TemporaryUsers.FirstOrDefaultAsync(u => u.Email == model.Email);

            // Student registered recently but not verified an email
            if (existingTemporaryUser != null)
            {
                var existingOtpCode = await _dbContext.UserOtps.FirstOrDefaultAsync(o => o.Email == model.Email);

                if (existingOtpCode != null)
                    _dbContext.UserOtps.Remove(existingOtpCode);

                await _dbContext.SaveChangesAsync();

                string newOtp = OtpGenerator.GenerateOtp();
                DateTime newExpiration = DateTime.Now.AddMinutes(5);
                var newOtpVerification = new UserOtp
                {
                    Email = model.Email,
                    OtpCode = newOtp,
                    ExpiryTime = newExpiration
                };
                await _dbContext.UserOtps.AddAsync(newOtpVerification);
                await _dbContext.SaveChangesAsync();

                await _emailService.SendEmailAsync(model.Email, "Welcome, Your OTP Code Verification",
                    $"<p>Hi {model.FullName},</p>" +
                    $"<p>Welcome! We're excited to have you join us.</p>" +
                    $"<p>Your One-Time Password (OTP) for email verification is:</p>" +
                    $"<h2 style='color: #00bfff;'>{newOtp}</h2>");

                return new ApiResponse(200, $"Your OTP Code Verification has been resent to your email until expiry time: '{newOtpVerification.ExpiryTime}'. Check it now", new { IsSuccess = true });
            }
            
            string profilePicturePath = "";
            if (model.ProfilePicture != null)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Students/ProfilePictures");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{model.ProfilePicture.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfilePicture.CopyToAsync(stream);
                }

                profilePicturePath = $"{_configuration["ApiBaseUrl"]}Students/ProfilePictures/{uniqueFileName}";
            }

            var newTemporaryUser = new TemporaryUser
            {
                FullName = model.FullName,
                Email = model.Email,
                PasswordHash = model.Password,
                ProfilePicture = profilePicturePath,
                Specialty = model.Specialty
            };

            await _dbContext.TemporaryUsers.AddAsync(newTemporaryUser);

            string newOtpCode = OtpGenerator.GenerateOtp();
            DateTime newExpirationTime = DateTime.Now.AddMinutes(5);

            var newOtpCodeVerification = new UserOtp
            {
                Email = model.Email,
                OtpCode = newOtpCode,
                ExpiryTime = newExpirationTime
            };

            await _dbContext.UserOtps.AddAsync(newOtpCodeVerification);
            await _dbContext.SaveChangesAsync();

            await _emailService.SendEmailAsync(model.Email, $"Welcome {model.FullName}!",
                        $"<p>Hi {model.FullName},</p>" +
                        $"<p>We’re thrilled to have you here! To complete your registration, Please verify your email using the otp code below:</p>" +
                        $"<h2 style='color: #00bfff;'>{newOtpCode}</h2>");

            return new ApiResponse(200, $"Registration successful, The OTP Code Verification has been sent to your email until expiry time: '{newOtpCodeVerification.ExpiryTime}'. Check it now", new { IsSuccess = true });
        }

        // Finished / Tested
        public async Task<ApiResponse> RegisterDoctorAsync(DoctorRegisterDto model)
        {
            var existingDoctor = await _userManager.FindByEmailAsync(model.Email);
            if (existingDoctor != null)
                return new ApiResponse(400, $"This email \'{model.Email}\' is already taken or registered, Please register with another email", new { IsSuccess = false });
            
            var currentActiveAcademicYearAppointment = await _unitOfWork.Repository<AcademicAppointment>()
                .FindAsync(a => a.Status == "Active");
            if (currentActiveAcademicYearAppointment == null)
                return new ApiResponse(400, "No active academic year appointment found. Please wait until registration will open.", new { IsSuccess = false });

            var newDoctorAppUser = new AppUser
            {
                FullName = model.FullName,
                Email = model.Email,
                UserName = model.Email.Split('@')[0],
                //AcademicAppointmentId = currentActiveAcademicYearAppointment.Id,
            };

            var createDoctorResult = await _userManager.CreateAsync(newDoctorAppUser, model.Password);
            if (!createDoctorResult.Succeeded)
                return new ApiResponse(400, "User creation failed.", new { IsSuccess = false });

            var doctor = new Doctor
            {
                AppUserId = newDoctorAppUser.Id,
                FullName = newDoctorAppUser.FullName,
                Email = newDoctorAppUser.Email
            };

            await _unitOfWork.Repository<Doctor>().AddAsync(doctor);
            await _userManager.AddToRoleAsync(newDoctorAppUser, "Doctor");
            await _unitOfWork.CompleteAsync();
            return new ApiResponse(200, $"Doctor registered successfully.", new { IsSuccess = true });
        }

        // Finished / Tested
        public async Task<ApiResponse> LoginAsync(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user is null)
                return new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false });

            var userResult = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

            if (!userResult.Succeeded)
                return new ApiResponse(401, "Incorrect email or password.", new { IsSuccess = false });

            var token = await _tokenService.CreateTokenAsync(user);
            
            return new ApiResponse(200, "Login successfully.", new { IsSuccess = true, Token = token });
        }

        // Finished / Tested
        public async Task<ApiResponse> ForgetPasswordAsync(ForgetPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null)
                return new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false });

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            if (resetToken == null || resetToken.Trim().Length == 0)
                    return new ApiResponse(400, "Failed to generate reset token.", new { IsSuccess = false });

            var resetLink = $"https://graduation-project-angular.vercel.app/reset-password?userEmail={model.Email}&token={resetToken}";

            var emailBody = $@"
                                <html>
                                <body>
                                    <h4>Reset Your Password</h4>
                                    <p>Click the link below to reset your password:</p>
                                    <a href='{resetLink}' style='text-decoration: none; color: blue;'>Reset Password</a>
                                </body>
                                </html>
                            ";

            await _emailService.SendEmailAsync(model.Email, "Reset Your Password", emailBody);

            return new ApiResponse(200, "Password reset link has been sent to your email.", new { IsSuccess = true }); 
        }

        // Finished / Tested
        public async Task<ApiResponse> ResetPasswordAsync(ResetPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user is null)
                return new ApiResponse(404, "Account Not Found.", new { IsSuccess = false });

            if(model.NewPassword != model.ConfirmPassword)
                return new ApiResponse(400, "New password and confirm password are not matched.", new { IsSuccess = false });

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.ConfirmPassword);

            if (!result.Succeeded)
                return new ApiResponse(400, "Password reset failed, Please check the token or try again later.", new { IsSuccess = false });

            return new ApiResponse(200, "Password reset successfully.", new { IsSuccess = true });
        }

        // Finished / Tested
        public async Task<ApiResponse> VerifyEmailByOTPAsync(string otpCode)
        {
            if(string.IsNullOrEmpty(otpCode))
                return new ApiResponse(400, "Invalid input OTP Code.", new { IsSuccess = false });

            var existingOtpCode = await _unitOfWork.Repository<UserOtp>().FindAsync(o => o.OtpCode == otpCode);
            if (existingOtpCode == null)
                return new ApiResponse(404, "OTP Code not found.", new { IsSuccess = false });

            if (existingOtpCode.ExpiryTime < DateTime.Now)
                return new ApiResponse(400, "OTP Code has expired.", new { IsSuccess = false });

            var existingTemporaryUser = await _unitOfWork.Repository<TemporaryUser>().FindAsync(u => u.Email == existingOtpCode.Email);
            if (existingTemporaryUser == null)
                return new ApiResponse(404, "Account not registered recently.", new { IsSuccess = false });

            _unitOfWork.Repository<UserOtp>().Delete(existingOtpCode);
            await _unitOfWork.CompleteAsync();

            var currentActiveAcademicYearAppointment = await _unitOfWork.Repository<AcademicAppointment>()
                .FindAsync(a => a.Status == "Active");
            if (currentActiveAcademicYearAppointment == null)
                return new ApiResponse(400, "No active academic year appointment found. Please wait until registration will open.", new { IsSuccess = false });

            var newStudentAppUser = new AppUser
            {
                FullName = existingTemporaryUser.FullName,
                Email = existingTemporaryUser.Email,
                UserName = existingTemporaryUser.Email.Split('@')[0],
                ProfilePicture = existingTemporaryUser.ProfilePicture,
                EmailConfirmed = true,
                Specialty = existingTemporaryUser?.Specialty?.ToUpper(),
                //AcademicAppointmentId = currentActiveAcademicYearAppointment.Id,
            };

            var StudentCreatedResult = await _userManager.CreateAsync(newStudentAppUser, existingTemporaryUser.PasswordHash);
            if (!StudentCreatedResult.Succeeded)
                return new ApiResponse(400, "User Creation Failed.", new { IsSuccess = false });

            await _userManager.AddToRoleAsync(newStudentAppUser, "Student");
            var newStudent = new Student
            {
                FullName = newStudentAppUser.FullName,
                Email = newStudentAppUser.Email,
                Specialty = newStudentAppUser.Specialty ?? string.Empty,
                AppUserId = newStudentAppUser.Id,
                AcademicAppointmentId = currentActiveAcademicYearAppointment.Id,

            };
            await _unitOfWork.Repository<Student>().AddAsync(newStudent);
            await _unitOfWork.CompleteAsync();

            _unitOfWork.Repository<TemporaryUser>().Delete(existingTemporaryUser);
            await _unitOfWork.CompleteAsync();
            return new ApiResponse(200, $"Email verified and your account created successfully.", new { IsSuccess = true });
        }

        // Finished / Tested
        public async Task<ApiResponse> ResendOtpAsync(string studentEmail)
        {
            var existingTemporaryUser = await _dbContext.TemporaryUsers.FirstOrDefaultAsync(t => t.Email == studentEmail);
            if (existingTemporaryUser == null)
                return new ApiResponse(404, $"This '{studentEmail}' is not registered before that.", new { IsSuccess = false });

            var existingOtp = await _dbContext.UserOtps.FirstOrDefaultAsync(u => u.Email == existingTemporaryUser.Email);
            if(existingOtp != null)
                _dbContext.UserOtps.Remove(existingOtp);

            await _dbContext.SaveChangesAsync();

            string newOtpCode = OtpGenerator.GenerateOtp();
            DateTime newExpirationTime = DateTime.Now.AddMinutes(5);

            var newOtpCodeVerification = new UserOtp
            {
                Email = existingTemporaryUser.Email,
                OtpCode = newOtpCode,
                ExpiryTime = newExpirationTime
            };

            await _dbContext.UserOtps.AddAsync(newOtpCodeVerification);
            await _dbContext.SaveChangesAsync();

            await _emailService.SendEmailAsync(existingTemporaryUser.Email, "Welcome, Your OTP Code Verification",
                    $"<p>Hi {existingTemporaryUser.FullName},</p>" +
                    $"<p>Welcome! We're excited to have you join us.</p>" +
                    $"<p>Your One-Time Password (OTP) for email verification is:</p>" +
                    $"<h2 style='color: #00bfff;'>{newOtpCode}</h2>");

            return new ApiResponse(200, $"Your OTP Code Verification has been resent to your email until expiry time: '{newOtpCodeVerification.ExpiryTime}'. Check it now", new { IsSuccess = true });
        }
    }
}
