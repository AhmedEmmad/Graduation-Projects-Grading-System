using GradingManagementSystem.Core.DTOs;
using GradingManagementSystem.Core.Entities;

namespace GradingManagementSystem.Core.Repositories.Contact
{
    public interface ITeamRepository : IGenericRepository<Team>
    {
        Task<IEnumerable<TeamWithMembersDto>> GetAllTeamsForDoctorAsync(int doctorId);
    }
}
