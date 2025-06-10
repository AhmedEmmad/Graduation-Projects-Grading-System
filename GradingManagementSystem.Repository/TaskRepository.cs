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
            var academicAppointment = await _dbContext.AcademicAppointments
                .Where(a => a.Status == StatusType.Active.ToString())
                .FirstOrDefaultAsync();
            if (academicAppointment == null)
                return Enumerable.Empty<TaskItem>();

            return await _dbContext.Tasks.Where(t => t.TeamId == teamId && t.AcademicAppointmentId == academicAppointment.Id)
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
            var academicAppointment = await _dbContext.AcademicAppointments
                .Where(a => a.Status == StatusType.Active.ToString())
                .FirstOrDefaultAsync();
            if (academicAppointment == null)
                return Enumerable.Empty<TaskMember>();

            return await _dbContext.TaskMembers.Where(tm => tm.TeamId == teamId && tm.Task.AcademicAppointmentId == academicAppointment.Id)
                                               .Include(tm => tm.Student)
                                               .Include(tm => tm.Team)
                                               .Include(tm => tm.Task)
                                               .AsNoTracking()
                                               .OrderByDescending(tm => tm.CreatedAt)
                                               .ToListAsync();
        }
    }
}
