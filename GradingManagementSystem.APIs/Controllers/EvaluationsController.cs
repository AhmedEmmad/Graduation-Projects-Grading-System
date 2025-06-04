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
                            Specialty = t.Specialty,
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
                                                           e.EvaluatorRole == evaluatorRole)
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
                                                           e.EvaluatorRole == evaluatorRole)
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
                                              (e.DoctorEvaluatorId == evaluatorId || e.DoctorEvaluatorId == null));

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
                    var criteria = await _dbContext.Criterias.FirstOrDefaultAsync(c => c.Id == gradeItem.CriteriaId);
                    if (criteria == null)
                        return NotFound(new ApiResponse(404, $"Criteria not found.", new { IsSuccess = false }));

                    if (gradeItem.Grade < 0 || gradeItem.Grade > criteria.MaxGrade)
                        return BadRequest(new ApiResponse(400, $"Grade '{gradeItem.Grade}' is out of range.", new { IsSuccess = false }));

                    var currentActiveAcademicYearAppointment = await _dbContext.AcademicAppointments.FirstOrDefaultAsync(a => a.Status == "Active");
                    if (currentActiveAcademicYearAppointment == null)
                        return BadRequest(new ApiResponse(400, "Can not evaluate now.", new { IsSuccess = false }));
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
                        //AcademicAppointmentId = currentActiveAcademicYearAppointment.Id,
                    };

                    await _dbContext.Evaluations.AddAsync(newEvaluation);
                }
            }

            await _dbContext.SaveChangesAsync();

            var committeeDoctorSchedule = await _dbContext.CommitteeDoctorSchedules.FirstOrDefaultAsync(cds => cds.ScheduleId == model.ScheduleId && cds.DoctorId == evaluatorId);

            if (committeeDoctorSchedule != null)
            {
                committeeDoctorSchedule.HasCompletedEvaluation = true;
                _dbContext.CommitteeDoctorSchedules.Update(committeeDoctorSchedule);
            }

            await _dbContext.SaveChangesAsync();

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

            var activeAppointment = await _dbContext.AcademicAppointments.FirstOrDefaultAsync(a => a.Status == "Active");
            if (activeAppointment == null)
                return NotFound(new ApiResponse(404, "No active academic appointment found in this time.", new { IsSuccess = true }));

            var teams = await _dbContext.Teams
                                        .Include(t => t.Students)
                                            .ThenInclude(s => s.AppUser)
                                        .Include(t => t.Schedules)
                                        .Include(t => t.Evaluations)
                                        .Where(t => t.Specialty == specialty &&
                                                    t.HasProject == true &&
                                                    t.Schedules.Any(s => s.IsActive &&
                                                                    s.AcademicAppointmentId == activeAppointment.Id &&
                                                                    s.Status == "Finished"
                                                                   )
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
    }
}