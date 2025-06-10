using GradingManagementSystem.Core.CustomResponses;
using GradingManagementSystem.Core.DTOs;
using GradingManagementSystem.Core.Entities;
using GradingManagementSystem.Core.Entities.Identity;
using GradingManagementSystem.Core.Repositories.Contact;
using GradingManagementSystem.Repository.Data.DbContexts;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace GradingManagementSystem.Repository
{
    public class UserProfileRepository : GenericRepository<AppUser>, IUserProfileRepository
    {
        private readonly GradingManagementSystemDbContext _dbContext;

        public UserProfileRepository(GradingManagementSystemDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AdminProfileDto?> GetAdminProfileAsync(string userId, AcademicAppointment? currentAppointment, DateTime currentTime)
        {
            var adminUser = await _dbContext.Users.Include(u => u.Admin).FirstOrDefaultAsync(u => u.Id == userId);
            if (adminUser == null)
                return null;

            return new AdminProfileDto
            {
                Id = adminUser.Admin.Id,
                FullName = adminUser.FullName,
                Email = adminUser.Email,
                EnrollmentDate = adminUser.Admin.EnrollmentDate,
                Role = "Admin",
                ProfilePicture = adminUser.ProfilePicture,
            };
        }

        public async Task<DoctorProfileDto?> GetDoctorProfileAsync(string userId, AcademicAppointment? currentAppointment, DateTime currentTime)
        {
            var doctorUser = await _dbContext.Users.Include(u => u.Doctor).FirstOrDefaultAsync(u => u.Id == userId);
            if (doctorUser == null)
                return null;

            return new DoctorProfileDto
            {
                Id = doctorUser.Doctor.Id,
                FullName = doctorUser.FullName,
                Email = doctorUser.Email,
                Role = "Doctor",
                EnrollmentDate = doctorUser.Doctor.EnrollmentDate,
                ProfilePicture = doctorUser.ProfilePicture,
            };
        }

        public async Task<StudentProfileDto?> GetStudentProfileAsync(string userId, AcademicAppointment? currentAppointment, DateTime currentTime)
        {

            if (currentAppointment == null)
                return null;

            if (currentAppointment.FirstTermStart > currentTime || currentAppointment.SecondTermEnd < currentTime)
                return null;

            var studentUser = await _dbContext.Users
                .Include(u => u.Student)
                .ThenInclude(s => s.Team) // Ensure Team is included to avoid null reference
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (studentUser == null || studentUser.Student == null)
                return null;

            string currentTerm = string.Empty;
            if (currentAppointment?.FirstTermStart <= currentTime && currentAppointment?.FirstTermEnd >= currentTime)
                currentTerm = "First-Term";
            else if (currentAppointment?.SecondTermStart <= currentTime && currentAppointment?.SecondTermEnd >= currentTime)
                currentTerm = "Second-Term";
            else
                currentTerm = "Invalid-Term";

            return new StudentProfileDto
            {
                Id = studentUser.Student.Id,
                FullName = studentUser.FullName,
                Email = studentUser.Email,
                Role = "Student",
                ProfilePicture = studentUser.ProfilePicture,
                EnrollmentDate = studentUser.Student.EnrollmentDate,
                TeamId = studentUser.Student.TeamId,
                HasProject = studentUser.Student.Team?.HasProject ?? false, // Handle null Team
                LeaderOfTeamId = studentUser.Student.LeaderOfTeamId,
                InTeam = studentUser.Student.InTeam,
                Specialty = studentUser.Specialty,
                CurrentAcademicYear = currentAppointment?.Year,
                CurrentAcademicTerm = currentTerm,
            };
        }

        public async Task<AppUser?> GetAppUserAsync(string userId)
        {
            var appUser = await _dbContext.Users
                .Include(u => u.Doctor)
                .Include(u => u.Student)
                .Include(u => u.Admin)
                .FirstOrDefaultAsync(u => u.Id == userId);
            return appUser;
        }
    }
}
