using GradingManagementSystem.Core.DTOs;
using GradingManagementSystem.Core.Entities;
using GradingManagementSystem.Core.Repositories.Contact;
using GradingManagementSystem.Repository.Data.DbContexts;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GradingManagementSystem.Repository
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly GradingManagementSystemDbContext _dbContext;

        public ProjectRepository(GradingManagementSystemDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<DoctorProjectIdeaDto>?> GetDoctorProjectIdeasByStatusAndDoctorIdIfNeededAsync(string status, int? doctorId = null)
        {
            var activeAppointment = await _dbContext.AcademicAppointments
                                                    .Where(a => a.Status == "Active")
                                                    .FirstOrDefaultAsync();
            if (activeAppointment == null)
                return null;

            if (status == "Pending")
            {
                var pendingDoctorProjects = await _dbContext.DoctorProjectIdeas.Where(p => p.Status == "Pending" &&
                                                                                           p.AcademicAppointmentId == activeAppointment.Id)
                                                                               .Include(p => p.Doctor)
                                                                               .OrderByDescending(p => p.SubmissionDate)
                                                                               .ToListAsync();

                return pendingDoctorProjects.Select(p => new DoctorProjectIdeaDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    SubmissionDate = p.SubmissionDate,
                    Status = p.Status,
                    DoctorId = p.DoctorId,
                    DoctorName = p.Doctor.FullName,
                });
            }
            else if (status == "Accepted")
            {
                if(doctorId != null)
                {
                    var acceptedDoctorProjects = await _dbContext.DoctorProjectIdeas.Where(p => p.Status == "Accepted" &&
                                                                                                p.DoctorId == doctorId &&
                                                                                                p.AcademicAppointmentId == activeAppointment.Id)
                                                                                    .Include(p => p.Doctor)
                                                                                    .OrderByDescending(p => p.SubmissionDate)
                                                                                    .ToListAsync();

                    return acceptedDoctorProjects.Select(p => new DoctorProjectIdeaDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        SubmissionDate = p.SubmissionDate,
                        Status = p.Status,
                        DoctorId = p.DoctorId,
                        DoctorName = p.Doctor.FullName
                    });
                }
                else
                {
                    var acceptedDoctorProjects = await _dbContext.DoctorProjectIdeas.Where(p => p.Status == "Accepted" &&
                                                                                                p.Taken == false &&
                                                                                                p.AcademicAppointmentId == activeAppointment.Id)
                                                                                    .Include(p => p.Doctor)
                                                                                    .OrderByDescending(p => p.SubmissionDate)
                                                                                    .ToListAsync();

                    return acceptedDoctorProjects.Select(p => new DoctorProjectIdeaDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        SubmissionDate = p.SubmissionDate,
                        Status = p.Status,
                        DoctorId = p.DoctorId,
                        DoctorName = p.Doctor.FullName
                    });
                }
            }
            else if (status == "Rejected")
            {
                if (doctorId != null)
                {
                    var rejectedDoctorProjects = await _dbContext.DoctorProjectIdeas.Where(p => p.Status == "Rejected" &&
                                                                                                p.DoctorId == doctorId &&
                                                                                                p.AcademicAppointmentId == activeAppointment.Id)
                                                                                    .Include(p => p.Doctor)
                                                                                    .OrderByDescending(p => p.SubmissionDate)
                                                                                    .ToListAsync();

                    return rejectedDoctorProjects.Select(p => new DoctorProjectIdeaDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        SubmissionDate = p.SubmissionDate,
                        Status = p.Status,
                        DoctorId = p.DoctorId,
                        DoctorName = p.Doctor.FullName
                    });
                }
                else
                {
                    var rejectedDoctorProjects = await _dbContext.DoctorProjectIdeas.Where(p => p.Status == "Rejected" &&
                                                                                                p.AcademicAppointmentId == activeAppointment.Id)
                                                                                    .Include(p => p.Doctor)
                                                                                    .OrderByDescending(p => p.SubmissionDate)
                                                                                    .ToListAsync();

                    return rejectedDoctorProjects.Select(p => new DoctorProjectIdeaDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        SubmissionDate = p.SubmissionDate,
                        Status = p.Status,
                        DoctorId = p.DoctorId,
                        DoctorName = p.Doctor.FullName
                    });
                }
            }
            return null;
        }

        public async Task<IEnumerable<TeamProjectIdeaDto>?> GetTeamProjectIdeasByStatusAsync(string status)
        {
            var activeAppointment = await _dbContext.AcademicAppointments
                                                    .Where(a => a.Status == "Active")
                                                    .FirstOrDefaultAsync();
            if (activeAppointment == null)
                return null;

            if (status == StatusType.Pending.ToString())
            {
                var pendingTeamProjects = await _dbContext.TeamProjectIdeas.Where(p => p.Status == StatusType.Pending.ToString() &&
                                                                                       p.AcademicAppointmentId == activeAppointment.Id)
                                                                           .Include(p => p.Leader)
                                                                           .Include(p => p.Team)
                                                                                .ThenInclude(t => t.Supervisor)
                                                                            .OrderByDescending(p => p.SubmissionDate)
                                                                           .ToListAsync();

                return pendingTeamProjects.Select(P => new TeamProjectIdeaDto
                {
                    Id = P.Id,
                    Name = P.Name,
                    Description = P.Description,
                    SubmissionDate = P.SubmissionDate,
                    Status = P.Status,
                    TeamId = P.TeamId,
                    TeamName = P.Team.Name,
                    LeaderId = P.LeaderId,
                    LeaderName = P.Leader?.FullName,
                    SupervisorId = P.Team?.SupervisorId,
                    SupervisorName = P.Team?.Supervisor?.FullName
                });
            }
            else if (status == StatusType.Accepted.ToString())
            {
                var acceptedTeamProjects = await _dbContext.TeamProjectIdeas.Where(p => p.Status == StatusType.Accepted.ToString() &&
                                                                                        p.AcademicAppointmentId == activeAppointment.Id)
                                                                           .Include(p => p.Leader)
                                                                           .Include(p => p.Team)
                                                                           .OrderByDescending(p => p.SubmissionDate)
                                                                           .ToListAsync();

                return acceptedTeamProjects.Select(P => new TeamProjectIdeaDto
                {
                    Id = P.Id,
                    Name = P.Name,
                    Description = P.Description,
                    SubmissionDate = P.SubmissionDate,
                    Status = P.Status,
                    TeamId = P.TeamId,
                    TeamName = P.Team.Name,
                    LeaderId = P.LeaderId,
                    LeaderName = P.Leader.FullName,
                    SupervisorId = P.Team.SupervisorId,
                    SupervisorName = P.Team.Supervisor.FullName
                });
            }
            return null;
        }

        public async Task<IEnumerable<TeamRequestForDoctorProjectIdeaDto>?> GetPendingTeamRequestsForDoctorProjectIdeasAsync(int doctorId)
        {
            var activeAppointment = await _dbContext.AcademicAppointments
                                                    .Where(a => a.Status == "Active")
                                                    .FirstOrDefaultAsync();
            if (activeAppointment == null)
                return null;

            var pendingTeamRequests = await _dbContext.TeamsRequestDoctorProjectIdeas.Where(P => P.Status == "Pending" &&
                                                                                            P.DoctorId == doctorId &&
                                                                                            P.AcademicAppointmentId == activeAppointment.Id)
                                                                              .Include(P => P.Team)
                                                                              .ThenInclude(T => T.Students)
                                                                              .ThenInclude(S => S.AppUser)
                                                                              .Include(P => P.Leader)
                                                                              .ThenInclude(L => L.AppUser)
                                                                              .Include(P => P.Doctor)
                                                                              .ThenInclude(D => D.TeamsRequestDoctorProjectIdeas)
                                                                              .Include(P => P.DoctorProjectIdea)
                                                                              .OrderByDescending(P => P.RequestedDate)
                                                                              .ToListAsync();

            return pendingTeamRequests.Select(P => new TeamRequestForDoctorProjectIdeaDto
            {
                RequestId = P.Id,
                Status = P.Status,
                RequestedDate = P.RequestedDate,
                TeamId = P.TeamId,
                TeamName = P.Team.Name,
                LeaderId = P.LeaderId,
                LeaderName = P.Leader.FullName,
                DoctorId = P.DoctorId,
                DoctorName = P.Doctor.FullName,
                DoctorProjectIdeaId = P.DoctorProjectIdeaId,
                DoctorProjectIdeaName = P.DoctorProjectIdea.Name,
                TeamMembers = P.Team.Students.Select(S => new TeamMemberDto
                {
                    Id = S.Id,
                    FullName = S.FullName,
                    Email = S.Email,
                    Specialty = S.Specialty,
                    InTeam = S.InTeam,
                    ProfilePicture = S.AppUser?.ProfilePicture
                }).ToList()
            });
        }

        public async Task<IEnumerable<FinalProjectIdeaForAdminDto>?> GetAllFinalProjectIdeas()
        {
            var activeAppointment = await _dbContext.AcademicAppointments
                                                    .Where(a => a.Status == "Active")
                                                    .FirstOrDefaultAsync();
            if (activeAppointment == null)
                return null;

            var finalProjects = await _dbContext.FinalProjectIdeas.Where(FP => FP.AcademicAppointmentId == activeAppointment.Id)
                                                                  .Include(FP => FP.Supervisor)
                                                                  .Include(FP => FP.Team)
                                                                    .ThenInclude(T => T.Students)
                                                                    .ThenInclude(S => S.AppUser)
                                                                 .OrderDescending() 
                                                                 .ToListAsync();

            return finalProjects.Select(FP => new FinalProjectIdeaForAdminDto
            {
                ProjectId = FP.ProjectId,
                ProjectName = FP?.ProjectName,
                ProjectDescription = FP?.ProjectDescription,
                SupervisorId = FP?.SupervisorId,
                SupervisorName = FP?.Supervisor?.FullName,
                TeamId = FP?.TeamId,
                TeamName = FP?.Team?.Name,
                PostedBy = FP?.PostedBy,
                TeamMembers = FP?.Team?.Students?.Select(S => new TeamMemberDto
                {
                    Id = S.Id,
                    FullName = S.FullName,
                    Email = S.Email,
                    Specialty = S.Specialty,
                    InTeam = S.InTeam,
                    ProfilePicture = S.AppUser?.ProfilePicture
                }).ToList()
            }).ToList();
        }
    }
}
