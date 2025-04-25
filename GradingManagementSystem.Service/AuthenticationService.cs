using GradingManagementSystem.Core;
using GradingManagementSystem.Core.CustomResponses;
using GradingManagementSystem.Core.DTOs;
using GradingManagementSystem.Core.Entities;
using GradingManagementSystem.Core.Entities.Identity;
using GradingManagementSystem.Core.Services.Contact;
using GradingManagementSystem.Repository.Data.DbContexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GradingManagementSystem.Service
{
    // This class handles business logic for authentication and user management
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
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

        public async Task<ApiResponse> RegisterStudentAsync(StudentRegisterDto model)
        {
            var existAccount = await _userManager.FindByEmailAsync(model.Email);
            if (existAccount is not null)
                return new ApiResponse(400, $"This email \'{model.Email}\' is already taken/registered, Please register with another email.", new { IsSuccess = false });

            //var existTemporaryUser = await _unitOfWork.Repository<TemporaryUser>().FindAsync(U => U.Email == model.Email);
            var existTemporaryUser = await _dbContext.TemporaryUsers.FirstOrDefaultAsync(U => U.Email == model.Email);

            if (existTemporaryUser is not null)
            {
                //var existOtp = await _unitOfWork.Repository<UserOtp>().FindAsync(O => O.Email == model.Email && DateTime.Now < O.ExpiryTime);
                var existOtp = await _dbContext.UserOtps.FirstOrDefaultAsync(o => o.Email == model.Email && DateTime.Now < o.ExpiryTime);

                if (existOtp is not null)
                {
                    await _emailService.SendEmailAsync(model.Email, "Your OTP Code: ",
                           $@"
                           <p>Hello {model.FullName},</p>
                          <p>Here is your One-Time Password (OTP):</p>
                          <h1 style='color: #00bfff;'>{existOtp.OtpCode}</h1>
                          <p><strong>Note:</strong> This code is valid for the next <strong>5 minutes</strong>.</p>
                           ");
                    return new ApiResponse(200, "OTP has been resent to your email.", new { IsSuccess = true });
                }
                else
                {
                    string otp = OtpGenerator.GenerateOtp();
                    DateTime expirationTime = DateTime.Now.AddMinutes(5);
                    var otpVerification = new UserOtp
                    {
                        Email = model.Email,
                        OtpCode = otp,
                        ExpiryTime = expirationTime
                    };
                    await _dbContext.UserOtps.AddAsync(otpVerification);
                    await _dbContext.SaveChangesAsync();

                    await _emailService.SendEmailAsync(model.Email, "Your OTP for Registration",
                        $"<p>Dear {model.FullName},</p>" +
                        $"<p>Welcome! We're excited to have you join us.</p>" +
                        $"<p>Your One-Time Password (OTP) for email verification is:</p>" +
                        $"<h2 style='color: #00bfff;'>{otp}</h2>");

                    return new ApiResponse(200, "OTP has been resent to your email.", new {IsSuccess = true});
                }
            }
            string profilePicturePath = "";
            if (model.ProfilePicture is not null)
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
            var tempUser = new TemporaryUser
            {
                FullName = model.FullName,
                Email = model.Email,
                PasswordHash = model.Password,
                ProfilePicture = profilePicturePath,
                Specialty = model.Specialty
            };
            //await _unitOfWork.Repository<TemporaryUser>().AddAsync(tempUser);
            await _dbContext.TemporaryUsers.AddAsync(tempUser);

            string newOtp = OtpGenerator.GenerateOtp();
            DateTime newExpirationTime = DateTime.Now.AddMinutes(5);

            var newOtpVerification = new UserOtp
            {
                Email = model.Email,
                OtpCode = newOtp,
                ExpiryTime = newExpirationTime
            };
            //await _unitOfWork.Repository<UserOtp>().AddAsync(newOtpVerification);
            await _dbContext.UserOtps.AddAsync(newOtpVerification);
            //await _unitOfWork.CompleteAsync();
            await _dbContext.SaveChangesAsync();
            await _emailService.SendEmailAsync(model.Email, "Welcome",
                        $"<p>Hi {model.FullName},</p>" +
                        $"<p>We’re thrilled to have you here! To complete your registration, please verify your email using the code below:</p>" +
                        $"<h2 style='color: #00bfff;'>{newOtp}</h2>");

            return new ApiResponse(200, "Registration successful, The Verification code has been sent to your email.", new { IsSuccess = true });
        }

        public async Task<ApiResponse> RegisterDoctorAsync(DoctorRegisterDto model)
        {
            var existDoctor = await _userManager.FindByEmailAsync(model.Email);
            if (existDoctor is not null)
                return new ApiResponse(400, $"This email \'{model.Email}\' is already taken/registered, Please register with another email");

            var doctorAppUser = new AppUser
            {
                FullName = model.FullName,
                Email = model.Email,
                UserName = model.Email.Split('@')[0],
            };

            var createDoctorResult = await _userManager.CreateAsync(doctorAppUser, model.Password);
            if (!createDoctorResult.Succeeded)
                return new ApiResponse(400, "User creation failed.");

            var doctorId = doctorAppUser.Id;
            var doctor = new Doctor
            {
                AppUserId = doctorId,
                FullName = doctorAppUser.FullName,
                Email = doctorAppUser.Email
            };

            await _unitOfWork.Repository<Doctor>().AddAsync(doctor);
            await _userManager.AddToRoleAsync(doctorAppUser, "Doctor");
            await _unitOfWork.CompleteAsync();
            return new ApiResponse(200, "Doctor registered successfully.");
        }

        public async Task<ApiResponse> LoginAsync(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user is null)
                return new ApiResponse(401, "Account Not Found(Unauthorized).");

            var userResult = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

            if (!userResult.Succeeded)
                return new ApiResponse(401, "Incorrect password.");

            var token = await _tokenService.CreateTokenAsync(user);
            
            return new ApiResponse(200, "Login successfully.", new { Token = token });
        }

        public async Task<ApiResponse> ForgetPasswordAsync(ForgetPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null)
                return new ApiResponse(400, "Account Not Found.");

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            if (resetToken == null || resetToken.Trim().Length == 0)
                    return new ApiResponse(400, "Failed to generate reset token.");

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

            return new ApiResponse(200, "Password reset link has been sent to your email."); 
        }

        public async Task<ApiResponse> ResetPasswordAsync(ResetPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user is null)
                return new ApiResponse(404, "Account Not Found(Unauthorized).");

            if(model.NewPassword != model.ConfirmPassword)
                return new ApiResponse(400, "New password and confirm password are not matched.");

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.ConfirmPassword);

            if (!result.Succeeded)
                return new ApiResponse(400, "Password reset failed, Please check the token or try again later.");

            return new ApiResponse(200, "Password reset successfully.");
        }

        public async Task<ApiResponse> VerifyEmailByOTPAsync(string otpCode)
        {
            var otpRecord = await _unitOfWork.Repository<UserOtp>().FindAsync(O => O.OtpCode == otpCode);
            if (otpRecord == null)
                return new ApiResponse(400, "Invalid OTP.", new { IsSuccess = false });

            if (otpRecord.ExpiryTime < DateTime.Now)
                return new ApiResponse(400, "OTP has expired.", new { IsSuccess = false });

            var temporaryUser = await _unitOfWork.Repository<TemporaryUser>().FindAsync(U => U.Email == otpRecord.Email);
            if (temporaryUser == null)
                return new ApiResponse(400, "Incorrect Email.", new { IsSuccess = false });

            _unitOfWork.Repository<UserOtp>().Delete(otpRecord);
            await _unitOfWork.CompleteAsync();

            var studentAppUser = new AppUser
            {
                FullName = temporaryUser.FullName,
                Email = temporaryUser.Email,
                UserName = temporaryUser.Email.Split('@')[0],
                ProfilePicture = temporaryUser.ProfilePicture,
                EmailConfirmed = true,
                Specialty = temporaryUser?.Specialty?.ToUpper()
            };

            var createStudentResult = await _userManager.CreateAsync(studentAppUser, temporaryUser.PasswordHash);
            if (!createStudentResult.Succeeded)
                return new ApiResponse(400, "User Creation Failed.", new { IsSuccess = false });

            await _userManager.AddToRoleAsync(studentAppUser, "Student");
            var student = new Student
            {
                FullName = studentAppUser.FullName,
                Email = studentAppUser.Email,
                Specialty = studentAppUser.Specialty ?? string.Empty,
                AppUserId = studentAppUser.Id
            };
            await _unitOfWork.Repository<Student>().AddAsync(student);
            await _unitOfWork.CompleteAsync();

            _unitOfWork.Repository<TemporaryUser>().Delete(temporaryUser);
            await _unitOfWork.CompleteAsync();
            return new ApiResponse(200, "Email verified successfully and user created.", new { IsSuccess = true });
        }
    }
}
