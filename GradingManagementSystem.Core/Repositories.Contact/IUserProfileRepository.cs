using GradingManagementSystem.Core.DTOs;
using GradingManagementSystem.Core.Entities;
using GradingManagementSystem.Core.Entities.Identity;

namespace GradingManagementSystem.Core.Repositories.Contact
{
    public interface IUserProfileRepository : IGenericRepository<AppUser>
    {
        Task<AdminProfileDto?> GetAdminProfileAsync(string userId, AcademicAppointment? currentAppointment, DateTime currentTime); 
        Task<DoctorProfileDto?> GetDoctorProfileAsync(string userId, AcademicAppointment? currentAppointment, DateTime currentTime); 
        Task<StudentProfileDto?> GetStudentProfileAsync(string userId, AcademicAppointment? currentAppointment, DateTime currentTime);
        Task<AppUser?> GetAppUserAsync(string userId);
    }
}