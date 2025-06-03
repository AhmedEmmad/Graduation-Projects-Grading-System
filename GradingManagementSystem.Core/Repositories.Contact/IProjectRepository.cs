using GradingManagementSystem.Core.DTOs;
using GradingManagementSystem.Core.Entities;

namespace GradingManagementSystem.Core.Repositories.Contact
{
    public interface IProjectRepository
    {
        Task<IEnumerable<DoctorProjectIdeaDto>?> GetDoctorProjectIdeasByStatusAndDoctorIdIfNeededAsync(string status, int? doctorId = null);
        Task<IEnumerable<TeamProjectIdeaDto>?> GetTeamProjectIdeasByStatusAsync(string status);
        Task<IEnumerable<TeamRequestForDoctorProjectIdeaDto>?> GetPendingTeamRequestsForDoctorProjectIdeasAsync(int doctorId);
        Task<IEnumerable<FinalProjectIdeaForAdminDto>?> GetAllFinalProjectIdeas();
    }
}