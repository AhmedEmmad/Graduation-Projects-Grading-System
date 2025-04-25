using GradingManagementSystem.Core.DTOs;
using GradingManagementSystem.Core.Entities;
using GradingManagementSystem.Core.Repositories.Contact;
using GradingManagementSystem.Repository.Data.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace GradingManagementSystem.Repository
{
    public class TeamRepository : GenericRepository<Team>, ITeamRepository
    {
        private readonly GradingManagementSystemDbContext _dbContext;

        public TeamRepository(GradingManagementSystemDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<TeamWithMembersDto>> GetAllTeamsForDoctor(int doctorId)
        {
            var teams = await _dbContext.Teams.Include(T => T.Students).Where(T => T.SupervisorId == doctorId).ToListAsync();
            var students = await _dbContext.Students.Include(S => S.AppUser).ToListAsync();

            var result = teams.Select(T => new TeamWithMembersDto
            {
                Id = T.Id,
                Name = T.Name,
                HasProject = T.HasProject,
                LeaderId = T.LeaderId,
                SupervisorId = T.SupervisorId,
                Members = T.Students.Select(s => new TeamMemberDto
                {
                    Id = s.Id,
                    FullName = s.FullName,
                    Email = s.Email,
                    Specialty = s.Specialty,
                    InTeam = s.InTeam,
                    ProfilePicture = s.AppUser.ProfilePicture
                }).ToList()
            });
            return result;
        }
    }
}
