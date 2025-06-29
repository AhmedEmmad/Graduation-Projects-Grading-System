﻿using GradingManagementSystem.Core;
using GradingManagementSystem.Core.CustomResponses;
using GradingManagementSystem.Core.Entities;
using GradingManagementSystem.Core.Entities.Identity;
using GradingManagementSystem.Core.Repositories.Contact;
using GradingManagementSystem.Core.Services.Contact;
using GradingManagementSystem.Repository.Data.DbContexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GradingManagementSystem.Service
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly GradingManagementSystemDbContext _dbContext;
        private readonly IWebHostEnvironment _env;

        public UserProfileService(IUserProfileRepository userProfileRepository,
                                  IUnitOfWork unitOfWork,
                                  IConfiguration configuration,
                                  GradingManagementSystemDbContext dbContext,
                                  IWebHostEnvironment env)

        {
            _userProfileRepository = userProfileRepository;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _dbContext = dbContext;
            _env = env;
        }

        public async Task<ApiResponse> GetUserProfileAsync(string userId, string userRole)
        {
            var currentTime = DateTime.Now;
            var activeAppointment = await _dbContext.AcademicAppointments
                                                     .Where(a => a.Status == StatusType.Active.ToString())
                                                     .FirstOrDefaultAsync();

            object? userProfile = null;
            if (userRole == "Admin")
                userProfile = await _userProfileRepository.GetAdminProfileAsync(userId, activeAppointment, currentTime);
            else if (userRole == "Doctor")
                userProfile = await _userProfileRepository.GetDoctorProfileAsync(userId, activeAppointment, currentTime);
            else if (userRole == "Student")
                userProfile = await _userProfileRepository.GetStudentProfileAsync(userId, activeAppointment, currentTime);
            else
                return new ApiResponse(400, "Invalid user role.", new { IsSuccess = false });

            if (userProfile == null)
                return new ApiResponse(404, "User profile not found.", new { IsSuccess = false });
            return new ApiResponse(200, "User profile retrieved successfully.", userProfile);
        }

        public async Task<ApiResponse> ChangeUsernameAsync(string newUsername, string userId, string userRole)
        {
            var existingUser = await _userProfileRepository.GetAppUserAsync(userId);
            if (existingUser == null)
                return new ApiResponse(404, "User not found.", new { IsSuccess = false });

            if (existingUser.FullName == newUsername)
                return new ApiResponse(400, "New username is the same as the current one, Please choose another one.", new { IsSuccess = false });

            if (userRole == "Admin")
            {
                var admin = await _unitOfWork.Repository<Admin>().FindAsync(a => a.AppUserId == userId);
                if (admin.FullName == newUsername)
                    return new ApiResponse(400, "New username is the same as the current one, Please choose another one.", new { IsSuccess = false });
                existingUser.FullName = newUsername;
                admin.FullName = newUsername;
                _userProfileRepository.Update(existingUser);
                _unitOfWork.Repository<Admin>().Update(admin);
            }
            else if (userRole == "Doctor")
            {
                var doctor = await _unitOfWork.Repository<Doctor>().FindAsync(d => d.AppUserId == userId);
                if (doctor.FullName == newUsername)
                    return new ApiResponse(400, "New username is the same as the current one, Please choose another one.", new { IsSuccess = false });
                existingUser.FullName = newUsername;
                doctor.FullName = newUsername;
                _userProfileRepository.Update(existingUser);
                _unitOfWork.Repository<Doctor>().Update(doctor);
            }
            else if (userRole == "Student")
            {
                var student = await _unitOfWork.Repository<Student>().FindAsync(s => s.AppUserId == userId);
                if (student.FullName == newUsername)
                    return new ApiResponse(400, "New username is the same as the current one, Please choose another one.", new { IsSuccess = false });
                existingUser.FullName = newUsername;
                student.FullName = newUsername;
                _userProfileRepository.Update(existingUser);
                _unitOfWork.Repository<Student>().Update(student);
            }
            else
            {
                return new ApiResponse(400, "Invalid user role.", new { IsSuccess = false });
            }
            await _unitOfWork.CompleteAsync();
            return new ApiResponse(200, "Username changed successfully.", new { IsSuccess = true });
        }

        public async Task<ApiResponse> ChangePasswordAsync(string oldPassword, string newPassword, string userId)
        {
            var existingUser = await _userProfileRepository.GetAppUserAsync(userId);
            if (existingUser == null)
                return new ApiResponse(404, "User not found.", new { IsSuccess = false });

            if (oldPassword == newPassword)
                return new ApiResponse(400, "New password is the same as the current one. Please choose another one.", new { IsSuccess = false });

            var passwordHasher = new PasswordHasher<AppUser>();
            var passwordVerification = passwordHasher.VerifyHashedPassword(existingUser, existingUser.PasswordHash, oldPassword);
            if (passwordVerification == PasswordVerificationResult.Failed)
                return new ApiResponse(400, "Old password is incorrect.", new { IsSuccess = false });

            existingUser.PasswordHash = passwordHasher.HashPassword(existingUser, newPassword);
            _userProfileRepository.Update(existingUser);
            await _unitOfWork.CompleteAsync();

            return new ApiResponse(200, "Password changed successfully.", new { IsSuccess = true });
        }

        public async Task<ApiResponse> ChangeProfilePictureAsync(IFormFile newProfilePicture, string userId, string userRole)
        {
            var existingUser = await _userProfileRepository.GetAppUserAsync(userId);
            if (existingUser == null)
                return new ApiResponse(404, "User not found.", new { IsSuccess = false });

            var profilePicturePath = string.Empty;
            var uploadsFolder = string.Empty;
            if (userRole == "Admin")
                uploadsFolder = Path.Combine(_env.WebRootPath, "Admins/ProfilePictures");
            else if (userRole == "Doctor")
                uploadsFolder = Path.Combine(_env.WebRootPath, "Doctors/ProfilePictures");
            else
                uploadsFolder = Path.Combine(_env.WebRootPath, "Students/ProfilePictures");

            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{newProfilePicture.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await newProfilePicture.CopyToAsync(stream);
            }

            if (userRole == "Admin")
                profilePicturePath = $"{_configuration["ApiBaseUrl"]}Admins/ProfilePictures/{uniqueFileName}";
            else if (userRole == "Doctor")
                profilePicturePath = $"{_configuration["ApiBaseUrl"]}Doctors/ProfilePictures/{uniqueFileName}";
            else
                profilePicturePath = $"{_configuration["ApiBaseUrl"]}Students/ProfilePictures/{uniqueFileName}";

            existingUser.ProfilePicture = profilePicturePath;
            _userProfileRepository.Update(existingUser);
            await _unitOfWork.CompleteAsync();

            return new ApiResponse(200, "Profile picture changed successfully.", new { IsSuccess = true, ProfilePicturePath = profilePicturePath });
        }
    }
}
