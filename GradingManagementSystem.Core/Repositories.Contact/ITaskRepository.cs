using GradingManagementSystem.Core.Entities;

namespace GradingManagementSystem.Core.Repositories.Contact
{
    public interface ITaskRepository : IGenericRepository<TaskItem>
    {
        Task<IEnumerable<TaskItem>> GetTeamTasksByTeamIdAsync(int teamId);
        Task<IEnumerable<TaskMember>> GetTaskMembersByTeamAsync(int teamId);
    }
}
