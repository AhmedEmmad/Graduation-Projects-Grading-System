using GradingManagementSystem.Core.DTOs;
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

        public async Task<AdminProfileDto> GetAdminProfileAsync(string userId)
        {
            var adminUser = await _dbContext.Users.Include(u => u.Admin).FirstOrDefaultAsync(u => u.Id ==  userId);
            if (adminUser == null)
                return null;

            return new AdminProfileDto
            {
                Id = adminUser.Admin.Id,
                FullName = adminUser.FullName,
                Email = adminUser.Email,
                EnrollmentDate = adminUser.Admin.EnrollmentDate,
                Role = "Admin"
            };
        }

        public async Task<DoctorProfileDto> GetDoctorProfileAsync(string userId)
        {
            var doctorUser = await _dbContext.Users.Include(u => u.Doctor).FirstOrDefaultAsync(u => u.Id == userId);

            return new DoctorProfileDto
            {
                Id = doctorUser.Doctor.Id,
                FullName = doctorUser.FullName,
                Email = doctorUser.Email,
                EnrollmentDate = doctorUser.Doctor.EnrollmentDate,
                Role = "Doctor",
                Teams = doctorUser.Doctor.Teams.Select(t => new TeammDto
                {
                    TeamId = t.Id,
                    TeamName = t.Name,
                    TeamLeaderId = t.LeaderId,
                }).ToHashSet()
            };
        }

        public async Task<StudentProfileDto> GetStudentProfileAsync(string userId)
        {
            var studentUser = await _dbContext.Users.Include(u => u.Student).FirstOrDefaultAsync(u => u.Id == userId);

            return new StudentProfileDto
            {
                Id = studentUser.Student.Id,
                FullName = studentUser.FullName,
                Email = studentUser.Email,
                Specialty = studentUser.Specialty,
                InTeam = studentUser.Student.InTeam,
                EnrollmentDate = studentUser.Student.EnrollmentDate,
                TeamId = studentUser.Student.TeamId,
                LeaderOfTeamId = studentUser.Student.LeaderOfTeamId,
                ProfilePicture = studentUser.ProfilePicture,
                Role = "Student"
            };
        }

        public async Task<AppUser> GetAppUserAsync(string userId)
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
