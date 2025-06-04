using GradingManagementSystem.Core.CustomResponses;
using GradingManagementSystem.Core.DTOs;

namespace GradingManagementSystem.Core.Services.Contact
{
    public interface IAcademicAppointmentService
    {
        Task<ApiResponse> CreateNewAcademicAppointmentAsync(CreateAcademicAppointmentDto model);
        Task<ApiResponse> GetAllAcademicYearAppointmentsAsync();
        Task<ApiResponse> SetNewAcademicYearAppointmentAsync(SetActiveYearDto model);
    }
}
