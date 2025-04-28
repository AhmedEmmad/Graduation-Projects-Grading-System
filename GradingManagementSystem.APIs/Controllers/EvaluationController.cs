using GradingManagementSystem.Core.CustomResponses;
using GradingManagementSystem.Core.DTOs;
using GradingManagementSystem.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GradingManagementSystem.Repository.Data.DbContexts;
using Microsoft.EntityFrameworkCore;

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

        // Finished
        [HttpGet("AllTeams")]
        [Authorize(Roles = "Admin,Doctor,Student")]
        public async Task<IActionResult> GetAllTeamsToEvaluating()
        {
            var appUserId = User.FindFirst("UserId")?.Value;
            var appUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (appUserId == null || appUserRole == null)
                return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));

            if (appUserRole != "Admin" && appUserRole != "Doctor")
                return Unauthorized(new ApiResponse(401, "Unauthorized role for evaluation.", new { IsSuccess = false }));

            var evaluatorId = 0;
            var evaluatorRole = "";

            if (appUserRole == "Admin")
            {
                var admin = await _dbContext.Admins.FirstOrDefaultAsync(a => a.AppUserId ==  appUserId);
                if(admin == null)
                    return NotFound(new ApiResponse(404, "Admin not found.", new { IsSuccess = false }));
                evaluatorId = admin.Id;
                evaluatorRole = "Admin";
            }

            if (appUserRole == "Doctor")
            {
                var doctor = await _dbContext.Doctors.FirstOrDefaultAsync(d => d.AppUserId == appUserId);
                if (doctor == null)
                    return NotFound(new ApiResponse(404, "Doctor not found.", new { IsSuccess = false }));
                evaluatorId = doctor.Id;
                evaluatorRole = "Doctor";
            }
            
            if (evaluatorRole == "Admin")
            {
                var criterias = await _dbContext.Criterias.Include(c => c.Teams)
                                                          .Where(c => c.IsActive == true && c.Evaluator == "Admin")
                                                          .AsNoTracking()
                                                          .ToListAsync();

                if (criterias == null || criterias.Count == 0 || !criterias.Any())
                    return NotFound(new ApiResponse(404, "No active criteria found for Admin evaluation.", new { IsSuccess = false }));

                var adminTeams = await _dbContext.Teams.Include(t => t.Criterias)
                                                       .Include(t => t.FinalProjectIdea)
                                                        .Include(t => t.Students)
                                                            .ThenInclude(s => s.AppUser)
                                                      .Where(t => t.Criterias.Any(c => c.Evaluator == "Admin" && c.IsActive == true))
                                                      .AsNoTracking()
                                                      .ToListAsync();

                if (adminTeams == null || adminTeams.Count == 0 || !adminTeams.Any())
                    return NotFound(new ApiResponse(404, "No teams found for Admin evaluation.", new { IsSuccess = false }));

                // Get teams with their admin criteria grouped by specialty
                var adminTeamsAndCriteriasBySpecialty = await _dbContext.Teams
                    .Include(t => t.Criterias)
                    .Include(t => t.FinalProjectIdea)
                    .Include(t => t.Students)
                           .ThenInclude(s => s.AppUser)
                    .Where(t => t.Criterias.Any(c => c.Evaluator == "Admin" && c.IsActive == true))
                    .GroupBy(t => t.Specialty)
                    .Select(g => new TeamUnderSpecialtyForEvaluationDto
                    {
                        Specialty = g.Key,
                        Teams = g.Select(t => new TeamWithCriteriaDto
                        {
                            TeamId = t.Id,
                            TeamName = t.Name,
                            ProjectId = t.FinalProjectIdea.ProjectId,
                            ProjectName = t.FinalProjectIdea.ProjectName,
                            ProjectDescription = t.FinalProjectIdea.ProjectDescription,
                            Criterias = t.Criterias
                                            .Where(c => c.Evaluator == "Admin" && c.IsActive == true)
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
                    })
                    .AsNoTracking()
                    .ToListAsync();
            }

            if (evaluatorRole == "Doctor")
            {
                var supervisorSchedules = await _dbContext.Schedules.Include(s => s.CommitteeDoctorSchedules)
                    .ThenInclude(cd => cd.Doctor)
                    .Include(s => s.CommitteeDoctorSchedules)
                    .ThenInclude(cd => cd.Schedule)
                    .Where(s => s.CommitteeDoctorSchedules.Any(cd => cd.DoctorId == evaluatorId && cd.DoctorRole == "Supervisor"))
                    .AsNoTracking()
                    .ToListAsync();

                if (supervisorSchedules == null || supervisorSchedules.Count == 0 || !supervisorSchedules.Any())
                    return NotFound(new ApiResponse(404, "No schedules found for Doctor evaluation as supervisor.", new { IsSuccess = false }));

                var supervisorScheduleIds = supervisorSchedules.Select(s => s.Id).ToList();


                var supervisorCriterias = await _dbContext.Criterias
                    .Include(c => c.Teams)
                    .Where(c => c.IsActive == true &&
                                c.Evaluator == "Supervisor" &&
                                c.Schedules.Any(s => supervisorScheduleIds.Contains(s.Id)))
                    .AsNoTracking()
                    .ToListAsync();

                if (supervisorCriterias == null || supervisorCriterias.Count == 0 || !supervisorCriterias.Any())
                    return NotFound(new ApiResponse(404, "No active criteria found for Doctor evaluation as supervisor.", new { IsSuccess = false }));

                var examinerSchedules = await _dbContext.Schedules.Include(s => s.CommitteeDoctorSchedules)
                    .ThenInclude(cd => cd.Doctor)
                    .Include(s => s.CommitteeDoctorSchedules)
                    .ThenInclude(cd => cd.Schedule)
                    .Where(s => s.CommitteeDoctorSchedules.Any(cd => cd.DoctorId == evaluatorId && cd.DoctorRole == "Examiner"))
                    .AsNoTracking()
                    .ToListAsync();

                if (examinerSchedules == null || examinerSchedules.Count == 0 || !examinerSchedules.Any())
                    return NotFound(new ApiResponse(404, "No schedules found for Doctor evaluation as examiner.", new { IsSuccess = false }));

                var examinerScheduleIds = examinerSchedules.Select(s => s.Id).ToList();

                var examinerCriterias = await _dbContext.Criterias
                    .Include(c => c.Teams)
                    .Where(c => c.IsActive == true &&
                                c.Evaluator == "Examiner" &&
                                c.Schedules.Any(s => examinerScheduleIds.Contains(s.Id)))
                    .AsNoTracking()
                    .ToListAsync();

                if (examinerCriterias == null || examinerCriterias.Count == 0 || !examinerCriterias.Any())
                    return NotFound(new ApiResponse(404, "No active criteria found for Doctor evaluation as examiner.", new { IsSuccess = false }));

                var supervisionTeamsWithCriteriasBySpecialty = await _dbContext.Teams
                    .Include(t => t.Criterias)
                    .Include(t => t.Schedules)
                        .ThenInclude(s => s.CommitteeDoctorSchedules)
                    .Include(t => t.FinalProjectIdea)
                    .Include(t => t.Students)
                           .ThenInclude(s => s.AppUser)
                    .Where(t => t.Criterias.Any(c => c.Evaluator == "Supervisor" && c.IsActive == true &&
                                             c.Schedules.Any(s => supervisorScheduleIds.Contains(s.Id))))
                    .GroupBy(t => t.Specialty)
                    .Select(g => new TeamUnderSpecialtyForEvaluationDto
                    {
                        Specialty = g.Key,
                        Teams = g.Select(t => new TeamWithCriteriaDto
                        {
                            TeamId = t.Id,
                            TeamName = t.Name,
                            ProjectId = t.FinalProjectIdea.ProjectId,
                            ProjectName = t.FinalProjectIdea.ProjectName,
                            ProjectDescription = t.FinalProjectIdea.ProjectDescription,
                            ScheduleId = t.Schedules.FirstOrDefault().Id,
                            ScheduleDate = t.Schedules.FirstOrDefault().ScheduleDate,
                            ScheduleStatus = t.Schedules.FirstOrDefault().Status,
                            Criterias = t.Criterias
                                            .Where(c => c.Evaluator == "Supervisor" && c.IsActive == true)
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
                    })
                    .AsNoTracking()
                    .ToListAsync();

                if (supervisionTeamsWithCriteriasBySpecialty == null || supervisionTeamsWithCriteriasBySpecialty.Count == 0 || !supervisionTeamsWithCriteriasBySpecialty.Any())
                    return NotFound(new ApiResponse(404, "No teams found for Doctor evaluation as supervisor.", new { IsSuccess = false }));

                var examinerTeamsWithCriteriasBySpecialty = await _dbContext.Teams
                    .Include(t => t.Criterias)
                    .Include(t => t.Schedules)
                        .ThenInclude(s => s.CommitteeDoctorSchedules)
                    .Include(t => t.FinalProjectIdea)
                    .Include(t => t.Students)
                           .ThenInclude(s => s.AppUser)
                    .Where(t => t.Criterias.Any(c => c.Evaluator == "Examiner" && c.IsActive == true &&
                                             c.Schedules.Any(s => examinerScheduleIds.Contains(s.Id))))
                    .GroupBy(t => t.Specialty)
                    .Select(g => new TeamUnderSpecialtyForEvaluationDto
                    {
                        Specialty = g.Key,
                        Teams = g.Select(t => new TeamWithCriteriaDto
                        {
                            TeamId = t.Id,
                            TeamName = t.Name,
                            ProjectId = t.FinalProjectIdea.ProjectId,
                            ProjectName = t.FinalProjectIdea.ProjectName,
                            ProjectDescription = t.FinalProjectIdea.ProjectDescription,
                            ScheduleId = t.Schedules.FirstOrDefault().Id,
                            ScheduleDate = t.Schedules.FirstOrDefault().ScheduleDate,
                            ScheduleStatus = t.Schedules.FirstOrDefault().Status,
                            Criterias = t.Criterias
                                            .Where(c => c.Evaluator == "Examiner" && c.IsActive == true)
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
                    })
                    .AsNoTracking()
                    .ToListAsync();

                if (examinerTeamsWithCriteriasBySpecialty == null || examinerTeamsWithCriteriasBySpecialty.Count == 0 || !examinerTeamsWithCriteriasBySpecialty.Any())
                    return NotFound(new ApiResponse(404, "No teams found for Doctor evaluation as examiner.", new { IsSuccess = false }));

                // Combine the two lists of teams by specialty
                var examinationAndSupervisionTeamsBySpecialty = new SupervisionAndExaminationDto
                {
                    SupervisionTeams = supervisionTeamsWithCriteriasBySpecialty,
                    ExaminationTeams = examinerTeamsWithCriteriasBySpecialty
                };

                if (examinationAndSupervisionTeamsBySpecialty == null || examinationAndSupervisionTeamsBySpecialty.SupervisionTeams.Count == 0 || examinationAndSupervisionTeamsBySpecialty.ExaminationTeams.Count == 0)
                    return NotFound(new ApiResponse(404, "No teams found for Doctor evaluation.", new { IsSuccess = false }));

                return Ok(new ApiResponse(200, "Teams retrieved successfully.", examinationAndSupervisionTeamsBySpecialty));

            }

            return NotFound(new ApiResponse(404, "No teams found for evaluation.", new { IsSuccess = false }));
        }

        //[HttpPost("SubmitGradesByAdmin")]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> SubmitGradesByAdmin([FromBody] SubmitAdminEvaluationDto model)
        //{
        //    if (model is null)
        //        return BadRequest(new ApiResponse(400, "Invalid input data.", new { IsSuccess = false }));
            
        //    var evaluatorId = User.FindFirst("Id")?.Value;
        //    var evaluatorRole = User.FindFirst(ClaimTypes.Role)?.Value;
        //    if (evaluatorId == null || evaluatorRole == null)
        //        return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));
            
        //    if (evaluatorRole != "Admin")
        //        return Unauthorized(new ApiResponse(401, "Unauthorized role for evaluation.", new { IsSuccess = false }));

        //    var evaluation = await _dbContext


        //}



    }
}
//public async Task<ApiResponse> SubmitEvaluation(SubmitEvaluationDto model, string appUserId, string appUserRole)
//        {
//            if(appUserRole != "Admin" && appUserRole != "Doctor")
//                return new ApiResponse(401, "Unauthorized role for evaluation.", new { IsSuccess = false });

//            var evaluatorId = 0;
//            if(appUserRole == "Admin")
//            {
//                var admin = await _unitOfWork.Repository<Admin>().FindAsync(a => a.AppUserId == appUserId);
//                if (admin == null)
//                    return new ApiResponse(404, $"Admin not found with ID: '{appUserId}'.", new { IsSuccess = false });
//            }
//            if (appUserRole == "Doctor")
//            {
//                var doctor = await _unitOfWork.Repository<Doctor>().FindAsync(d => d.AppUserId == appUserId);
//                if (doctor == null)
//                    return new ApiResponse(404, $"Doctor not found with ID: '{appUserId}'.", new { IsSuccess = false });

//                var schedule = await _unitOfWork.Repository<Schedule>().FindAsync(s => s.Id == model.ScheduleId);
//                if (schedule == null)
//                    return new ApiResponse(404, $"Schedule not found with ID: '{model.ScheduleId}'.", new { IsSuccess = false });
//                foreach(var s in schedule.CommitteeDoctorSchedules)
//                {
//                    if (s.DoctorId == doctor.Id && s.ScheduleId == model.ScheduleId)
//                        evaluatorId = s.DoctorId;
//                }
//                if (evaluatorId == 0)
//                    return new ApiResponse(404, $"Doctor not found in the schedule with ID: '{model.ScheduleId}'.", new { IsSuccess = false });

//            }

//            foreach (var gradeItem in model.Grades)
//            {
//                var criteria = await _unitOfWork.Repository<Criteria>().FindAsync(c => c.Id == gradeItem.CriteriaId);
//                if (criteria == null)
//                    new ApiResponse(404, $"Criteria not found with ID: '{gradeItem.CriteriaId}'.", new { IsSuccess = false });

//                if (gradeItem.Grade < 0 || gradeItem.Grade > criteria?.MaxGrade)
//                    return new ApiResponse(400, $"Grade '{gradeItem.Grade}' is out of range for criteria ID: '{gradeItem.CriteriaId}'.", new { IsSuccess = false });
//            }

//            var evaluations = model.Grades.Select(grade => new Evaluation
//            {
//                ScheduleId = model.ScheduleId,
//                CriteriaId = grade.CriteriaId,
//                EvaluatorId = evaluatorId,
//                EvaluatorRole = appUserRole,
//                TeamId = model.TeamId,
//                StudentId = model.StudentId,
//                Grade = grade.Grade,
//=               EvaluationDate = DateTime.Now
//            }).ToList();

//            await _unitOfWork.Repository<Evaluation>().AddRangeAsync(evaluations);
//            await _unitOfWork.CompleteAsync();
//        }





//        [HttpGet("Results/{scheduleId}")]
//        public async Task<IActionResult> GetEvaluationResults(int scheduleId, [FromQuery] int? teamId, [FromQuery] int? studentId)
//        {
//            var results = await _evaluationService.GetEvaluationResults(scheduleId, teamId, studentId);


//        public async Task<List<EvaluationResultDto>> GetEvaluationResults(int scheduleId, int? teamId, int? studentId)
//        {
//            var evaluations = await _unitOfWork.Repository<Evaluation>()
//                .FindAllAsync(e => e.ScheduleId == scheduleId &&
//                                 e.TeamId == teamId &&
//                                 e.StudentId == studentId,
//                             include: e => e.Include(x => x.Criteria));

//            return evaluations.Select(e => new EvaluationResultDto
//            {
//                CriteriaId = e.CriteriaId,
//                CriteriaName = e.Criteria.Name,
//                Grade = e.Grade,
//                MaxGrade = e.Criteria.MaxGrade,
//                Comments = e.Comments
//            }).ToList();
//        }
//            return Ok(new ApiResponse(200, "Evaluation results retrieved", results));
//        }

//        [HttpGet("TeamGrades/{teamId}")]
//        [Authorize(Roles = "Admin,Doctor,Student")]
//        public async Task<IActionResult> GetTeamGrades(int teamId)
//        {
//            var evaluations = await _unitOfWork.Repository<Evaluation>()
//                .FindAllAsync(e => e.TeamId == teamId,
//                    include: e => e.Include(x => x.Criteria)
//                                  .Include(x => x.Schedule));

//            var grouped = evaluations.GroupBy(e => e.ScheduleId)
//                .Select(g => new
//                {
//                    ScheduleId = g.Key,
//                    ScheduleDate = g.First().Schedule.ScheduleDate,
//                    Grades = g.Select(e => new EvaluationResultDto
//                    {
//                        CriteriaId = e.CriteriaId,
//                        CriteriaName = e.Criteria.Name,
//                        Grade = e.Grade,
//                        MaxGrade = e.Criteria.MaxGrade,
//                    })
//                });

//            return Ok(new ApiResponse(200, "Team grades retrieved", grouped));
//        }

//        [HttpGet("StudentGrades/{studentId}")]
//        [Authorize(Roles = "Admin,Doctor,Student")]
//        public async Task<IActionResult> GetStudentGrades(int studentId)
//        {
//            var evaluations = await _unitOfWork.Repository<Evaluation>()
//                .FindAllAsync(e => e.StudentId == studentId,
//                    include: e => e.Include(x => x.Criteria)
//                                  .Include(x => x.Schedule)
//                                  .Include(x => x.Team));

//            var grouped = evaluations.GroupBy(e => e.ScheduleId)
//                .Select(g => new
//                {
//                    ScheduleId = g.Key,
//                    ScheduleDate = g.First().Schedule.ScheduleDate,
//                    TeamId = g.First().TeamId,
//                    TeamName = g.First().Team?.Name,
//                    Grades = g.Select(e => new EvaluationResultDto
//                    {
//                        CriteriaId = e.CriteriaId,
//                        CriteriaName = e.Criteria.Name,
//                        Grade = e.Grade,
//                        MaxGrade = e.Criteria.MaxGrade,
//                    })
//                });

//            return Ok(new ApiResponse(200, "Student grades retrieved", grouped));
//        }
//    }
//}
