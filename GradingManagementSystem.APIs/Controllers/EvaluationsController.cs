using GradingManagementSystem.Core.CustomResponses;
using GradingManagementSystem.Core.DTOs;
using GradingManagementSystem.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GradingManagementSystem.Repository.Data.DbContexts;
using Microsoft.EntityFrameworkCore;
using GradingManagementSystem.Core.Entities;
using System.Linq;

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

        // Finished / Reviewed / Tested  
        [HttpGet("AllTeamsForDoctorSupervisionEvaluation")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetAllTeamsForDoctorSupervisionEvaluation()
        {
            var appUserId = User.FindFirst("UserId")?.Value;
            var appUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (appUserId == null || appUserRole == null)
                return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));

            if (appUserRole != "Doctor")
                return Unauthorized(new ApiResponse(401, "Unauthorized role for evaluation.", new { IsSuccess = false }));

            var doctor = await _dbContext.Doctors.FirstOrDefaultAsync(d => d.AppUserId == appUserId);
            if (doctor == null)
                return NotFound(new ApiResponse(404, "Doctor not found.", new { IsSuccess = false }));

            var evaluatorId = doctor.Id;

            var activeAppointment = await _dbContext.AcademicAppointments.FirstOrDefaultAsync(a => a.Status == "Active");
            if (activeAppointment == null)
                return NotFound(new ApiResponse(404, "No active academic appointment found.", new { IsSuccess = true }));

            var supervisorScheduleIds = await _dbContext.CommitteeDoctorSchedules
                .Where(cds => cds.DoctorId == evaluatorId && cds.DoctorRole == "Supervisor")
                .Select(cds => cds.ScheduleId)
                .ToListAsync();

            var supervisedTeams = await _dbContext.Teams
                .Include(t => t.FinalProjectIdea)
                .Include(t => t.Schedules)
                .Include(t => t.Students)
                    .ThenInclude(s => s.AppUser)
                .Where(t => t.SupervisorId == evaluatorId && t.HasProject && t.Schedules.Any(s => s.IsActive && s.AcademicAppointmentId == activeAppointment.Id) && t.Schedules.Any(s => supervisorScheduleIds.Contains(s.Id)))
                .AsNoTracking()
                .ToListAsync();

            if (supervisedTeams == null || !supervisedTeams.Any())
                return NotFound(new ApiResponse(404, "No teams found for Doctor evaluation as supervisor.", new { IsSuccess = false }));

            var teamSpecialties = supervisedTeams.Select(t => t.Specialty).Distinct().ToList();
            var activeCriterias = await _dbContext.Criterias
                .Where(c => c.IsActive && c.Evaluator == "Supervisor" && teamSpecialties.Contains(c.Specialty) && c.AcademicAppointmentId == activeAppointment.Id)
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
                            ProjectId = t.FinalProjectIdea.ProjectId,
                            ProjectName = t.FinalProjectIdea.ProjectName,
                            ProjectDescription = t.FinalProjectIdea.ProjectDescription,
                            ScheduleId = t.Schedules.FirstOrDefault()?.Id,
                            ScheduleDate = t.Schedules.FirstOrDefault()?.ScheduleDate,
                            ScheduleStatus = t.Schedules.FirstOrDefault()?.Status,
                            TeamMembers = t.Students.Select(s => new TeamMemberDto
                            {
                                Id = s.Id,
                                FullName = s.FullName,
                                Email = s.AppUser.Email,
                                Specialty = s.Specialty,
                                InTeam = s.InTeam,
                                ProfilePicture = s.AppUser.ProfilePicture,
                            }).ToList()
                        }).ToList()
            }).ToList();

            // Update the status of schedules to "Finished"  
            var schedulesToUpdate = await _dbContext.Schedules
                .Where(s => supervisorScheduleIds.Contains(s.Id) && s.Status != "Finished")
                .ToListAsync();

            foreach (var schedule in schedulesToUpdate)
            {
                schedule.Status = "Finished";
            }

            await _dbContext.SaveChangesAsync();

            return Ok(new ApiResponse(200, "Supervision teams retrieved successfully.", new { IsSuccess = true, supervisorTeamsWithCriteriaBySpecialtyGroup }));
        }

        // Finished / Reviewed / Tested
        [HttpGet("AllTeamsForDoctorExaminationEvaluation")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetAllTeamsForDoctorExaminationEvaluation()
        {
            var appUserId = User.FindFirst("UserId")?.Value;
            var appUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (appUserId == null || appUserRole == null)
                return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));

            if (appUserRole != "Doctor")
                return Unauthorized(new ApiResponse(401, "Unauthorized role for evaluation.", new { IsSuccess = false }));

            var doctor = await _dbContext.Doctors.FirstOrDefaultAsync(d => d.AppUserId == appUserId);
            if (doctor == null)
                return NotFound(new ApiResponse(404, "Doctor not found.", new { IsSuccess = false }));

            var evaluatorId = doctor.Id;

            var activeAppointment = await _dbContext.AcademicAppointments.FirstOrDefaultAsync(a => a.Status == "Active");
            if (activeAppointment == null)
                return NotFound(new ApiResponse(404, "No active academic appointment found.", new { IsSuccess = true }));

            var examinerScheduleIds = await _dbContext.CommitteeDoctorSchedules
                .Where(cds => cds.DoctorId == evaluatorId && cds.DoctorRole == "Examiner")
                .Select(cds => cds.ScheduleId)
                .ToListAsync();

            var examinationTeams = await _dbContext.Teams
                .Include(t => t.FinalProjectIdea)
                .Include(t => t.Schedules)
                .Include(t => t.Students)
                    .ThenInclude(s => s.AppUser)
                .Where(t => t.Schedules.Any(s => examinerScheduleIds.Contains(s.Id)) && t.Schedules.Any(s => s.IsActive && s.AcademicAppointmentId == activeAppointment.Id))
                .AsNoTracking()
                .ToListAsync();

            if (examinationTeams == null || !examinationTeams.Any())
                return NotFound(new ApiResponse(404, "No teams found for Doctor evaluation as examiner.", new { IsSuccess = false }));            

            var teamSpecialties = examinationTeams.Select(t => t.Specialty).Distinct().ToList();
            var activeCriterias = await _dbContext.Criterias
                .Where(c => c.IsActive && c.Evaluator == "Examiner" && teamSpecialties.Contains(c.Specialty) && c.AcademicAppointmentId == activeAppointment.Id)
                .AsNoTracking()
                .ToListAsync();

            if (activeCriterias == null || !activeCriterias.Any())
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
                            ProjectId = t.FinalProjectIdea.ProjectId,
                            ProjectName = t.FinalProjectIdea.ProjectName,
                            ProjectDescription = t.FinalProjectIdea.ProjectDescription,
                            ScheduleId = t.Schedules.FirstOrDefault()?.Id,
                            ScheduleDate = t.Schedules.FirstOrDefault()?.ScheduleDate,
                            ScheduleStatus = t.Schedules.FirstOrDefault()?.Status,
                            TeamMembers = t.Students.Select(s => new TeamMemberDto
                            {
                                Id = s.Id,
                                FullName = s.FullName,
                                Email = s.AppUser.Email,
                                Specialty = s.Specialty,
                                InTeam = s.InTeam,
                                ProfilePicture = s.AppUser.ProfilePicture,
                            }).ToList()
                        }).ToList()
            }).ToList();

            // Update the status of schedules to "Finished"  
            var schedulesToUpdate = await _dbContext.Schedules
                .Where(s => examinerScheduleIds.Contains(s.Id) && s.Status != "Finished")
                .ToListAsync();

            foreach (var schedule in schedulesToUpdate)
            {
                schedule.Status = "Finished";
            }

            await _dbContext.SaveChangesAsync();

            return Ok(new ApiResponse(200, "Examination teams retrieved successfully.", new { IsSuccess = true, examinerTeamsWithCriteriaBySpecialtyGroup }));
        }

        // Finished / Reviewed / Tested
        [HttpGet("AllTeamsForAdminEvaluation")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllTeamsForAdminEvaluation()
        {
            var appUserId = User.FindFirst("UserId")?.Value;
            var appUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (appUserId == null || appUserRole == null)
                return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));

            if (appUserRole != "Admin")
                return Unauthorized(new ApiResponse(401, "Unauthorized role for evaluation.", new { IsSuccess = false }));

            var admin = await _dbContext.Admins.FirstOrDefaultAsync(a => a.AppUserId == appUserId);
            if (admin == null)
                return NotFound(new ApiResponse(404, "Admin not found.", new { IsSuccess = false }));

            var evaluatorId = admin.Id;

            var activeAppointment = await _dbContext.AcademicAppointments.FirstOrDefaultAsync(a => a.Status == "Active");
            if (activeAppointment == null)
                return NotFound(new ApiResponse(404, "No active academic appointment found.", new { IsSuccess = true }));

            var examinerScheduleIds = await _dbContext.CommitteeDoctorSchedules
                .Select(cds => cds.ScheduleId)
                .ToListAsync();

            var adminTeams = await _dbContext.Teams
                .Include(t => t.FinalProjectIdea)
                .Include(t => t.Schedules)
                .Include(t => t.Students)
                    .ThenInclude(s => s.AppUser)
                .Where(t => t.HasProject && t.Schedules.Any(s => s.IsActive && s.AcademicAppointmentId == activeAppointment.Id))
                .AsNoTracking()
                .ToListAsync();

            if (adminTeams == null || !adminTeams.Any())
                return NotFound(new ApiResponse(404, "No teams found for Admin evaluation.", new { IsSuccess = false }));


            // Fetch active criteria for admin evaluation
            var teamSpecialties = adminTeams.Select(t => t.Specialty).Distinct().ToList();
            var activeCriterias = await _dbContext.Criterias
                .Where(c => c.IsActive && c.Evaluator == "Admin" && teamSpecialties.Contains(c.Specialty) && c.AcademicAppointmentId == activeAppointment.Id)
                .AsNoTracking()
                .ToListAsync();

            if (activeCriterias == null || !activeCriterias.Any())
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
                            ProjectId = t.FinalProjectIdea.ProjectId,
                            ProjectName = t.FinalProjectIdea.ProjectName,
                            ProjectDescription = t.FinalProjectIdea.ProjectDescription,
                            ScheduleId = t.Schedules.FirstOrDefault()?.Id,
                            ScheduleDate = t.Schedules.FirstOrDefault()?.ScheduleDate,
                            ScheduleStatus = t.Schedules.FirstOrDefault()?.Status,
                            TeamMembers = t.Students.Select(s => new TeamMemberDto
                            {
                                Id = s.Id,
                                FullName = s.FullName,
                                Email = s.AppUser.Email,
                                Specialty = s.Specialty,
                                InTeam = s.InTeam,
                                ProfilePicture = s.AppUser.ProfilePicture,
                            }).ToList()
                        }).ToList()
            }).ToList();

            // Update the status of schedules to "Finished"  
            var schedulesToUpdate = await _dbContext.Schedules
                .Where(s => s.Status != "Finished")
                .ToListAsync();

            foreach (var schedule in schedulesToUpdate)
            {
                schedule.Status = "Finished";
            }

            await _dbContext.SaveChangesAsync();

            return Ok(new ApiResponse(200, "Admin teams retrieved successfully.", new { IsSuccess = true, TeamsWithCriteriaBySpecialtyGroup }));
        }

        // Finished / Reviewed / Tested
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
                                               .FirstOrDefaultAsync(s => s.Id == scheduleId);

                if (schedule == null)
                    return NotFound(new ApiResponse(404, "Schedule not found.", new { IsSuccess = false }));

                var isSupervisor = schedule.TeamId == teamId && doctor.Id == schedule.Team.SupervisorId && schedule.CommitteeDoctorSchedules.Any(cds => cds.DoctorRole == "Supervisor" && cds.DoctorId == doctor.Id && cds.ScheduleId == scheduleId);
                var isExaminer = schedule.TeamId == teamId && schedule.CommitteeDoctorSchedules.Any(cds => cds.DoctorRole == "Examiner" && cds.DoctorId == doctor.Id && cds.ScheduleId == scheduleId);

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
                                                    .Where(e => e.TeamId == teamId && e.ScheduleId == scheduleId && e.AdminEvaluatorId == evaluatorId && e.DoctorEvaluatorId == null && e.EvaluatorRole == evaluatorRole)
                                                    .AsNoTracking()
                                                    .ToListAsync();
            }
            else
            {
                existingEvaluations = await _dbContext.Evaluations
                                                    .Include(e => e.Criteria)
                                                    .Where(e => e.TeamId == teamId && e.ScheduleId == scheduleId && e.AdminEvaluatorId == null && e.DoctorEvaluatorId == evaluatorId && e.EvaluatorRole == evaluatorRole)
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

        // Finished / Reviewed / Tested
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
                    .FirstOrDefaultAsync(s => s.Id == model.ScheduleId);
                if (schedule == null)
                    return NotFound(new ApiResponse(404, "Schedule not found.", new { IsSuccess = false }));

                var isSupervisor = schedule.TeamId == model.TeamId && doctor.Id == schedule.Team.SupervisorId && schedule.CommitteeDoctorSchedules.Any(cds => cds.DoctorRole == "Supervisor" && cds.DoctorId == doctor.Id && cds.ScheduleId == schedule.Id);
                var isExaminer = schedule.TeamId == model.TeamId && schedule.CommitteeDoctorSchedules.Any(cds => cds.DoctorRole == "Examiner" && cds.Id == schedule.Id && cds.DoctorId == doctor.Id);

                if (isSupervisor)
                    evaluatorRole = "Supervisor";
                else if (isExaminer)
                    evaluatorRole = "Examiner";
                else
                    return NotFound(new ApiResponse(404, "Doctor not authorized for this evaluation.", new { IsSuccess = false }));

                evaluatorId = doctor.Id;
            }

            if (evaluatorRole == "Admin")
            {
                if (model.StudentId == null)
                {
                    // Team Evaluation
                    foreach (var gradeItem in model.Grades)
                    {
                        var criteria = await _dbContext.Criterias.FirstOrDefaultAsync(c => c.Id == gradeItem.CriteriaId);
                        if (criteria == null)
                            return NotFound(new ApiResponse(404, $"Criteria not found with ID: '{gradeItem.CriteriaId}'.", new { IsSuccess = false }));

                        if (gradeItem.Grade < 0 || gradeItem.Grade > criteria.MaxGrade)
                            return BadRequest(new ApiResponse(400, $"Grade '{gradeItem.Grade}' is out of range for criteria ID: '{gradeItem.CriteriaId}'.", new { IsSuccess = false }));

                        var newEvaluation = new Evaluation
                        {
                            ScheduleId = model.ScheduleId,
                            CriteriaId = gradeItem.CriteriaId,
                            DoctorEvaluatorId = null,
                            AdminEvaluatorId = evaluatorId,
                            EvaluatorRole = evaluatorRole,
                            StudentId = null,
                            TeamId = model.TeamId,
                            Grade = gradeItem.Grade,
                        };

                        await _dbContext.Evaluations.AddAsync(newEvaluation);
                    }
                }
                else
                {
                    // Individual Student Evaluation
                    foreach (var gradeItem in model.Grades)
                    {
                        var criteria = await _dbContext.Criterias.FirstOrDefaultAsync(c => c.Id == gradeItem.CriteriaId);
                        if (criteria == null)
                            return NotFound(new ApiResponse(404, $"Criteria not found with ID: '{gradeItem.CriteriaId}'.", new { IsSuccess = false }));

                        if (gradeItem.Grade < 0 || gradeItem.Grade > criteria.MaxGrade)
                            return BadRequest(new ApiResponse(400, $"Grade '{gradeItem.Grade}' is out of range for criteria ID: '{gradeItem.CriteriaId}'.", new { IsSuccess = false }));

                        var newEvaluation = new Evaluation
                        {
                            ScheduleId = model.ScheduleId,
                            CriteriaId = gradeItem.CriteriaId,
                            DoctorEvaluatorId = null,
                            AdminEvaluatorId = evaluatorId,
                            EvaluatorRole = evaluatorRole,
                            StudentId = model.StudentId,
                            TeamId = model.TeamId,
                            Grade = gradeItem.Grade,
                        };
                        await _dbContext.Evaluations.AddAsync(newEvaluation);
                    }
                }
            }
            else
            {
                if (model.StudentId == null)
                {
                    // Team Evaluation
                    foreach (var gradeItem in model.Grades)
                    {
                        var criteria = await _dbContext.Criterias.FirstOrDefaultAsync(c => c.Id == gradeItem.CriteriaId);
                        if (criteria == null)
                            return NotFound(new ApiResponse(404, $"Criteria not found with ID: '{gradeItem.CriteriaId}'.", new { IsSuccess = false }));

                        if (gradeItem.Grade < 0 || gradeItem.Grade > criteria.MaxGrade)
                            return BadRequest(new ApiResponse(400, $"Grade '{gradeItem.Grade}' is out of range for criteria ID: '{gradeItem.CriteriaId}'.", new { IsSuccess = false }));

                        var newEvaluation = new Evaluation
                        {
                            ScheduleId = model.ScheduleId,
                            CriteriaId = gradeItem.CriteriaId,
                            DoctorEvaluatorId = evaluatorId,
                            AdminEvaluatorId = null,
                            EvaluatorRole = evaluatorRole,
                            StudentId = null,
                            TeamId = model.TeamId,
                            Grade = gradeItem.Grade,
                        };

                        await _dbContext.Evaluations.AddAsync(newEvaluation);
                    }
                }
                else
                {
                    // Individual Student Evaluation
                    foreach (var gradeItem in model.Grades)
                    {
                        var criteria = await _dbContext.Criterias.FirstOrDefaultAsync(c => c.Id == gradeItem.CriteriaId);
                        if (criteria == null)
                            return NotFound(new ApiResponse(404, $"Criteria not found with ID: '{gradeItem.CriteriaId}'.", new { IsSuccess = false }));

                        if (gradeItem.Grade < 0 || gradeItem.Grade > criteria.MaxGrade)
                            return BadRequest(new ApiResponse(400, $"Grade '{gradeItem.Grade}' is out of range for criteria ID: '{gradeItem.CriteriaId}'.", new { IsSuccess = false }));

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
                        };
                        await _dbContext.Evaluations.AddAsync(newEvaluation);
                    }
                }
                await _dbContext.SaveChangesAsync();

                // Mark HasCompletedEvaluation as true for the relevant CommitteeDoctorSchedules
                var committeeDoctorSchedules = await _dbContext.CommitteeDoctorSchedules
                    .Where(cds => cds.ScheduleId == model.ScheduleId && cds.DoctorId == evaluatorId && cds.DoctorRole == evaluatorRole)
                    .ToListAsync();

                foreach (var cds in committeeDoctorSchedules)
                {
                    cds.HasCompletedEvaluation = true;
                }

                await _dbContext.SaveChangesAsync();
            }

            return Ok(new ApiResponse(200, "Grades submitted successfully.", new { IsSuccess = true }));
        }

        // Finished / Reviewed / Tested
        [HttpGet("StudentGrades")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetStudentGrades()
        {
            var appUserId = User.FindFirst("UserId")?.Value;
            var appUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (appUserId == null || appUserRole == null)
                return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));

            if (appUserRole != "Student")
                return Unauthorized(new ApiResponse(401, "Unauthorized role for viewing grades.", new { IsSuccess = false }));

            var student = await _dbContext.Students.Include(s => s.Team).FirstOrDefaultAsync(s => s.AppUserId == appUserId);
            if (student == null)
                return NotFound(new ApiResponse(404, "Student not found.", new { IsSuccess = false }));

            var studentId = student.Id;
            var teamId = student.TeamId;

            // Fetch all evaluations for the student (individual and team-based)
            var evaluations = await _dbContext.Evaluations
                .Include(e => e.Criteria)
                .Where(e => e.StudentId == studentId || (e.TeamId == teamId && e.TeamId != null))
                .AsNoTracking()
                .ToListAsync();

            if (evaluations == null || !evaluations.Any())
                return NotFound(new ApiResponse(404, "No grades found for the student.", new { IsSuccess = false }));

            var supervisorEvaluations = evaluations
                .Where(e => e.EvaluatorRole == "Supervisor")
                .GroupBy(e => e.Criteria.Name)
                .Select(g =>
                {
                    var criteria = g.First().Criteria;
                    var totalGrade = g.Sum(e => e.Grade);

                    return new
                    {
                        CriteriaId = criteria.Id,
                        CriteriaName = criteria.Name,
                        CriteriaDescription = criteria.Description,
                        GivenTo = criteria.GivenTo,
                        MaximumGrade = criteria.MaxGrade,
                        Grade = totalGrade,
                        EvaluatorRole = "Supervisor"
                    };
                });

            var adminEvaluations = evaluations
                .Where(e => e.EvaluatorRole == "Admin")
                .GroupBy(e => e.Criteria.Name)
                .Select(g =>
                {
                    var criteria = g.First().Criteria;
                    var totalGrade = g.Sum(e => e.Grade);

                    return new
                    {
                        CriteriaId = criteria.Id,
                        CriteriaName = criteria.Name,
                        CriteriaDescription = criteria.Description,
                        GivenTo = criteria.GivenTo,
                        MaximumGrade = criteria.MaxGrade,
                        Grade = totalGrade,
                        EvaluatorRole = "Admin"
                    };
                });

            var examinerEvaluations = evaluations
                .Where(e => e.EvaluatorRole == "Examiner")
                .GroupBy(e => e.Criteria.Name)
                .Select(g =>
                {
                    var criteria = g.First().Criteria;
                    var totalGrade = g.Sum(e => e.Grade);
                    var averageGrade = totalGrade / g.Count();

                    return new
                    {
                        CriteriaId = criteria.Id,
                        CriteriaName = criteria.Name,
                        CriteriaDescription = criteria.Description,
                        GivenTo = criteria.GivenTo,
                        MaximumGrade = criteria.MaxGrade,
                        Grade = averageGrade,
                        EvaluatorRole = "Examiner"
                    };
                });

            var combinedEvaluations = supervisorEvaluations
                .Concat(adminEvaluations)
                .Concat(examinerEvaluations)
                .ToList();

            return Ok(new ApiResponse(200, "Student grades retrieved successfully.", new { IsSuccess = true, Grades = combinedEvaluations }));
        }
    }
}