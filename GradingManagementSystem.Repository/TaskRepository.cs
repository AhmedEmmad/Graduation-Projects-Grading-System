using GradingManagementSystem.Core.Entities;
using GradingManagementSystem.Core.Repositories.Contact;
using GradingManagementSystem.Repository.Data.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace GradingManagementSystem.Repository
{
    public class TaskRepository : GenericRepository<TaskItem>, ITaskRepository
    {
        private readonly GradingManagementSystemDbContext _dbContext;

        public TaskRepository(GradingManagementSystemDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<TaskItem>> GetTeamTasksByTeamIdAsync(int teamId)
        {
            return await _dbContext.Tasks.Where(t => t.TeamId == teamId)
                                         .Include(t => t.TaskMembers)
                                            .ThenInclude(tm => tm.Student)
                                            .ThenInclude(s => s.AppUser)
                                         .Include(t => t.Team)
                                         .Include(t => t.Supervisor)
                                         .AsNoTracking()
                                         .OrderDescending()
                                         .ToListAsync();
        }

        public async Task<IEnumerable<TaskMember>> GetTaskMembersByTeamAsync(int teamId)
        {
            return await _dbContext.TaskMembers.Where(tm => tm.TeamId == teamId)
                                               .Include(tm => tm.Student)
                                               .Include(tm => tm.Team)
                                               .Include(tm => tm.Task)
                                               .AsNoTracking()
                                               .OrderByDescending(tm => tm.CreatedAt)
                                               .ToListAsync();
        }
    }
}
