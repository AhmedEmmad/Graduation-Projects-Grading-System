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

        // Finished / Reviewed / Tested / Edited / D
        [HttpPost("SubmitGrades")]
        [Authorize(Roles = "Admin, Doctor")]
        public async Task<IActionResult> SubmitGrades([FromBody] SubmitEvaluationDto model)
        {
            if (model is null)
                return BadRequest(new ApiResponse(400, "Invalid input data.", new { IsSuccess = false }));

            var appUserId = User.FindFirst("UserId")?.Value;
            if (appUserId == null)
                return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));
            var appUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (appUserRole == null)
                return Unauthorized(new ApiResponse(401, "Unauthorized role.", new { IsSuccess = false }));
            if (appUserRole != "Admin" && appUserRole != "Doctor")
                return Unauthorized(new ApiResponse(401, "Unauthorized role for evaluation.", new { IsSuccess = false }));

            int? evaluatorId = null;
            string evaluatorRole = string.Empty;

            if (model.DoctorId != null)
            {
                var doctor = await _dbContext.Doctors.FirstOrDefaultAsync(d => d.Id == model.DoctorId);
                if (doctor == null)
                    return NotFound(new ApiResponse(404, "Doctor not found.", new { IsSuccess = false }));
                evaluatorId = doctor.Id;
                evaluatorRole = "Doctor";
            }
            else
            {
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
                    evaluatorId = doctor.Id;
                    evaluatorRole = "Doctor";
                }
            }

            var activeAppointment = await _dbContext.AcademicAppointments.FirstOrDefaultAsync(a => a.Status == StatusType.Active.ToString());
            if (activeAppointment == null)
                return NotFound(new ApiResponse(404, "No active academic appointment found, You cannot evaluate now.", new { IsSuccess = false }));

            if (evaluatorRole == "Doctor")
            {
                var schedule = await _dbContext.Schedules
                    .Include(s => s.CommitteeDoctorSchedules)
                    .Include(s => s.Team)
                    .FirstOrDefaultAsync(s => s.Id == model.ScheduleId &&
                                              s.TeamId == model.TeamId &&
                                              s.AcademicAppointmentId == activeAppointment.Id);
                if (schedule == null)
                    return NotFound(new ApiResponse(404, "Schedule not found.", new { IsSuccess = false }));

                var isSupervisor = (schedule.TeamId == model.TeamId) &&
                                   (evaluatorId == schedule.Team.SupervisorId) &&
                                   (schedule.CommitteeDoctorSchedules.Any(cds => cds.DoctorRole == "Supervisor" && cds.DoctorId == evaluatorId && cds.ScheduleId == schedule.Id));
                var isExaminer = (schedule.CommitteeDoctorSchedules.Any(cds => cds.DoctorId == evaluatorId && cds.ScheduleId == schedule.Id && cds.DoctorRole == "Examiner"));

                if (isSupervisor)
                    evaluatorRole = "Supervisor";
                else if (isExaminer)
                    evaluatorRole = "Examiner";
                else
                    return NotFound(new ApiResponse(404, "Doctor not authorized for this evaluation.", new { IsSuccess = false }));
            }

            foreach (var gradeItem in model.Grades)
            {
                var existingEvaluation = await _dbContext.Evaluations
                    .FirstOrDefaultAsync(e =>
                        e.ScheduleId == model.ScheduleId &&
                        e.CriteriaId == gradeItem.CriteriaId &&
                        e.TeamId == model.TeamId &&
                        ((e.StudentId == model.StudentId) || (e.StudentId == null && model.StudentId == null)) &&
                        e.EvaluatorRole == evaluatorRole &&
                        ((evaluatorRole == "Admin" && e.AdminEvaluatorId == evaluatorId) ||
                         (evaluatorRole != "Admin" && e.DoctorEvaluatorId == evaluatorId)) &&
                        e.AcademicAppointmentId == activeAppointment.Id);

                if (existingEvaluation != null)
                {
                    if (existingEvaluation.Grade != gradeItem.Grade)
                    {
                        existingEvaluation.Grade = gradeItem.Grade;
                        existingEvaluation.LastUpdatedAt = DateTime.Now.AddHours(1);
                        _dbContext.Evaluations.Update(existingEvaluation);
                    }
                }
                else
                {
                    var criteria = await _dbContext.Criterias.FirstOrDefaultAsync(c => c.Id == gradeItem.CriteriaId &&
                                                                                       c.AcademicAppointmentId == activeAppointment.Id &&
                                                                                       c.Evaluator == evaluatorRole);
                    if (criteria == null)
                        return NotFound(new ApiResponse(404, "Criteria not found.", new { IsSuccess = false }));

                    if (gradeItem.Grade < 0 || gradeItem.Grade > criteria.MaxGrade)
                        return BadRequest(new ApiResponse(400, $"Grade '{gradeItem.Grade}' is out of range.", new { IsSuccess = false }));

                    var newEvaluation = new Evaluation
                    {
                        ScheduleId = model.ScheduleId,
                        CriteriaId = gradeItem.CriteriaId,
                        DoctorEvaluatorId = (evaluatorRole == "Admin") ? null : evaluatorId,
                        AdminEvaluatorId = (evaluatorRole == "Admin") ? evaluatorId : null,
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

            // Mark evaluation as completed for doctor (if doctor or admin evaluating as doctor)
            if (evaluatorRole == "Supervisor" || evaluatorRole == "Examiner")
            {
                var committeeDoctorSchedule = await _dbContext.CommitteeDoctorSchedules
                    .FirstOrDefaultAsync(cds => cds.ScheduleId == model.ScheduleId &&
                                                cds.DoctorId == evaluatorId &&
                                                cds.DoctorRole == evaluatorRole);

                if (committeeDoctorSchedule != null)
                {
                    committeeDoctorSchedule.HasCompletedEvaluation = true;
                    _dbContext.CommitteeDoctorSchedules.Update(committeeDoctorSchedule);
                    await _dbContext.SaveChangesAsync();
                }
            }

            if (model.ScheduleId.HasValue && model.TeamId.HasValue)
            {
                var schedule = await _dbContext.Schedules
                    .Include(s => s.CommitteeDoctorSchedules)
                    .Include(s => s.Team)
                        .ThenInclude(t => t.Students)
                    .FirstOrDefaultAsync(s => s.Id == model.ScheduleId &&
                                              s.AcademicAppointmentId == activeAppointment.Id &&
                                              s.TeamId == model.TeamId);

                if (schedule != null)
                {
                    var allExaminersCompleted = await _dbContext.CommitteeDoctorSchedules
                        .Where(cds => cds.ScheduleId == schedule.Id && cds.DoctorRole == "Examiner")
                        .AllAsync(cds => cds.HasCompletedEvaluation);

                    var supervisorCompleted = await _dbContext.CommitteeDoctorSchedules
                        .Where(cds => cds.ScheduleId == schedule.Id && cds.DoctorRole == "Supervisor")
                        .AllAsync(cds => cds.HasCompletedEvaluation);

                    // Check all students in team evaluated by all "Team" and "Student" criteria
                    bool allCriteriaEvaluated = false;
                    if (schedule.Team != null && schedule.Team.Students != null)
                    {
                        var teamId = schedule.Team.Id;
                        var studentIds = schedule.Team.Students.Select(st => st.Id).ToList();

                        // Get all criteria for this team and appointment
                        var criterias = await _dbContext.Criterias
                            .Where(c => c.AcademicAppointmentId == activeAppointment.Id && c.Specialty == schedule.Team.Specialty)
                            .ToListAsync();

                        var teamCriterias = criterias.Where(c => c.GivenTo == "Team").ToList();
                        var studentCriterias = criterias.Where(c => c.GivenTo == "Student").ToList();

                        bool allTeamCriteriaEvaluated = true;
                        foreach (var criteria in teamCriterias)
                        {
                            var hasEval = await _dbContext.Evaluations.AnyAsync(e =>
                                e.CriteriaId == criteria.Id &&
                                e.TeamId == teamId &&
                                e.StudentId == null &&
                                e.ScheduleId == schedule.Id &&
                                e.AcademicAppointmentId == activeAppointment.Id);
                            if (!hasEval)
                            {
                                allTeamCriteriaEvaluated = false;
                                break;
                            }
                        }

                        bool allStudentCriteriaEvaluated = true;
                        foreach (var criteria in studentCriterias)
                        {
                            foreach (var studentId in studentIds)
                            {
                                var hasEval = await _dbContext.Evaluations.AnyAsync(e =>
                                    e.CriteriaId == criteria.Id &&
                                    e.TeamId == teamId &&
                                    e.StudentId == studentId &&
                                    e.ScheduleId == schedule.Id &&
                                    e.AcademicAppointmentId == activeAppointment.Id);
                                if (!hasEval)
                                {
                                    allStudentCriteriaEvaluated = false;
                                    break;
                                }
                            }
                            if (!allStudentCriteriaEvaluated) break;
                        }

                        allCriteriaEvaluated = allTeamCriteriaEvaluated && allStudentCriteriaEvaluated;
                    }

                    if (allExaminersCompleted && supervisorCompleted && allCriteriaEvaluated)
                    {
                        schedule.IsGraded = true;
                        schedule.Status = "Finished";
                        _dbContext.Schedules.Update(schedule);
                        await _dbContext.SaveChangesAsync();
                    }
                }
            }

            return Ok(new ApiResponse(200, "Grades submitted successfully.", new { IsSuccess = true }));
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
                .Where(e => (e.StudentId == studentId || (e.StudentId == null && e.TeamId == teamId))
                    && e.AcademicAppointmentId == activeAppointment.Id)
                .AsNoTracking()
                .ToListAsync();

            if (evaluations == null || !evaluations.Any())
                return NotFound(new ApiResponse(404, "No grades found for the student.", new { IsSuccess = false }));

            // Get all criteria for this specialty and appointment
            var criteriaList = await _dbContext.Criterias
                .Where(c => c.Specialty == student.Team.Specialty && c.AcademicAppointmentId == activeAppointment.Id)
                .AsNoTracking()
                .ToListAsync();

            var gradesResult = new List<object>();
            double totalGrade = 0.0;

            foreach (var criteria in criteriaList)
            {
                // Supervisor: get only the grade given by supervisor for this student/criteria
                if (criteria.Evaluator == "Supervisor")
                {
                    var grade = evaluations
                        .Where(e => e.CriteriaId == criteria.Id && e.EvaluatorRole == "Supervisor"
                            && ((criteria.GivenTo == "Student" && e.StudentId == studentId && e.TeamId == teamId)
                                || (criteria.GivenTo == "Team" && e.StudentId == null && e.TeamId == teamId)))
                        .Select(e => (double?)e.Grade)
                        .FirstOrDefault();

                    if (grade.HasValue)
                    {
                        gradesResult.Add(new
                        {
                            CriteriaId = criteria.Id,
                            CriteriaName = criteria.Name,
                            CriteriaDescription = criteria.Description,
                            GivenTo = criteria.GivenTo,
                            MaximumGrade = criteria.MaxGrade,
                            Grade = Math.Round(grade.Value),
                            EvaluatorRole = "Supervisor"
                        });
                        totalGrade += grade.Value;
                    }
                }
                // Admin: get only the grade given by admin for this student/criteria
                else if (criteria.Evaluator == "Admin")
                {
                    var grade = evaluations
                        .Where(e => e.CriteriaId == criteria.Id && e.EvaluatorRole == "Admin"
                            && ((criteria.GivenTo == "Student" && e.StudentId == studentId && e.TeamId == teamId)
                                || (criteria.GivenTo == "Team" && e.StudentId == null && e.TeamId == teamId)))
                        .Select(e => (double?)e.Grade)
                        .FirstOrDefault();

                    if (grade.HasValue)
                    {
                        gradesResult.Add(new
                        {
                            CriteriaId = criteria.Id,
                            CriteriaName = criteria.Name,
                            CriteriaDescription = criteria.Description,
                            GivenTo = criteria.GivenTo,
                            MaximumGrade = criteria.MaxGrade,
                            Grade = Math.Round(grade.Value),
                            EvaluatorRole = "Admin"
                        });
                        totalGrade += grade.Value;
                    }
                }
                // Examiner: get average of all grades for this student/criteria
                else if (criteria.Evaluator == "Examiner")
                {
                    var grades = evaluations
                        .Where(e => e.CriteriaId == criteria.Id && e.EvaluatorRole == "Examiner"
                            && ((criteria.GivenTo == "Student" && e.StudentId == studentId && e.TeamId == teamId)
                                || (criteria.GivenTo == "Team" && e.StudentId == null && e.TeamId == teamId)))
                        .Select(e => e.Grade)
                        .ToList();

                    if (grades.Any())
                    {
                        var avg = grades.Average();
                        gradesResult.Add(new
                        {
                            CriteriaId = criteria.Id,
                            CriteriaName = criteria.Name,
                            CriteriaDescription = criteria.Description,
                            GivenTo = criteria.GivenTo,
                            MaximumGrade = criteria.MaxGrade,
                            Grade = Math.Round(avg),
                            EvaluatorRole = "Examiner"
                        });
                        totalGrade += avg;
                    }
                }
            }

            totalGrade = Math.Round(totalGrade);

            if (_dbContext.StudentTotalGrades.Any(stg => stg.StudentId == studentId && stg.TeamId == teamId))
            {
                // Update existing total grade
                var existingTotalGrade = await _dbContext.StudentTotalGrades.FirstOrDefaultAsync(stg => stg.StudentId == studentId && stg.TeamId == teamId);
                if (existingTotalGrade != null)
                {
                    existingTotalGrade.Total = totalGrade;
                    _dbContext.StudentTotalGrades.Update(existingTotalGrade);
                }
            }
            else
            {
                var studentTotalGrade = new StudentTotalGrade 
                {
                    StudentId = studentId,
                    TeamId = teamId,
                    Total = totalGrade
                };
                await _dbContext.StudentTotalGrades.AddAsync(studentTotalGrade);
            }
            await _dbContext.SaveChangesAsync();

            return Ok(new ApiResponse(200, "Student grades retrieved successfully.", new { IsSuccess = true, Grades = gradesResult, totalGrade }));
        }

        // Finished / Reviewed / Tested / Edited / I
        [HttpGet("ExportGradesForSpecialty/{specialty}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportGradesForSpecialty(string specialty)
        {
            var adminAppUserId = User.FindFirst("UserId")?.Value;
            if (adminAppUserId == null)
                return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));
            var appUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (appUserRole == null)
                return Unauthorized(new ApiResponse(401, "Unauthorized role.", new { IsSuccess = false }));

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
                            t.Schedules.Any(s => s.AcademicAppointmentId == activeAppointment.Id) &&
                            t.AcademicAppointmentId == activeAppointment.Id)
                .AsNoTracking()
                .OrderBy(t => t.Name)
                .ToListAsync();

            if (teams == null || !teams.Any())
                return NotFound(new ApiResponse(404, $"No teams found or there exist teams not evaluated for this specialty: '{specialty}'.", new { IsSuccess = false }));

            var criterias = await _dbContext.Criterias
                .Where(c => c.IsActive == true &&
                            c.Specialty == specialty &&
                            c.AcademicAppointmentId == activeAppointment.Id)
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();

            if (criterias == null || !criterias.Any())
                return NotFound(new ApiResponse(404, $"No criterias found for this specialty: '{specialty}'.", new { IsSuccess = false }));

            var supervisorCriteria = criterias.Where(c => c.Evaluator == "Supervisor").ToList();
            var adminCriteria = criterias.Where(c => c.Evaluator == "Admin").ToList();
            var examinerCriteria = criterias.Where(c => c.Evaluator == "Examiner").ToList();

            // Validation: all teams must be fully evaluated
            foreach (var team in teams)
            {
                foreach (var student in team.Students)
                {
                    foreach (var criteria in supervisorCriteria)
                    {
                        var supervisorEvaluation = await _dbContext.Evaluations
                            .Where(e => e.CriteriaId == criteria.Id &&
                                        e.TeamId == team.Id &&
                                        e.AcademicAppointmentId == activeAppointment.Id &&
                                        (e.StudentId == student.Id || e.StudentId == null) &&
                                        e.EvaluatorRole == "Supervisor")
                            .OrderBy(e => e.EvaluationDate)
                            .ToListAsync();

                        var totalSupervisors = await _dbContext.CommitteeDoctorSchedules
                            .Where(cds => cds.Schedule.TeamId == team.Id &&
                                          cds.Schedule.AcademicAppointmentId == activeAppointment.Id &&
                                          cds.DoctorRole == "Supervisor")
                            .CountAsync();

                        if (supervisorEvaluation.Count != totalSupervisors)
                        {
                            return BadRequest(new ApiResponse(400, $"Not all supervisors have evaluated remaining teams.", new { IsSuccess = false }));
                        }
                    }
                }
            }

            foreach (var team in teams)
            {
                foreach (var student in team.Students)
                {
                    foreach (var criteria in adminCriteria)
                    {
                        var adminEvaluation = await _dbContext.Evaluations
                            .Where(e => e.CriteriaId == criteria.Id &&
                                        e.TeamId == team.Id &&
                                        e.AcademicAppointmentId == activeAppointment.Id &&
                                        (e.StudentId == student.Id || e.StudentId == null) &&
                                        e.EvaluatorRole == "Admin")
                            .OrderBy(e => e.EvaluationDate)
                            .ToListAsync();

                        if (!adminEvaluation.Any())
                        {
                            return BadRequest(new ApiResponse(400, $"Admin has not evaluated remaining teams.", new { IsSuccess = false }));
                        }
                    }
                }
            }

            foreach (var team in teams)
            {
                foreach (var student in team.Students)
                {
                    foreach (var criteria in examinerCriteria)
                    {
                        var evaluations = await _dbContext.Evaluations
                            .Where(e => e.CriteriaId == criteria.Id &&
                                        e.TeamId == team.Id &&
                                        e.AcademicAppointmentId == activeAppointment.Id &&
                                        (e.StudentId == student.Id || e.StudentId == null) &&
                                        e.EvaluatorRole == "Examiner")
                            .OrderBy(e => e.EvaluationDate)
                            .ToListAsync();

                        var totalExaminers = await _dbContext.CommitteeDoctorSchedules
                            .Where(cds => cds.Schedule.TeamId == team.Id &&
                                          cds.Schedule.AcademicAppointmentId == activeAppointment.Id &&
                                          cds.DoctorRole == "Examiner")
                            .CountAsync();

                        if (evaluations.Count != totalExaminers)
                        {
                            return BadRequest(new ApiResponse(400, $"Not all examiners have evaluated remaining teams.", new { IsSuccess = false }));
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

                worksheet.Cells[1, col++].Value = "Total";

                int row = 2;
                foreach (var team in teams)
                {
                    foreach (var student in team.Students.OrderBy(s => s.FullName))
                    {
                        col = 1;
                        worksheet.Cells[row, col++].Value = student.FullName;
                        worksheet.Cells[row, col++].Value = student.AppUser.Email;
                        worksheet.Cells[row, col++].Value = team.Name;

                        // Get all evaluations for this student/team
                        var evaluations = await _dbContext.Evaluations
                            .Include(e => e.Criteria)
                            .Where(e => (e.StudentId == student.Id && e.TeamId == team.Id) ||
                                        (e.StudentId == null && e.TeamId == team.Id))
                            .AsNoTracking()
                            .ToListAsync();

                        double totalGrade = 0.0;

                        // Admin grades
                        foreach (var c in adminCriteria)
                        {
                            double? grade = evaluations
                                .Where(e => e.CriteriaId == c.Id && e.EvaluatorRole == "Admin" &&
                                    ((c.GivenTo == "Student" && e.StudentId == student.Id && e.TeamId == team.Id) ||
                                     (c.GivenTo == "Team" && e.StudentId == null && e.TeamId == team.Id)))
                                .Select(e => (double?)e.Grade)
                                .FirstOrDefault();

                            worksheet.Cells[row, col].Value = grade.HasValue ? Math.Round(grade.Value).ToString() : "N/A";
                            if (grade.HasValue)
                                worksheet.Cells[row, col].Style.Numberformat.Format = "0.00";
                            if (grade.HasValue) totalGrade += grade.Value;
                            col++;
                        }

                        // Supervisor grades
                        foreach (var c in supervisorCriteria)
                        {
                            double? grade = evaluations
                                .Where(e => e.CriteriaId == c.Id && e.EvaluatorRole == "Supervisor" &&
                                    ((c.GivenTo == "Student" && e.StudentId == student.Id && e.TeamId == team.Id) ||
                                     (c.GivenTo == "Team" && e.StudentId == null && e.TeamId == team.Id)))
                                .Select(e => (double?)e.Grade)
                                .FirstOrDefault();

                            worksheet.Cells[row, col].Value = grade.HasValue ? Math.Round(grade.Value).ToString() : "N/A";
                            if (grade.HasValue)
                                worksheet.Cells[row, col].Style.Numberformat.Format = "0.00";
                            if (grade.HasValue) totalGrade += grade.Value;
                            col++;
                        }

                        // Examiner grades (average)
                        foreach (var c in examinerCriteria)
                        {
                            var grades = evaluations
                                .Where(e => e.CriteriaId == c.Id && e.EvaluatorRole == "Examiner" &&
                                    ((c.GivenTo == "Student" && e.StudentId == student.Id && e.TeamId == team.Id) ||
                                     (c.GivenTo == "Team" && e.StudentId == null && e.TeamId == team.Id)))
                                .Select(e => e.Grade)
                                .ToList();

                            double? avg = grades.Any() ? (double?)grades.Average() : null;
                            worksheet.Cells[row, col].Value = avg.HasValue ? Math.Round(avg.Value).ToString() : "N/A";
                            if (avg.HasValue)
                                worksheet.Cells[row, col].Style.Numberformat.Format = "0.00";
                            if (avg.HasValue) totalGrade += avg.Value;
                            col++;
                        }

                        // Get total from StudentTotalGrades if exists, else use calculated
                        var studentTotal = await _dbContext.StudentTotalGrades
                            .Where(stg => stg.StudentId == student.Id)
                            .OrderByDescending(stg => stg.Total)
                            .Select(stg => stg.Total)
                            .FirstOrDefaultAsync();

                        worksheet.Cells[row, col].Value = studentTotal.HasValue ? Math.Round(studentTotal.Value).ToString() : Math.Round(totalGrade).ToString();
                        worksheet.Cells[row, col].Style.Numberformat.Format = "0.00";

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

            var admin = await _dbContext.Admins.FirstOrDefaultAsync(a => a.AppUserId == appUserId);
            if (admin == null)
                return NotFound(new ApiResponse(404, "Admin not found.", new { IsSuccess = false }));

            var doctor = await _dbContext.Doctors.FirstOrDefaultAsync(d => d.Id == doctorId);
            if (doctor == null)
                return NotFound(new ApiResponse(404, "Doctor not found.", new { IsSuccess = false }));

            var activeAppointment = await _dbContext.AcademicAppointments.FirstOrDefaultAsync(a => a.Status == StatusType.Active.ToString());
            if (activeAppointment == null)
                return NotFound(new ApiResponse(404, "No active academic appointment found, You cannot evaluate now.", new { IsSuccess = false }));

            // Get all schedules for this doctor (as supervisor or examiner) in the active appointment
            var doctorSchedules = await _dbContext.CommitteeDoctorSchedules
                .Include(cds => cds.Schedule)
                    .ThenInclude(s => s.Team)
                        .ThenInclude(t => t.Students)
                            .ThenInclude(st => st.AppUser)
                .Include(cds => cds.Schedule)
                    .ThenInclude(s => s.Team)
                        .ThenInclude(t => t.FinalProjectIdea)
                .Include(cds => cds.Schedule.Team.Supervisor)
                .Where(cds => cds.DoctorId == doctorId &&
                              cds.Schedule.AcademicAppointmentId == activeAppointment.Id &&
                              cds.Schedule.IsGraded == false)
                .AsNoTracking()
                .ToListAsync();

            if (doctorSchedules == null || !doctorSchedules.Any())
                return NotFound(new ApiResponse(404, "No teams found without evaluation for this doctor.", new { IsSuccess = false }));

            // Get all teams from these schedules
            var schedules = doctorSchedules.Select(cds => cds.Schedule)
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
                var specialtyDoctorSchedules = doctorSchedules
                    .Where(cds => cds.Schedule.Team.Specialty == specialty)
                    .ToList();

                var isExaminer = specialtyDoctorSchedules.Any(cds => cds.DoctorRole == "Examiner");
                var isSupervisor = specialtyDoctorSchedules.Any(cds => cds.DoctorRole == "Supervisor");

                var criteriasForExaminer = activeCriterias
                    .Where(c => c.Specialty == specialty && c.Evaluator == "Examiner" && c.IsActive)
                    .ToList();

                var criteriasForSupervisor = activeCriterias
                    .Where(c => c.Specialty == specialty && c.Evaluator == "Supervisor" && c.IsActive)
                    .ToList();

                var criterias = new List<CriteriaDto>();
                if (isExaminer)
                    criterias.AddRange(criteriasForExaminer.Select(c => new CriteriaDto
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
                    }));
                if (isSupervisor)
                    criterias.AddRange(criteriasForSupervisor.Select(c => new CriteriaDto
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
                    }));

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

                    var requiredCriterias = criterias.Where(c => (c.Evaluator == "Examiner" && isExaminer) ||
                                                                (c.Evaluator == "Supervisor" && isSupervisor)).ToList();

                    bool allCriteriasEvaluated = true;
                    foreach (var criteria in requiredCriterias)
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
                            Criterias = criterias.Where(c =>
                                        (c.Evaluator == "Examiner" && isExaminer) ||
                                        (c.Evaluator == "Supervisor" && isSupervisor)).ToList(),
                            TeamMembers = teamMemberDtos
                        });
                    }
                }
                if (teamsWithoutEvaluation.Any())
                {
                    teamsWithCriteriaBySpecialtyGroup.Add(new TeamsWithCriteriaBySpecialtyGroupDto
                    {
                        Specialty = specialty,
                        Criterias = criterias,
                        Teams = teamsWithoutEvaluation
                    });
                }
            }
            if (!teamsWithCriteriaBySpecialtyGroup.Any())
                return NotFound(new ApiResponse(404, "All teams for this doctor have been fully evaluated.", new { IsSuccess = false }));

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

    }
}