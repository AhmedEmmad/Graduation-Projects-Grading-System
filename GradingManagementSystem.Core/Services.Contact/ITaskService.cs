using GradingManagementSystem.Core.CustomResponses;
using GradingManagementSystem.Core.Entities;

namespace GradingManagementSystem.Core.Services.Contact
{
    public interface ITaskService
    {
        Task<ApiResponse> CreateTaskAsync(TaskItem task, List<int> studentIds);
        Task<ApiResponse> GetAllTeamTasksByTeamIdAsync(int teamId);
    }
}
