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

        public async Task<IEnumerable<TeamWithMembersDto>> GetAllTeamsForDoctorAsync(int doctorId)
        {
            var teams = await _dbContext.Teams.Include(t => t.Students)
                                                    .ThenInclude(s => s.AppUser)
                                              .Where(t => t.SupervisorId == doctorId)
                                              .OrderDescending()
                                              .ToListAsync();

            var result = teams.Select(t => new TeamWithMembersDto
            {
                Id = t.Id,
                Name = t.Name,
                HasProject = t.HasProject,
                LeaderId = t.LeaderId,
                SupervisorId = t.SupervisorId,
                Specialty = t.Specialty,
                Members = t.Students.Select(s => new TeamMemberDto
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
