using GradingManagementSystem.Core.DTOs;
using GradingManagementSystem.Core.Entities.Identity;

namespace GradingManagementSystem.Core.Repositories.Contact
{
    public interface IUserProfileRepository : IGenericRepository<AppUser>
    {
        Task<AdminProfileDto?> GetAdminProfileAsync(string userId); 
        Task<DoctorProfileDto?> GetDoctorProfileAsync(string userId); 
        Task<StudentProfileDto?> GetStudentProfileAsync(string userId);
        Task<AppUser?> GetAppUserAsync(string userId);
    }
}