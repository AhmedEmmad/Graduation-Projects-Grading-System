using GradingManagementSystem.Core.CustomResponses;
using GradingManagementSystem.Core.DTOs;
using GradingManagementSystem.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GradingManagementSystem.Repository.Data.DbContexts;
using Microsoft.EntityFrameworkCore;
using GradingManagementSystem.Core.Entities;
using OfficeOpenXml;

namespace GradingManagementSystem.APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EvaluationsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly GradingManagementSystemDbContext _dbContext;

        public EvaluationsController(IUnitOfWork unitOfWork, GradingManagementSystemDbContext dbContext)
        {
            _unitOfWork = unitOfWork;
            _dbContext = dbContext;
        }

        // Finished / Reviewed / Tested / Edited / D
        [HttpGet("AllTeamsForDoctorSupervisionEvaluation")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetAllTeamsForDoctorSupervisionEvaluation()
        {
            var doctorAppUserId = User.FindFirst("UserId")?.Value;
            if (doctorAppUserId == null)
                return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));
            var doctorAppUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (doctorAppUserRole == null)
                return Unauthorized(new ApiResponse(401, "Unauthorized role.", new { IsSuccess = false }));
            if (doctorAppUserRole != "Doctor")
                return Unauthorized(new ApiResponse(401, "Unauthorized role for evaluation.", new { IsSuccess = false }));

            var doctor = await _dbContext.Doctors.FirstOrDefaultAsync(d => d.AppUserId == doctorAppUserId);
            if (doctor == null)
                return NotFound(new ApiResponse(404, "Doctor not found.", new { IsSuccess = false }));

            var evaluatorId = doctor.Id;

            var activeAcademicYearAppointment = await _dbContext.AcademicAppointments.FirstOrDefaultAsync(a => a.Status == StatusType.Active.ToString());
            if (activeAcademicYearAppointment == null)
                return NotFound(new ApiResponse(404, "No active academic appointment found, You cannot evaluate now.", new { IsSuccess = true }));

            var supervisorScheduleIds = await _dbContext.CommitteeDoctorSchedules
                                                        .Include(cds => cds.Schedule)
                                                        .Where(cds => cds.DoctorId == evaluatorId &&
                                                                      cds.DoctorRole == "Supervisor" &&
                                                                      cds.Schedule.AcademicAppointmentId == activeAcademicYearAppointment.Id)
                                                        .Select(cds => cds.ScheduleId)
                                                        .ToListAsync();

            var supervisedTeams = await _dbContext.Teams
                                                  .Include(t => t.FinalProjectIdea)
                                                  .Include(t => t.Schedules)
                                                  .Include(t => t.Students)
                                                      .ThenInclude(s => s.AppUser)
                                                  .Include(t => t.AcademicAppointment)
                                                  .Include(t => t.Supervisor)
                                                  .Where(t => t.SupervisorId == evaluatorId &&
                                                              t.HasProject &&
                                                               t.AcademicAppointmentId == activeAcademicYearAppointment.Id &&
                                                               t.Schedules.Any(s => s.AcademicAppointmentId == activeAcademicYearAppointment.Id) &&
                                                               t.Schedules.Any(s => supervisorScheduleIds.Contains(s.Id)))
                                                  .AsNoTracking()
                                                  .ToListAsync();

            if (supervisedTeams == null || !supervisedTeams.Any() || supervisedTeams.Count == 0)
                return NotFound(new ApiResponse(404, "No teams found for Doctor to evaluate as supervisor.", new { IsSuccess = false }));

            var teamSpecialties = supervisedTeams.Select(t => t.Specialty).Distinct().ToList();
            var activeCriterias = await _dbContext.Criterias
                .Where(c => c.IsActive &&
                            c.Evaluator == "Supervisor" &&
                            teamSpecialties.Contains(c.Specialty) &&
                            c.AcademicAppointmentId == activeAcademicYearAppointment.Id)
                .AsNoTracking()
                .ToListAsync();

            if (activeCriterias == null || !activeCriterias.Any())
                return NotFound(new ApiResponse(404, "No active criteria found for the specialties of supervised teams.", new { IsSuccess = false }));

            var supervisorTeamsWithCriteriaBySpecialtyGroup = teamSpecialties.Select(specialty => new TeamsWithCriteriaBySpecialtyGroupDto
            {
                Specialty = specialty,
                Criterias = activeCriterias
                    .Where(c => c.Specialty == specialty && c.IsActive)
                    .Select(c => new CriteriaDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        MaxGrade = c.MaxGrade,
                        Evaluator = c.Evaluator,
                        GivenTo = c.GivenTo,
                        Specialty = c.Specialty,
                        Year = c.Year,
                        Term = c.Term,
                        CreatedAt = c.CreatedAt,
                    }).ToList(),
                Teams = supervisedTeams
                    .Where(t => t.Specialty == specialty)
                    .Select(t => new TeamWithCriteriaDto
                    {
                        TeamId = t.Id,
                        TeamName = t.Name,
                        ProjectId = t.FinalProjectIdea != null ? t.FinalProjectIdea.ProjectId : 0,
                        ProjectName = t.FinalProjectIdea != null ? t.FinalProjectIdea.ProjectName : "N/A",
                        ProjectDescription = t.FinalProjectIdea != null ? t.FinalProjectIdea.ProjectDescription : "N/A",
                        ScheduleId = t.Schedules.FirstOrDefault()?.Id ?? null,
                        ScheduleDate = t.Schedules.FirstOrDefault()?.ScheduleDate ?? null,
                        ScheduleStatus = t.Schedules.FirstOrDefault()?.Status ?? null,
                        Specialty = t.Specialty,
                        SupervisorId = t.SupervisorId,
                        SupervisorName = t.Supervisor?.FullName,
                        Criterias = new List<CriteriaDto>(),
                        //activeCriterias
                        //    .Where(c => c.Specialty == specialty &&
                        //                c.IsActive)
                        //    .Select(c => new CriteriaDto
                        //    {
                        //        Id = c.Id,
                        //        Name = c.Name,
                        //        Description = c.Description,
                        //        MaxGrade = c.MaxGrade,
                        //        Evaluator = c.Evaluator,
                        //        GivenTo = c.GivenTo,
                        //        Specialty = c.Specialty,
                        //        Year = c.Year,
                        //        Term = c.Term,
                        //        CreatedAt = c.CreatedAt,
                        //    }).ToList(),
                        TeamMembers = (t.Students != null)
                            ? t.Students
                                .Where(s => s.AppUser != null)
                                .Select(s => new TeamMemberDto
                                {
                                    Id = s.Id,
                                    FullName = s.FullName,
                                    Email = s.AppUser.Email ?? "N/A",
                                    Specialty = s.Specialty,
                                    InTeam = s.InTeam,
                                    ProfilePicture = s.AppUser.ProfilePicture ?? "N/A"
                                }).ToList()
                            : new List<TeamMemberDto>()
                    }).ToList()
            }).ToList();

            return Ok(new ApiResponse(200, "Supervision teams retrieved successfully.", new { IsSuccess = true, supervisorTeamsWithCriteriaBySpecialtyGroup }));
        }

        // Finished / Reviewed / Tested / Edited / D
        [HttpGet("AllTeamsForDoctorExaminationEvaluation")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetAllTeamsForDoctorExaminationEvaluation()
        {
            var appUserId = User.FindFirst("UserId")?.Value;
            if (appUserId == null)
                return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));
            var appUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (appUserRole == null)
                return Unauthorized(new ApiResponse(401, "Unauthorized role.", new { IsSuccess = false }));
            if (appUserRole != "Doctor")
                return Unauthorized(new ApiResponse(401, "Unauthorized role for evaluation.", new { IsSuccess = false }));

            var doctor = await _dbContext.Doctors.FirstOrDefaultAsync(d => d.AppUserId == appUserId);
            if (doctor == null)
                return NotFound(new ApiResponse(404, "Doctor not found.", new { IsSuccess = false }));

            var evaluatorId = doctor.Id;

            var activeAppointment = await _dbContext.AcademicAppointments.FirstOrDefaultAsync(a => a.Status == StatusType.Active.ToString());
            if (activeAppointment == null)
                return NotFound(new ApiResponse(404, "No active academic appointment found, You cannot evaluate now.", new { IsSuccess = true }));

            var examinerScheduleIds = await _dbContext.CommitteeDoctorSchedules
                                                      .Where(cds => cds.DoctorId == evaluatorId &&
                                                                    cds.DoctorRole == "Examiner" &&
                                                                    cds.Schedule.AcademicAppointmentId == activeAppointment.Id)
                                                      .Select(cds => cds.ScheduleId)
                                                      .ToListAsync();

            var examinationTeams = await _dbContext.Teams
                                                    .Include(t => t.FinalProjectIdea)
                                                        .ThenInclude(fp => fp.TeamProjectIdea)
                                                            .ThenInclude(tp => tp.Team)
                                                    .Include(t => t.FinalProjectIdea)
                                                        .ThenInclude(fp => fp.TeamRequestDoctorProjectIdea)
                                                            .ThenInclude(tr => tr.DoctorProjectIdea)
                                                    .Include(t => t.Schedules)
                                                    .Include(t => t.Students)
                                                        .ThenInclude(s => s.AppUser)
                                                    .Include(t => t.Supervisor)
                                                    .Where(t => t.SupervisorId != evaluatorId &&
                                                                t.HasProject &&
                                                                t.Schedules.Any(s => examinerScheduleIds.Contains(s.Id)) &&
                                                                t.AcademicAppointmentId == activeAppointment.Id &&
                                                                t.Schedules.Any(s => s.AcademicAppointmentId == activeAppointment.Id))
                                                    .AsNoTracking()
                                                    .ToListAsync();

            if (examinationTeams == null || !examinationTeams.Any() || examinationTeams.Count == 0)
                return NotFound(new ApiResponse(404, "No teams found for Doctor to evaluate as examiner.", new { IsSuccess = false }));

            var teamSpecialties = examinationTeams.Select(t => t.Specialty).Distinct().ToList();
            var activeCriterias = await _dbContext.Criterias
                                                  .Where(c => c.IsActive &&
                                                              c.Evaluator == "Examiner" &&
                                                              teamSpecialties.Contains(c.Specialty) &&
                                                              c.AcademicAppointmentId == activeAppointment.Id)
                                                  .AsNoTracking()
                                                  .ToListAsync();

            if (activeCriterias == null || !activeCriterias.Any() || activeCriterias.Count == 0)
                return NotFound(new ApiResponse(404, "No active criteria found for the specialties of examination teams.", new { IsSuccess = false }));

            var examinerTeamsWithCriteriaBySpecialtyGroup = teamSpecialties.Select(specialty => new TeamsWithCriteriaBySpecialtyGroupDto
            {
                Specialty = specialty,
                Criterias = activeCriterias
                    .Where(c => c.Specialty == specialty && c.IsActive)
                    .Select(c => new CriteriaDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        MaxGrade = c.MaxGrade,
                        Evaluator = c.Evaluator,
                        GivenTo = c.GivenTo,
                        Specialty = c.Specialty,
                        Year = c.Year,
                        Term = c.Term,
                        CreatedAt = c.CreatedAt,
                    }).ToList(),
                Teams = examinationTeams
                    .Where(t => t.Specialty == specialty)
                    .Select(t => new TeamWithCriteriaDto
                    {
                        TeamId = t.Id,
                        TeamName = t.Name,
                        ProjectId = t.FinalProjectIdea != null ? t.FinalProjectIdea.ProjectId : 0,
                        ProjectName = t.FinalProjectIdea != null ? t.FinalProjectIdea.ProjectName : "N/A",
                        ProjectDescription = t.FinalProjectIdea != null ? t.FinalProjectIdea.ProjectDescription : "N/A",
                        ScheduleId = t.Schedules.FirstOrDefault()?.Id ?? null,
                        ScheduleDate = t.Schedules.FirstOrDefault()?.ScheduleDate ?? null,
                        ScheduleStatus = t.Schedules.FirstOrDefault()?.Status ?? null,
                        Specialty = t.Specialty,
                        SupervisorId = t.SupervisorId,
                        SupervisorName = t.Supervisor?.FullName,
                        Criterias = new List<CriteriaDto>(),
                        //activeCriterias
                        //    .Where(c => c.Specialty == specialty && c.IsActive)
                        //    .Select(c => new CriteriaDto
                        //    {
                        //        Id = c.Id,
                        //        Name = c.Name,
                        //        Description = c.Description,
                        //        MaxGrade = c.MaxGrade,
                        //        Evaluator = c.Evaluator,
                        //        GivenTo = c.GivenTo,
                        //        Specialty = c.Specialty,
                        //        Year = c.Year,
                        //        Term = c.Term,
                        //        CreatedAt = c.CreatedAt,
                        //    }).ToList(),
                        TeamMembers = (t.Students != null)
                            ? t.Students
                                .Where(s => s.AppUser != null)
                                .Select(s => new TeamMemberDto
                                {
                                    Id = s.Id,
                                    FullName = s.FullName,
                                    Email = s.AppUser.Email ?? "N/A",
                                    Specialty = s.Specialty,
                                    InTeam = s.InTeam,
                                    ProfilePicture = s.AppUser.ProfilePicture ?? "N/A"
                                }).ToList()
                            : new List<TeamMemberDto>()
                    }).ToList()
            }).ToList();

            return Ok(new ApiResponse(200, "Examination teams retrieved successfully.", new { IsSuccess = true, examinerTeamsWithCriteriaBySpecialtyGroup }));
        }

        // Finished / Reviewed / Tested / Edited / D
        [HttpGet("AllTeamsForAdminEvaluation")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllTeamsForAdminEvaluation()
        {
            var appUserId = User.FindFirst("UserId")?.Value;
            if (appUserId == null)
                return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));
            var appUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (appUserRole == null)
                return Unauthorized(new ApiResponse(401, "Unauthorized role.", new { IsSuccess = false }));
            if (appUserRole != "Admin")
                return Unauthorized(new ApiResponse(401, "Unauthorized role for evaluation.", new { IsSuccess = false }));

            var admin = await _dbContext.Admins.FirstOrDefaultAsync(a => a.AppUserId == appUserId);
            if (admin == null)
                return NotFound(new ApiResponse(404, "Admin not found.", new { IsSuccess = false }));

            var evaluatorId = admin.Id;

            var activeAppointment = await _dbContext.AcademicAppointments.FirstOrDefaultAsync(a => a.Status == StatusType.Active.ToString());
            if (activeAppointment == null)
                return NotFound(new ApiResponse(404, "No active academic appointment found, You cannot evaluate now.", new { IsSuccess = true }));

            //var examinerScheduleIds = await _dbContext.CommitteeDoctorSchedules
            //                                          .Where(cds => cds.Schedule.AcademicAppointmentId == activeAppointment.Id)
            //                                          .Select(cds => cds.ScheduleId)
            //                                          .ToListAsync();

            var adminTeams = await _dbContext.Teams
                                            .Include(t => t.FinalProjectIdea)
                                                .ThenInclude(fp => fp.TeamProjectIdea)
                                                    .ThenInclude(tp => tp.Team)
                                            .Include(t => t.Schedules)
                                            .Include(t => t.Students)
                                                .ThenInclude(s => s.AppUser)
                                            .Include(t => t.Supervisor)
                                            .Where(t => t.HasProject &&
                                                        t.AcademicAppointmentId == activeAppointment.Id &&
                                                        t.Schedules.Any(s => s.AcademicAppointmentId == activeAppointment.Id))
                                            .AsNoTracking()
                                            .ToListAsync();

            if (adminTeams == null || !adminTeams.Any() || adminTeams.Count == 0)
                return NotFound(new ApiResponse(404, "No teams found for Admin to evaluate.", new { IsSuccess = false }));

            var teamSpecialties = adminTeams.Select(t => t.Specialty).Distinct().ToList();
            var activeCriterias = await _dbContext.Criterias
                                                   .Where(c => c.IsActive &&
                                                                c.Evaluator == "Admin" &&
                                                                teamSpecialties.Contains(c.Specialty) &&
                                                                c.AcademicAppointmentId == activeAppointment.Id)
                                                   .AsNoTracking()
                                                   .ToListAsync();

            if (activeCriterias == null || !activeCriterias.Any() || activeCriterias.Count == 0)
                return NotFound(new ApiResponse(404, "No active criteria found for the specialties of admin teams.", new { IsSuccess = false }));

            var TeamsWithCriteriaBySpecialtyGroup = teamSpecialties.Select(specialty => new TeamsWithCriteriaBySpecialtyGroupDto
            {
                Specialty = specialty,
                Criterias = activeCriterias
                            .Where(c => c.Specialty == specialty && c.IsActive)
                            .Select(c => new CriteriaDto
                            {
                                Id = c.Id,
                                Name = c.Name,
                                Description = c.Description,
                                MaxGrade = c.MaxGrade,
                                Evaluator = c.Evaluator,
                                GivenTo = c.GivenTo,
                                Specialty = c.Specialty,
                                Year = c.Year,
                                Term = c.Term,
                                CreatedAt = c.CreatedAt,
                            }).ToList(),
                Teams = adminTeams
                        .Where(t => t.Specialty == specialty)
                        .Select(t => new TeamWithCriteriaDto
                        {
                            TeamId = t.Id,
                            TeamName = t.Name,
                            ProjectId = t.FinalProjectIdea != null ? t.FinalProjectIdea.ProjectId : 0,
                            ProjectName = t.FinalProjectIdea != null ? t.FinalProjectIdea.ProjectName : "N/A",
                            ProjectDescription = t.FinalProjectIdea != null ? t.FinalProjectIdea.ProjectDescription : "N/A",
                            Specialty = t.Specialty,
                            ScheduleId = t.Schedules.FirstOrDefault()?.Id ?? null,
                            ScheduleDate = t.Schedules.FirstOrDefault()?.ScheduleDate ?? null,
                            ScheduleStatus = t.Schedules.FirstOrDefault()?.Status ?? null,
                            SupervisorId = t.SupervisorId,
                            SupervisorName = t.Supervisor?.FullName,
                            Criterias = new List<CriteriaDto>(),
                            TeamMembers = t.Students != null ?
                                                t.Students
                                                .Where(s => s.AppUser != null) // Filter out students with null AppUser
                                                .Select(s => new TeamMemberDto
                                                {
                                                    Id = s.Id,
                                                    FullName = s.FullName,
                                                    Email = s.AppUser.Email ?? "N/A",
                                                    Specialty = s.Specialty,
                                                    InTeam = s.InTeam,
                                                    ProfilePicture = s.AppUser.ProfilePicture ?? "N/A"
                                                }).ToList()
                                            : new List<TeamMemberDto>()
                        }).ToList()
            }).ToList();

            return Ok(new ApiResponse(200, "Admin teams retrieved successfully.", new { IsSuccess = true, TeamsWithCriteriaBySpecialtyGroup }));
        }

        // Finished / Reviewed / Tested / Edited
        [HttpGet("TeamEvaluations/{teamId}/{scheduleId}")]
        [Authorize(Roles = "Admin, Doctor")]
        public async Task<IActionResult> GetTeamEvaluations(int teamId, int scheduleId)
        {
            var appUserId = User.FindFirst("UserId")?.Value;
            var appUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (appUserId == null || appUserRole == null)
                return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));

            if (appUserRole != "Admin" && appUserRole != "Doctor")
                return Unauthorized(new ApiResponse(401, "Unauthorized role for evaluation.", new { IsSuccess = false }));

            int evaluatorId = 0;
            string evaluatorRole = string.Empty;

            var activeAppointment = await _dbContext.AcademicAppointments.FirstOrDefaultAsync(a => a.Status == StatusType.Active.ToString());
            if (activeAppointment == null)
                return NotFound(new ApiResponse(404, "No active academic appointment found.", new { IsSuccess = false }));

            if (appUserRole == "Admin")
            {
                var admin = await _dbContext.Admins.FirstOrDefaultAsync(a => a.AppUserId == appUserId);
                if (admin == null)
                    return NotFound(new ApiResponse(404, "Admin not found.", new { IsSuccess = false }));
                evaluatorId = admin.Id;
                evaluatorRole = "Admin";
            }
            else if (appUserRole == "Doctor")
            {
                var doctor = await _dbContext.Doctors.FirstOrDefaultAsync(d => d.AppUserId == appUserId);
                if (doctor == null)
                    return NotFound(new ApiResponse(404, "Doctor not found.", new { IsSuccess = false }));

                var schedule = await _dbContext.Schedules
                                               .Include(s => s.CommitteeDoctorSchedules)
                                               .Include(s => s.Team)
                                               .FirstOrDefaultAsync(s => s.Id == scheduleId && s.AcademicAppointmentId == activeAppointment.Id);

                if (schedule == null)
                    return NotFound(new ApiResponse(404, "Schedule not found.", new { IsSuccess = false }));

                var isSupervisor = schedule.TeamId == teamId &&
                                   doctor.Id == schedule.Team.SupervisorId &&
                                   schedule.CommitteeDoctorSchedules.Any(cds => cds.DoctorRole == "Supervisor" &&
                                                                         cds.DoctorId == doctor.Id &&
                                                                         cds.ScheduleId == scheduleId);
                var isExaminer = schedule.TeamId == teamId &&
                                 schedule.CommitteeDoctorSchedules.Any(cds => cds.DoctorRole == "Examiner" &&
                                                                       cds.DoctorId == doctor.Id &&
                                                                       cds.ScheduleId == scheduleId);

                if (isSupervisor)
                    evaluatorRole = "Supervisor";
                else if (isExaminer)
                    evaluatorRole = "Examiner";
                else
                    return NotFound(new ApiResponse(404, "Doctor not authorized for this evaluation.", new { IsSuccess = false }));

                evaluatorId = doctor.Id;
            }

            var existingEvaluations = new List<Evaluation>();
            if (evaluatorRole == "Admin")
            {
                existingEvaluations = await _dbContext.Evaluations
                                                    .Include(e => e.Criteria)
                                                    .Where(e => e.TeamId == teamId &&
                                                           e.ScheduleId == scheduleId &&
                                                           e.AdminEvaluatorId == evaluatorId &&
                                                           e.DoctorEvaluatorId == null &&
                                                           e.EvaluatorRole == evaluatorRole &&
                                                           e.AcademicAppointmentId == activeAppointment.Id)
                                                    .AsNoTracking()
                                                    .ToListAsync();
            }
            else
            {
                existingEvaluations = await _dbContext.Evaluations
                                                    .Include(e => e.Criteria)
                                                    .Where(e => e.TeamId == teamId &&
                                                           e.ScheduleId == scheduleId &&
                                                           e.AdminEvaluatorId == null &&
                                                           e.DoctorEvaluatorId == evaluatorId &&
                                                           e.EvaluatorRole == evaluatorRole &&
                                                           e.AcademicAppointmentId == activeAppointment.Id)
                                                    .AsNoTracking()
                                                    .ToListAsync();
            }

            if (existingEvaluations == null || !existingEvaluations.Any())
                return NotFound(new ApiResponse(404, "No evaluations found for the specified team and schedule.", new { IsSuccess = false }));

            var evaluations = existingEvaluations.Select(e => new EvaluationObjectDto
            {
                EvaluationId = e.Id,
                ScheduleId = e.ScheduleId,
                CriteriaId = e.CriteriaId,
                CriteriaName = e.Criteria.Name,
                CriteriaDescription = e.Criteria.Description,
                Grade = e.Grade,
                EvaluationDate = e.EvaluationDate,
                EvaluatorRole = e.EvaluatorRole,
                DoctorEvaluatorId = e.DoctorEvaluatorId,
                AdminEvaluatorId = e.AdminEvaluatorId,
                TeamId = e.TeamId,
                StudentId = e.StudentId
            }).ToList();

            return Ok(new ApiResponse(200, "Last evaluations retrieved successfully.", new { IsSuccess = true, evaluations }));
        }

        // Finished / Reviewed / Tested / Edited
        [HttpPost("SubmitGrades")]
        [Authorize(Roles = "Admin, Doctor")]
        public async Task<IActionResult> SubmitGrades([FromBody] SubmitEvaluationDto model)
        {
            if (model == null)
                return BadRequest(new ApiResponse(400, "Invalid input data.", new { IsSuccess = false }));

            var appUserId = User.FindFirst("UserId")?.Value;
            var appUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (appUserId == null || appUserRole == null)
                return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));

            if (appUserRole != "Admin" && appUserRole != "Doctor")
                return Unauthorized(new ApiResponse(401, "Unauthorized role for evaluation.", new { IsSuccess = false }));

            int evaluatorId = 0;
            string evaluatorRole = string.Empty;

            var activeAppointment = await _dbContext.AcademicAppointments.FirstOrDefaultAsync(a => a.Status == StatusType.Active.ToString());
            if (activeAppointment == null)
                return NotFound(new ApiResponse(404, "No active academic appointment found, You cannot evaluate now.", new { IsSuccess = false }));

            if (appUserRole == "Admin")
            {
                var admin = await _dbContext.Admins.FirstOrDefaultAsync(a => a.AppUserId == appUserId);
                if (admin == null)
                    return NotFound(new ApiResponse(404, "Admin not found.", new { IsSuccess = false }));
                evaluatorId = admin.Id;
                evaluatorRole = "Admin";
            }
            else if (appUserRole == "Doctor")
            {
                var doctor = await _dbContext.Doctors.FirstOrDefaultAsync(d => d.AppUserId == appUserId);
                if (doctor == null)
                    return NotFound(new ApiResponse(404, "Doctor not found.", new { IsSuccess = false }));

                var schedule = await _dbContext.Schedules
                    .Include(s => s.CommitteeDoctorSchedules)
                    .Include(s => s.Team)
                    .FirstOrDefaultAsync(s => s.Id == model.ScheduleId && s.AcademicAppointmentId == activeAppointment.Id);
                if (schedule == null)
                    return NotFound(new ApiResponse(404, "Schedule not found.", new { IsSuccess = false }));

                var isSupervisor = schedule.TeamId == model.TeamId && doctor.Id == schedule.Team.SupervisorId && schedule.CommitteeDoctorSchedules.Any(cds => cds.DoctorRole == "Supervisor" && cds.DoctorId == doctor.Id && cds.ScheduleId == schedule.Id);
                var isExaminer = schedule.CommitteeDoctorSchedules.Any(cds => cds.DoctorId == doctor.Id && cds.ScheduleId == schedule.Id && cds.DoctorRole == "Examiner");

                if (isSupervisor)
                    evaluatorRole = "Supervisor";
                else if (isExaminer)
                    evaluatorRole = "Examiner";
                else
                    return NotFound(new ApiResponse(404, "Doctor not authorized for this evaluation.", new { IsSuccess = false, ExaminerId = doctor.Id }));

                evaluatorId = doctor.Id;
            }

            foreach (var gradeItem in model.Grades)
            {
                // Check if an evaluation with the same grade already exists
                var existingEvaluation = await _dbContext.Evaluations
                    .FirstOrDefaultAsync(e => e.ScheduleId == model.ScheduleId &&
                                              e.CriteriaId == gradeItem.CriteriaId &&
                                              e.TeamId == model.TeamId &&
                                              e.StudentId == model.StudentId &&
                                              e.EvaluatorRole == evaluatorRole &&
                                              (e.AdminEvaluatorId == evaluatorId || e.AdminEvaluatorId == null) &&
                                              (e.DoctorEvaluatorId == evaluatorId || e.DoctorEvaluatorId == null) &&
                                              e.AcademicAppointmentId == activeAppointment.Id);

                if (existingEvaluation != null)
                {
                    // Check if the grade has been modified
                    if (existingEvaluation.Grade != gradeItem.Grade)
                    {
                        existingEvaluation.Grade = gradeItem.Grade;
                        _dbContext.Evaluations.Update(existingEvaluation);
                    }
                }
                else
                {
                    // Proceed to add the new evaluation
                    var criteria = await _dbContext.Criterias.FirstOrDefaultAsync(c => c.Id == gradeItem.CriteriaId &&
                                                                                       c.AcademicAppointmentId == activeAppointment.Id);
                    if (criteria == null)
                        return NotFound(new ApiResponse(404, $"Criteria not found.", new { IsSuccess = false }));

                    if (gradeItem.Grade < 0 || gradeItem.Grade > criteria.MaxGrade)
                        return BadRequest(new ApiResponse(400, $"Grade '{gradeItem.Grade}' is out of range.", new { IsSuccess = false }));

                    var newEvaluation = new Evaluation
                    {
                        ScheduleId = model.ScheduleId,
                        CriteriaId = gradeItem.CriteriaId,
                        DoctorEvaluatorId = appUserRole == "Doctor" ? evaluatorId : null,
                        AdminEvaluatorId = appUserRole == "Admin" ? evaluatorId : null,
                        EvaluatorRole = evaluatorRole,
                        StudentId = model.StudentId,
                        TeamId = model.TeamId,
                        Grade = gradeItem.Grade,
                        AcademicAppointmentId = activeAppointment.Id,
                    };

                    await _dbContext.Evaluations.AddAsync(newEvaluation);
                }
            }

            await _dbContext.SaveChangesAsync();

            // Update HasCompletedEvaluation to true (1) for the examiner or supervisor
            if (appUserRole == "Doctor")
            {
                var committeeDoctorSchedule = await _dbContext.CommitteeDoctorSchedules
                    .FirstOrDefaultAsync(cds => cds.ScheduleId == model.ScheduleId && cds.DoctorId == evaluatorId);

                if (committeeDoctorSchedule != null)
                {
                    committeeDoctorSchedule.HasCompletedEvaluation = true;
                    _dbContext.CommitteeDoctorSchedules.Update(committeeDoctorSchedule);
                    await _dbContext.SaveChangesAsync();
                }
            }

            // Check if all examiners and supervisor for the schedule have completed their evaluation
            if (model.ScheduleId.HasValue && model.TeamId.HasValue)
            {
                var schedule = await _dbContext.Schedules
                    .Include(s => s.CommitteeDoctorSchedules)
                    .FirstOrDefaultAsync(s => s.Id == model.ScheduleId && s.AcademicAppointmentId == activeAppointment.Id);

                if (schedule != null)
                {
                    // Get all examiners and supervisor for this schedule
                    var examinerIds = schedule.CommitteeDoctorSchedules
                        .Where(cds => cds.DoctorRole == "Examiner")
                        .Select(cds => cds.DoctorId)
                        .ToList();

                    var supervisorId = schedule.CommitteeDoctorSchedules
                        .Where(cds => cds.DoctorRole == "Supervisor")
                        .Select(cds => cds.DoctorId)
                        .FirstOrDefault();

                    // Check if all examiners have completed evaluation
                    var allExaminersCompleted = !examinerIds.Any() ||
                        await _dbContext.CommitteeDoctorSchedules
                            .Where(cds => cds.ScheduleId == schedule.Id && cds.DoctorRole == "Examiner")
                            .AllAsync(cds => cds.HasCompletedEvaluation);

                    // Check if supervisor has completed evaluation
                    var supervisorCompleted = supervisorId == 0 ||
                        await _dbContext.CommitteeDoctorSchedules
                            .Where(cds => cds.ScheduleId == schedule.Id && cds.DoctorRole == "Supervisor")
                            .AllAsync(cds => cds.HasCompletedEvaluation);

                    // If all examiners and supervisor have completed, set IsGraded = true
                    if (allExaminersCompleted && supervisorCompleted)
                    {
                        schedule.IsGraded = true;
                        _dbContext.Schedules.Update(schedule);
                        await _dbContext.SaveChangesAsync();
                    }
                }
            }

            return Ok(new ApiResponse(200, "Grades submitted successfully.", new { IsSuccess = true }));
        }

        // Finished / Reviewed / Tested / Edited / I
        [HttpGet("StudentGrades")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetStudentGrades()
        {
            var appUserId = User.FindFirst("UserId")?.Value;
            if (appUserId == null)
                return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));
            var appUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (appUserRole == null)
                return Unauthorized(new ApiResponse(401, "Unauthorized role.", new { IsSuccess = false }));

            if (appUserRole != "Student")
                return Unauthorized(new ApiResponse(401, "Unauthorized role for viewing grades.", new { IsSuccess = false }));

            var student = await _dbContext.Students.Include(s => s.Team).FirstOrDefaultAsync(s => s.AppUserId == appUserId);
            if (student == null)
                return NotFound(new ApiResponse(404, "Student not found.", new { IsSuccess = false }));

            var studentId = student.Id;
            var teamId = student.TeamId;

            var activeAppointment = await _dbContext.AcademicAppointments.FirstOrDefaultAsync(a => a.Status == StatusType.Active.ToString());
            if (activeAppointment == null)
                return NotFound(new ApiResponse(404, "No active academic appointment found, You cannot see your grades.", new { IsSuccess = false }));

            var evaluations = await _dbContext.Evaluations
                .Include(e => e.Criteria)
                .Where(e => (e.StudentId == studentId || e.TeamId == teamId) &&
                            e.AcademicAppointmentId == activeAppointment.Id)
                .AsNoTracking()
                .ToListAsync();

            if (evaluations == null || !evaluations.Any())
                return NotFound(new ApiResponse(404, "No grades found for the student.", new { IsSuccess = false }));

            var totalGradeAfterSummation = 0.0;

            // Supervisor
            var supervisorEvaluations = evaluations
                .Where(e => e.EvaluatorRole == "Supervisor" &&
                    ((e.Criteria.GivenTo == "Student" && e.StudentId == studentId && e.TeamId == teamId) ||
                     (e.Criteria.GivenTo == "Team" && e.StudentId == null && e.TeamId == teamId)))
                .GroupBy(e => e.Criteria.Name)
                .Select(g =>
                {
                    var criteria = g.First().Criteria;
                    var totalGrade = g.Sum(e => e.Grade);
                    totalGradeAfterSummation += totalGrade;
                    return new
                    {
                        CriteriaId = criteria.Id,
                        CriteriaName = criteria.Name,
                        CriteriaDescription = criteria.Description,
                        GivenTo = criteria.GivenTo,
                        MaximumGrade = criteria.MaxGrade,
                        Grade = Math.Round(totalGrade),
                        EvaluatorRole = "Supervisor"
                    };
                });

            // Admin
            var adminEvaluations = evaluations
                .Where(e => e.EvaluatorRole == "Admin" &&
                    ((e.Criteria.GivenTo == "Student" && e.StudentId == studentId && e.TeamId == teamId) ||
                     (e.Criteria.GivenTo == "Team" && e.StudentId == null && e.TeamId == teamId)))
                .GroupBy(e => e.Criteria.Name)
                .Select(g =>
                {
                    var criteria = g.First().Criteria;
                    var totalGrade = g.Sum(e => e.Grade);
                    totalGradeAfterSummation += totalGrade;
                    return new
                    {
                        CriteriaId = criteria.Id,
                        CriteriaName = criteria.Name,
                        CriteriaDescription = criteria.Description,
                        GivenTo = criteria.GivenTo,
                        MaximumGrade = criteria.MaxGrade,
                        Grade = Math.Round(totalGrade),
                        EvaluatorRole = "Admin"
                    };
                });

            // Examiner: average of summation for both student and team criteria
            var examinerEvaluations = evaluations
                .Where(e => e.EvaluatorRole == "Examiner" &&
                    ((e.Criteria.GivenTo == "Student" && e.StudentId == studentId && e.TeamId == teamId) ||
                     (e.Criteria.GivenTo == "Team" && e.StudentId == null && e.TeamId == teamId)))
                .GroupBy(e => e.Criteria.Name)
                .Select(g =>
                {
                    var criteria = g.First().Criteria;
                    var totalGrade = g.Sum(e => e.Grade);
                    var count = g.Count();
                    var averageGrade = count > 0 ? totalGrade / count : 0;
                    totalGradeAfterSummation += averageGrade;
                    return new
                    {
                        CriteriaId = criteria.Id,
                        CriteriaName = criteria.Name,
                        CriteriaDescription = criteria.Description,
                        GivenTo = criteria.GivenTo,
                        MaximumGrade = criteria.MaxGrade,
                        Grade = Math.Round(averageGrade),
                        EvaluatorRole = "Examiner"
                    };
                });

            var totalGrade = Math.Round(totalGradeAfterSummation);

            var combinedEvaluations = supervisorEvaluations
                .Concat(adminEvaluations)
                .Concat(examinerEvaluations)
                .ToList();

            return Ok(new ApiResponse(200, "Student grades retrieved successfully.", new { IsSuccess = true, Grades = combinedEvaluations, totalGrade }));
        }

        // Finished / Reviewed / Tested / Edited
        [HttpGet("ExportGradesForSpecialty/{specialty}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportGradesForSpecialty(string specialty)
        {
            var adminAppUserId = User.FindFirst("UserId")?.Value;
            var appUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (adminAppUserId == null)
                return Unauthorized(new ApiResponse(401, "Unauthorized user.", new { IsSuccess = false }));
            if (appUserRole != "Admin" || appUserRole == null)
                return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));

            var activeAppointment = await _dbContext.AcademicAppointments.FirstOrDefaultAsync(a => a.Status == StatusType.Active.ToString());
            if (activeAppointment == null)
                return NotFound(new ApiResponse(404, "No active academic appointment found in this time.", new { IsSuccess = true }));

            var teams = await _dbContext.Teams
                                        .Include(t => t.Students)
                                            .ThenInclude(s => s.AppUser)
                                        .Include(t => t.Schedules)
                                        .Include(t => t.Evaluations)
                                        .Where(t => t.Specialty == specialty &&
                                                    t.HasProject == true &&
                                                    t.Schedules.Any(s => s.AcademicAppointmentId == activeAppointment.Id &&
                                                                         s.Status == "Finished"
                                                                   ) &&
                                                    t.AcademicAppointmentId == activeAppointment.Id
                                               )
                                        .AsNoTracking()
                                        .OrderBy(t => t.Name)
                                        .ToListAsync();

            if (teams == null || !teams.Any())
                return NotFound(new ApiResponse(404, $"No teams found for this specialty: '{specialty}'.", new { IsSuccess = false }));

            var criterias = await _dbContext.Criterias
                                            .Where(c => c.IsActive == true &&
                                                        c.Specialty == specialty &&
                                                        c.AcademicAppointmentId == activeAppointment.Id
                                                  )
                                            .AsNoTracking()
                                            .OrderBy(c => c.Name)
                                            .ToListAsync();

            if (criterias == null || !criterias.Any())
                return NotFound(new ApiResponse(404, $"No criterias found for this specialty: '{specialty}'.", new { IsSuccess = false }));

            // Check if all supervisors and admins have evaluated all students in the teams
            var supervisorCriteria = criterias.Where(c => c.Evaluator == "Supervisor").ToList();
            var adminCriteria = criterias.Where(c => c.Evaluator == "Admin").ToList();
            var examinerCriteria = criterias.Where(c => c.Evaluator == "Examiner").ToList();

            // Check if all supervisors have evaluated all students in the teams
            foreach (var team in teams)
            {
                foreach (var student in team.Students)
                {
                    // Check Supervisor Evaluations
                    foreach (var criteria in supervisorCriteria)
                    {
                        var supervisorEvaluation = await _dbContext.Evaluations
                            .Where(e => e.CriteriaId == criteria.Id &&
                                        e.TeamId == team.Id &&
                                        (e.StudentId == student.Id || e.StudentId == null) &&
                                        e.EvaluatorRole == "Supervisor")
                            .OrderBy(e => e.EvaluationDate)
                            .ToListAsync();

                        var totalSupervisors = await _dbContext.CommitteeDoctorSchedules
                                                             .Where(cds => cds.Schedule.TeamId == team.Id &&
                                                                           cds.DoctorRole == "Supervisor")
                                                             .CountAsync();

                        if (supervisorEvaluation.Count != totalSupervisors)
                        {
                            return BadRequest(new ApiResponse(400, $"Not all supervisors have evaluated student '{student.FullName}' in team '{team.Name}' for criteria '{criteria.Name}'.", new { IsSuccess = false }));
                        }
                    }
                }
            }

            // Check if all admins have evaluated all students in the teams
            foreach (var team in teams)
            {
                foreach (var student in team.Students)
                {
                    // Check Admin Evaluations
                    foreach (var criteria in adminCriteria)
                    {
                        var adminEvaluation = await _dbContext.Evaluations
                            .Where(e => e.CriteriaId == criteria.Id &&
                                        e.TeamId == team.Id &&
                                        (e.StudentId == student.Id || e.StudentId == null) &&
                                        e.EvaluatorRole == "Admin")
                            .OrderBy(e => e.EvaluationDate)
                            .ToListAsync();
                        if (!adminEvaluation.Any())
                        {
                            return BadRequest(new ApiResponse(400, $"Admin has not evaluated student '{student.FullName}' in team '{team.Name}' for criteria '{criteria.Name}'.", new { IsSuccess = false }));
                        }
                    }
                }
            }

            // Check if all examiners have evaluated all students in the teams
            foreach (var team in teams)
            {
                foreach (var student in team.Students)
                {
                    foreach (var criteria in examinerCriteria)
                    {
                        var evaluations = await _dbContext.Evaluations
                                                          .Where(e => e.CriteriaId == criteria.Id &&
                                                                      e.TeamId == team.Id &&
                                                                      (e.StudentId == student.Id || e.StudentId == null) &&
                                                                      e.EvaluatorRole == "Examiner")
                                                            .OrderBy(e => e.EvaluationDate)
                                                          .ToListAsync();

                        var totalExaminers = await _dbContext.CommitteeDoctorSchedules
                                                             .Where(cds => cds.Schedule.TeamId == team.Id &&
                                                                           cds.DoctorRole == "Examiner")
                                                             .CountAsync();

                        if (evaluations.Count != totalExaminers)
                        {
                            return BadRequest(new ApiResponse(400, $"Not all examiners have evaluated student '{student.FullName}' in team '{team.Name}' for criteria '{criteria.Name}'.", new { IsSuccess = false }));
                        }
                    }
                }
            }

            ExcelPackage.License.SetNonCommercialPersonal("Backend Team");
            using (var package = new ExcelPackage(new FileInfo("MyWorkbook.xlsx")))
            {
                var worksheet = package.Workbook.Worksheets.Add("Grades");

                int col = 1;
                worksheet.Cells[1, col++].Value = "Student Name";
                worksheet.Cells[1, col++].Value = "Email";
                worksheet.Cells[1, col++].Value = "Team Name";

                foreach (var c in adminCriteria)
                    worksheet.Cells[1, col++].Value = $"Admin: {c.Name}";
                foreach (var c in supervisorCriteria)
                    worksheet.Cells[1, col++].Value = $"Supervisor: {c.Name}";
                foreach (var c in examinerCriteria)
                    worksheet.Cells[1, col++].Value = $"Examiner: {c.Name} (Avg)";

                int row = 2;
                foreach (var team in teams)
                {
                    foreach (var student in team.Students.OrderBy(s => s.FullName))
                    {
                        col = 1;
                        worksheet.Cells[row, col++].Value = student.FullName;
                        worksheet.Cells[row, col++].Value = student.AppUser.Email;
                        worksheet.Cells[row, col++].Value = team.Name;

                        var evaluations = await _dbContext.Evaluations
                                                          .Include(e => e.Criteria)
                                                          .Where(e => ((e.StudentId == student.Id && e.TeamId == team.Id) ||
                                                                      (e.StudentId == null && e.TeamId == team.Id))
                                                                )
                                                          .AsNoTracking()
                                                          .ToListAsync();

                        foreach (var c in adminCriteria)
                        {
                            var grade = evaluations.FirstOrDefault(e => e.CriteriaId == c.Id &&
                                                                   e.EvaluatorRole == "Admin")?.Grade;

                            worksheet.Cells[row, col].Value = grade.HasValue ? grade.Value.ToString() : "N/A";
                            if (grade.HasValue)
                            {
                                worksheet.Cells[row, col].Style.Numberformat.Format = "0.00";
                            }
                            col++;
                        }

                        foreach (var c in supervisorCriteria)
                        {
                            var grade = evaluations.FirstOrDefault(e => e.CriteriaId == c.Id &&
                                                                   e.EvaluatorRole == "Supervisor")?.Grade;

                            worksheet.Cells[row, col].Value = grade.HasValue ? grade.Value.ToString() : "N/A";
                            if (grade.HasValue)
                            {
                                worksheet.Cells[row, col].Style.Numberformat.Format = "0.00";
                            }
                            col++;
                        }

                        foreach (var c in examinerCriteria)
                        {
                            var examinerGrades = evaluations.Where(e => e.CriteriaId == c.Id && e.EvaluatorRole == "Examiner")
                                                            .Select(e => e.Grade)
                                                            .ToList();

                            var average = examinerGrades.Any() ? examinerGrades.Average() : (double?)null;
                            worksheet.Cells[row, col].Value = average.HasValue ? average.Value.ToString() : "N/A";
                            if (average.HasValue)
                            {
                                worksheet.Cells[row, col].Style.Numberformat.Format = "0.00";
                            }
                            col++;
                        }

                        row++;
                    }
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"Grades_{specialty}.xlsx";
                var fileContent = stream.ToArray();

                var response = new
                {
                    FileName = fileName,
                    FileContent = Convert.ToBase64String(fileContent),
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                };

                return Ok(new ApiResponse(200, "Grades exported successfully.", new { IsSuccess = true, excelSheet = response }));
            }
        }

        // Finished / Reviewed / Tested / Edited / D
        [HttpGet("AllTeamsWithoutEvaluationBySpecialty/{doctorId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllTeamsWithoutEvaluationBySpecialtyForThisDoctor(int doctorId)
        {
            if (doctorId <= 0)
                return BadRequest(new ApiResponse(400, "Invalid doctor ID.", new { IsSuccess = false }));
            var appUserId = User.FindFirst("UserId")?.Value;
            if (appUserId == null)
                return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));
            var appUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (appUserRole == null || appUserRole != "Admin")
                return Unauthorized(new ApiResponse(401, "Unauthorized role for this action.", new { IsSuccess = false }));
            if (appUserRole != "Admin")
                return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));

            var admin = await _dbContext.Admins.FirstOrDefaultAsync(a => a.AppUserId == appUserId);
            if (admin == null)
                return NotFound(new ApiResponse(404, "Admin not found.", new { IsSuccess = false }));

            var doctor = await _dbContext.Doctors.FirstOrDefaultAsync(d => d.Id == doctorId);
            if (doctor == null)
                return NotFound(new ApiResponse(404, "Doctor not found.", new { IsSuccess = false }));

            var activeAppointment = await _dbContext.AcademicAppointments
                .FirstOrDefaultAsync(a => a.Status == StatusType.Active.ToString());
            if (activeAppointment == null)
                return NotFound(new ApiResponse(404, "No active academic appointment found, You cannot evaluate now.", new { IsSuccess = false }));

            var pendingDoctorSchedules = await _dbContext.CommitteeDoctorSchedules
                .Include(cds => cds.Schedule)
                    .ThenInclude(s => s.Team)
                        .ThenInclude(t => t.Students)
                            .ThenInclude(st => st.AppUser)
                .Include(cds => cds.Schedule)
                    .ThenInclude(s => s.Team)
                        .ThenInclude(t => t.FinalProjectIdea)
                .Include(t => t.Schedule.Team.Supervisor)
                .Where(cds => cds.DoctorId == doctorId &&
                              cds.Schedule.AcademicAppointmentId == activeAppointment.Id &&
                              cds.Schedule.IsGraded == false)
                .AsNoTracking()
                .ToListAsync();

            if (pendingDoctorSchedules == null || !pendingDoctorSchedules.Any())
                return NotFound(new ApiResponse(404, "No teams found without evaluation for this doctor.", new { IsSuccess = false }));

            var schedules = pendingDoctorSchedules
                .Select(cds => cds.Schedule)
                .Where(s => s.TeamId != null && s.AcademicAppointmentId == activeAppointment.Id)
                .ToList();

            var teams = schedules.Select(s => s.Team).Distinct().ToList();
            var teamSpecialties = teams.Select(t => t.Specialty).Distinct().ToList();

            var activeCriterias = await _dbContext.Criterias
                .Where(c => c.IsActive &&
                            teamSpecialties.Contains(c.Specialty) &&
                            c.AcademicAppointmentId == activeAppointment.Id &&
                            c.Evaluator != "Admin")
                .AsNoTracking()
                .ToListAsync();

            var teamsWithCriteriaBySpecialtyGroup = new List<TeamsWithCriteriaBySpecialtyGroupDto>();

            foreach (var specialty in teamSpecialties)
            {
                var doctorSchedules = pendingDoctorSchedules
                    .Where(cds => cds.Schedule.Team.Specialty == specialty)
                    .ToList();

                var isExaminer = doctorSchedules.Any(cds => cds.DoctorRole == "Examiner");
                var isSupervisor = doctorSchedules.Any(cds => cds.DoctorRole == "Supervisor");

                var criteriasForExaminer = activeCriterias
                    .Where(c => c.Specialty == specialty && c.Evaluator == "Examiner" && c.IsActive)
                    .Select(c => new CriteriaDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        MaxGrade = c.MaxGrade,
                        Evaluator = c.Evaluator,
                        GivenTo = c.GivenTo,
                        Specialty = c.Specialty,
                        Year = c.Year,
                        Term = c.Term,
                        CreatedAt = c.CreatedAt,
                    }).ToList();

                var criteriasForSupervisor = activeCriterias
                    .Where(c => c.Specialty == specialty && c.Evaluator == "Supervisor" && c.IsActive)
                    .Select(c => new CriteriaDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        MaxGrade = c.MaxGrade,
                        Evaluator = c.Evaluator,
                        GivenTo = c.GivenTo,
                        Specialty = c.Specialty,
                        Year = c.Year,
                        Term = c.Term,
                        CreatedAt = c.CreatedAt,
                    }).ToList();

                var criterias = new List<CriteriaDto>();
                if (isExaminer)
                    criterias.AddRange(criteriasForExaminer);
                if (isSupervisor)
                    criterias.AddRange(criteriasForSupervisor);

                var teamsWithoutEvaluation = new List<TeamWithCriteriaDto>();

                foreach (var t in teams.Where(t => t.Specialty == specialty))
                {
                    var schedule = schedules.FirstOrDefault(s => s.Team.Id == t.Id);
                    var teamMemberDtos = t.Students != null
                        ? t.Students
                            .Where(s => s.AppUser != null)
                            .Select(s => new TeamMemberDto
                            {
                                Id = s.Id,
                                FullName = s.FullName,
                                Email = s.AppUser.Email ?? "N/A",
                                Specialty = s.Specialty,
                                InTeam = s.InTeam,
                                ProfilePicture = s.AppUser.ProfilePicture ?? "N/A"
                            }).ToList()
                        : new List<TeamMemberDto>();

                    bool allCriteriasEvaluated = true;
                    foreach (var criteria in criterias)
                    {
                        if (criteria.GivenTo == "Student")
                        {
                            foreach (var student in t.Students)
                            {
                                var hasEval = await _dbContext.Evaluations.AnyAsync(e =>
                                    e.CriteriaId == criteria.Id &&
                                    e.TeamId == t.Id &&
                                    e.StudentId == student.Id &&
                                    e.EvaluatorRole == criteria.Evaluator &&
                                    e.DoctorEvaluatorId == doctorId &&
                                    e.AcademicAppointmentId == activeAppointment.Id);
                                if (!hasEval)
                                {
                                    allCriteriasEvaluated = false;
                                    break;
                                }
                            }
                        }
                        else // GivenTo == "Team"
                        {
                            var hasEval = await _dbContext.Evaluations.AnyAsync(e =>
                                e.CriteriaId == criteria.Id &&
                                e.TeamId == t.Id &&
                                e.StudentId == null &&
                                e.EvaluatorRole == criteria.Evaluator &&
                                e.DoctorEvaluatorId == doctorId &&
                                e.AcademicAppointmentId == activeAppointment.Id);
                            if (!hasEval)
                            {
                                allCriteriasEvaluated = false;
                                break;
                            }
                        }
                        if (!allCriteriasEvaluated) break;
                    }

                    if (!allCriteriasEvaluated)
                    {
                        teamsWithoutEvaluation.Add(new TeamWithCriteriaDto
                        {
                            TeamId = t.Id,
                            TeamName = t.Name,
                            ProjectId = t.FinalProjectIdea != null ? t.FinalProjectIdea.ProjectId : 0,
                            ProjectName = t.FinalProjectIdea != null ? t.FinalProjectIdea.ProjectName : "N/A",
                            ProjectDescription = t.FinalProjectIdea != null ? t.FinalProjectIdea.ProjectDescription : "N/A",
                            ScheduleId = schedule?.Id ?? null,
                            ScheduleDate = schedule?.ScheduleDate ?? null,
                            ScheduleStatus = schedule?.Status ?? null,
                            SupervisorId = t.SupervisorId,
                            SupervisorName = t.Supervisor?.FullName,
                            Specialty = t.Specialty,
                            Criterias = new List<CriteriaDto>(),
                            TeamMembers = teamMemberDtos
                        });
                    }
                }

                teamsWithCriteriaBySpecialtyGroup.Add(new TeamsWithCriteriaBySpecialtyGroupDto
                {
                    Specialty = specialty,
                    Criterias = criterias,
                    Teams = teamsWithoutEvaluation
                });
            }

            return Ok(new ApiResponse(200, "Teams without evaluation for this doctor retrieved successfully.", new { IsSuccess = true, teamsWithCriteriaBySpecialtyGroup }));
        }

        // Finished / Reviewed / Tested / Edited / D
        [HttpGet("TeamEvaluationsForEvaluatingAdmin/{teamId}/{scheduleId}/{doctorId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTeamEvaluationsForEvaluatingAdmin(int teamId, int scheduleId, int doctorId)
        {
            var appUserId = User.FindFirst("UserId")?.Value;
            var appUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (appUserId == null || appUserRole == null)
                return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));

            if (appUserRole != "Admin")
                return Unauthorized(new ApiResponse(401, "Unauthorized role for evaluation.", new { IsSuccess = false }));

            int evaluatorId = 0;
            string evaluatorRole = "Doctor";

            var activeAppointment = await _dbContext.AcademicAppointments.FirstOrDefaultAsync(a => a.Status == StatusType.Active.ToString());
            if (activeAppointment == null)
                return NotFound(new ApiResponse(404, "No active academic appointment found.", new { IsSuccess = false }));

            if (evaluatorRole == "Doctor")
            {
                var doctor = await _dbContext.Doctors.FirstOrDefaultAsync(d => d.Id == doctorId);
                if (doctor == null)
                    return NotFound(new ApiResponse(404, "Doctor not found.", new { IsSuccess = false }));

                var schedule = await _dbContext.Schedules
                                               .Include(s => s.CommitteeDoctorSchedules)
                                               .Include(s => s.Team)
                                               .FirstOrDefaultAsync(s => s.Id == scheduleId && s.AcademicAppointmentId == activeAppointment.Id && s.TeamId == teamId);

                if (schedule == null)
                    return NotFound(new ApiResponse(404, "Schedule not found.", new { IsSuccess = false }));

                var isSupervisor = schedule.TeamId == teamId &&
                                   doctor.Id == schedule.Team.SupervisorId &&
                                   schedule.CommitteeDoctorSchedules.Any(cds => cds.DoctorRole == "Supervisor" &&
                                                                         cds.DoctorId == doctor.Id &&
                                                                         cds.ScheduleId == scheduleId);
                var isExaminer = schedule.TeamId == teamId &&
                                 schedule.CommitteeDoctorSchedules.Any(cds => cds.DoctorRole == "Examiner" &&
                                                                       cds.DoctorId == doctor.Id &&
                                                                       cds.ScheduleId == scheduleId);

                if (isSupervisor)
                    evaluatorRole = "Supervisor";
                else if (isExaminer)
                    evaluatorRole = "Examiner";
                else
                    return NotFound(new ApiResponse(404, "Doctor not authorized for this evaluation.", new { IsSuccess = false }));

                evaluatorId = doctor.Id;
            }

            var existingEvaluations = await _dbContext.Evaluations
                                                    .Include(e => e.Criteria)
                                                    .Where(e => e.TeamId == teamId &&
                                                           e.ScheduleId == scheduleId &&
                                                           e.AdminEvaluatorId == null &&
                                                           e.DoctorEvaluatorId == evaluatorId &&
                                                           e.EvaluatorRole == evaluatorRole &&
                                                           e.AcademicAppointmentId == activeAppointment.Id)
                                                    .AsNoTracking()
                                                    .ToListAsync();

            if (existingEvaluations == null || !existingEvaluations.Any())
                return NotFound(new ApiResponse(404, "No evaluations found for the specified team and schedule.", new { IsSuccess = false }));

            var evaluations = existingEvaluations.Select(e => new EvaluationObjectDto
            {
                EvaluationId = e.Id,
                ScheduleId = e.ScheduleId,
                CriteriaId = e.CriteriaId,
                CriteriaName = e.Criteria.Name,
                CriteriaDescription = e.Criteria.Description,
                Grade = e.Grade,
                EvaluationDate = e.EvaluationDate,
                EvaluatorRole = e.EvaluatorRole,
                DoctorEvaluatorId = e.DoctorEvaluatorId,
                AdminEvaluatorId = e.AdminEvaluatorId,
                TeamId = e.TeamId,
                StudentId = e.StudentId
            }).ToList();

            return Ok(new ApiResponse(200, "Last evaluations retrieved successfully.", new { IsSuccess = true, evaluations }));
        }

        // Finished / Reviewed / Tested / Edited
        [HttpPost("SubmitGradesFromAdminForEvaluatingDoctor")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SubmiSubmitGradesFromAdminForEvaluatingDoctortGrades([FromBody] SubmitEvaluationIIDto model)
        {
            if (model == null)
                return BadRequest(new ApiResponse(400, "Invalid input data.", new { IsSuccess = false }));

            var appUserId = User.FindFirst("UserId")?.Value;
            var appUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (appUserId == null || appUserRole == null)
                return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));

            if (appUserRole != "Admin")
                return Unauthorized(new ApiResponse(401, "Unauthorized role for evaluation.", new { IsSuccess = false }));

            int evaluatorId = 0;
            string evaluatorRole = "Doctor";

            var activeAppointment = await _dbContext.AcademicAppointments.FirstOrDefaultAsync(a => a.Status == StatusType.Active.ToString());
            if (activeAppointment == null)
                return NotFound(new ApiResponse(404, "No active academic appointment found, You cannot evaluate now.", new { IsSuccess = false }));

            if (evaluatorRole == "Doctor")
            {
                var doctor = await _dbContext.Doctors.FirstOrDefaultAsync(d => d.Id == model.DoctorId);
                if (doctor == null)
                    return NotFound(new ApiResponse(404, "Doctor not found.", new { IsSuccess = false }));

                var schedule = await _dbContext.Schedules
                        .Include(s => s.CommitteeDoctorSchedules)
                        .Include(s => s.Team)
                        .FirstOrDefaultAsync(s => s.Id == model.ScheduleId && s.AcademicAppointmentId == activeAppointment.Id && s.TeamId == model.TeamId);
                if (schedule == null)
                    return NotFound(new ApiResponse(404, "Schedule not found.", new { IsSuccess = false }));

                var isSupervisor = schedule.TeamId == model.TeamId && doctor.Id == schedule.Team.SupervisorId && schedule.CommitteeDoctorSchedules.Any(cds => cds.DoctorRole == "Supervisor" && cds.DoctorId == doctor.Id && cds.ScheduleId == schedule.Id);
                var isExaminer = schedule.CommitteeDoctorSchedules.Any(cds => cds.DoctorId == doctor.Id && cds.ScheduleId == schedule.Id && cds.DoctorRole == "Examiner");

                if (isSupervisor)
                    evaluatorRole = "Supervisor";
                else if (isExaminer)
                    evaluatorRole = "Examiner";
                else
                    return NotFound(new ApiResponse(404, "Doctor not authorized for this evaluation.", new { IsSuccess = false, ExaminerId = doctor.Id }));

                evaluatorId = doctor.Id;
            }

            foreach (var gradeItem in model.Grades)
            {
                // Check if an evaluation with the same grade already exists
                var existingEvaluation = await _dbContext.Evaluations
                    .FirstOrDefaultAsync(e => e.ScheduleId == model.ScheduleId &&
                                              e.CriteriaId == gradeItem.CriteriaId &&
                                              e.TeamId == model.TeamId &&
                                              e.StudentId == model.StudentId &&
                                              e.EvaluatorRole == evaluatorRole &&
                                              (e.AdminEvaluatorId == evaluatorId || e.AdminEvaluatorId == null) &&
                                              (e.DoctorEvaluatorId == evaluatorId || e.DoctorEvaluatorId == null) &&
                                              e.AcademicAppointmentId == activeAppointment.Id);

                if (existingEvaluation != null)
                {
                    // Check if the grade has been modified
                    if (existingEvaluation.Grade != gradeItem.Grade)
                    {
                        existingEvaluation.Grade = gradeItem.Grade;
                        _dbContext.Evaluations.Update(existingEvaluation);
                    }
                }
                else
                {
                    // Proceed to add the new evaluation
                    var criteria = await _dbContext.Criterias.FirstOrDefaultAsync(c => c.Id == gradeItem.CriteriaId &&
                                                                                       c.AcademicAppointmentId == activeAppointment.Id);
                    if (criteria == null)
                        return NotFound(new ApiResponse(404, $"Criteria not found.", new { IsSuccess = false }));

                    if (gradeItem.Grade < 0 || gradeItem.Grade > criteria.MaxGrade)
                        return BadRequest(new ApiResponse(400, $"Grade '{gradeItem.Grade}' is out of range.", new { IsSuccess = false }));

                    var newEvaluation = new Evaluation
                    {
                        ScheduleId = model.ScheduleId,
                        CriteriaId = gradeItem.CriteriaId,
                        DoctorEvaluatorId = evaluatorId,
                        AdminEvaluatorId = null,
                        EvaluatorRole = evaluatorRole,
                        StudentId = model.StudentId,
                        TeamId = model.TeamId,
                        Grade = gradeItem.Grade,
                        AcademicAppointmentId = activeAppointment.Id,
                    };

                    await _dbContext.Evaluations.AddAsync(newEvaluation);
                }
            }

            await _dbContext.SaveChangesAsync();

                var committeeDoctorSchedule = await _dbContext.CommitteeDoctorSchedules
                    .FirstOrDefaultAsync(cds => cds.ScheduleId == model.ScheduleId && cds.DoctorId == evaluatorId);

                if (committeeDoctorSchedule != null)
                {
                    committeeDoctorSchedule.HasCompletedEvaluation = true;
                    _dbContext.CommitteeDoctorSchedules.Update(committeeDoctorSchedule);
                    await _dbContext.SaveChangesAsync();
                }

            // Check if all examiners and supervisor for the schedule have completed their evaluation
            if (model.ScheduleId.HasValue && model.TeamId.HasValue)
            {
                var schedule = await _dbContext.Schedules
                    .Include(s => s.CommitteeDoctorSchedules)
                    .FirstOrDefaultAsync(s => s.Id == model.ScheduleId && s.AcademicAppointmentId == activeAppointment.Id);

                if (schedule != null)
                {
                    // Get all examiners and supervisor for this schedule
                    var examinerIds = schedule.CommitteeDoctorSchedules
                        .Where(cds => cds.DoctorRole == "Examiner")
                        .Select(cds => cds.DoctorId)
                        .ToList();

                    var supervisorId = schedule.CommitteeDoctorSchedules
                        .Where(cds => cds.DoctorRole == "Supervisor")
                        .Select(cds => cds.DoctorId)
                        .FirstOrDefault();

                    // Check if all examiners have completed evaluation
                    var allExaminersCompleted = !examinerIds.Any() ||
                        await _dbContext.CommitteeDoctorSchedules
                            .Where(cds => cds.ScheduleId == schedule.Id && cds.DoctorRole == "Examiner")
                            .AllAsync(cds => cds.HasCompletedEvaluation);

                    // Check if supervisor has completed evaluation
                    var supervisorCompleted = supervisorId == 0 ||
                        await _dbContext.CommitteeDoctorSchedules
                            .Where(cds => cds.ScheduleId == schedule.Id && cds.DoctorRole == "Supervisor")
                            .AllAsync(cds => cds.HasCompletedEvaluation);

                    // If all examiners and supervisor have completed, set IsGraded = true
                    if (allExaminersCompleted && supervisorCompleted)
                    {
                        schedule.IsGraded = true;
                        _dbContext.Schedules.Update(schedule);
                        await _dbContext.SaveChangesAsync();
                    }
                }
            }

            return Ok(new ApiResponse(200, "Grades submitted successfully.", new { IsSuccess = true }));
        }
    }
}