using GradingManagementSystem.Core;
using GradingManagementSystem.Core.CustomResponses;
using GradingManagementSystem.Core.DTOs;
using GradingManagementSystem.Core.Entities;
using GradingManagementSystem.Repository.Data.DbContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GradingManagementSystem.APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchedulesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly GradingManagementSystemDbContext _dbContext;

        public SchedulesController(IUnitOfWork unitOfWork, GradingManagementSystemDbContext dbContext)
        {
            _unitOfWork = unitOfWork;
            _dbContext = dbContext;
        }

        // Finished / Reviewed / Tested / Edited
        [HttpGet("AllDoctorSchedules")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetAllDoctorSchedules()
        {
            var doctorAppUserId = User.FindFirst("UserId")?.Value;
            if (doctorAppUserId == null)
                return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));

            var doctor = await _unitOfWork.Repository<Doctor>().FindAsync(d => d.AppUserId == doctorAppUserId);
            if (doctor == null)
                return NotFound(CreateErrorResponse404NotFound("Doctor not found."));

            var activeAppointment = await _unitOfWork.Repository<AcademicAppointment>().FindAsync(a => a.Status == StatusType.Active.ToString());
            if (activeAppointment == null)
                return NotFound(CreateErrorResponse404NotFound("No active academic year appointment found."));

            var doctorSchedules = await _dbContext.CommitteeDoctorSchedules.Where(cds => cds.Schedule.AcademicAppointmentId == activeAppointment.Id)
                .Include(ds => ds.Schedule)
                    .ThenInclude(s => s.Team)
                        .ThenInclude(t => t.FinalProjectIdea)
                .Include(ds => ds.Doctor)
                .Include(ds => ds.Schedule.Team)
                    .ThenInclude(t => t.Students)
                        .ThenInclude(s => s.AppUser)
                .Include(ds => ds.Schedule.Team)
                    .ThenInclude(t => t.Supervisor)
                        .ThenInclude(s => s.AppUser)
                .Include(ds => ds.Schedule.Team)
                    .ThenInclude(t => t.Leader)
                .AsSplitQuery()
                .ToListAsync();

            if (doctorSchedules == null || !doctorSchedules.Any())
                return NotFound(CreateErrorResponse404NotFound("No schedules found for this doctor."));

            var doctorScheduleDtos = doctorSchedules
                .GroupBy(ds => ds.ScheduleId)
                .Select(group => new DoctorScheduleDto
                {
                    ScheduleId = group.Key,
                    ScheduleDate = group.First().Schedule.ScheduleDate,
                    Status = group.First().Schedule.Status,
                    TeamId = group.First().Schedule.TeamId,
                    TeamName = group.First().Schedule.Team?.Name,
                    TeamLeaderId = group.First().Schedule.Team?.LeaderId,
                    TeamLeaderName = group.First().Schedule.Team?.Leader?.FullName,
                    Specialty = group.First().Schedule.Team?.Specialty,
                    ProjectId = group.First().Schedule.Team?.FinalProjectIdea?.ProjectId,
                    ProjectName = group.First().Schedule.Team?.FinalProjectIdea?.ProjectName,
                    ProjectDescription = group.First().Schedule.Team?.FinalProjectIdea?.ProjectDescription,
                    DoctorRole = group.Any(ds => ds.DoctorId == doctor.Id && ds.DoctorRole == "Supervisor") ? "Supervisor" : "Examiner",
                    PostedBy = group.First().Schedule.Team?.FinalProjectIdea?.PostedBy,
                    SupervisorId = group.First().Schedule.Team?.SupervisorId,
                    SupervisorName = group.First().Schedule.Team?.Supervisor?.FullName,
                    TeamMembers = group.First().Schedule.Team?.Students?
                        .Select(s => new TeamMemberDto
                        {
                            Id = s.Id,
                            FullName = s.FullName,
                            Email = s.Email,
                            InTeam = s.InTeam,
                            Specialty = s.Specialty,
                            ProfilePicture = s.AppUser?.ProfilePicture
                        }).ToList() ?? new List<TeamMemberDto>(),
                    Examiners = doctorSchedules
                        .Where(d => d.ScheduleId == group.Key && d.DoctorRole == "Examiner")
                        .Select(d => new ExaminerDto
                        {
                            ExaminerId = d.DoctorId,
                            ExaminerName = d.Doctor?.FullName
                        }).ToList() ?? new List<ExaminerDto>()
                })
                .ToList();

            return Ok(new ApiResponse(200, "Doctor schedules retrieved successfully.", new
            {
                IsSuccess = true,
                Schedules = doctorScheduleDtos
            }));
        }

        // Finished / Reviewed / Tested / Edited
        [HttpGet("AllStudentSchedules")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetAllStudentSchedules()
        {
            var studentAppUserId = User.FindFirst("UserId")?.Value;
            if (studentAppUserId == null)
                return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));

            var student = await _unitOfWork.Repository<Student>().FindAsync(s => s.AppUserId == studentAppUserId);
            if (student == null)
                return NotFound(CreateErrorResponse404NotFound("Student not found."));

            var activeAppointment = await _unitOfWork.Repository<AcademicAppointment>().FindAsync(a => a.Status == StatusType.Active.ToString());
            if (activeAppointment == null)
                return NotFound(CreateErrorResponse404NotFound("No active academic year appointment found."));

            var studentSchedules = await _dbContext.Schedules
                                                   .Include(s => s.Team)
                                                        .ThenInclude(t => t.FinalProjectIdea)
                                                   .Include(s => s.Team)
                                                        .ThenInclude(t => t.Supervisor)
                                                                .ThenInclude(s => s.AppUser)
                                                   .Include(s => s.Team)
                                                        .ThenInclude(t => t.Students)
                                                                .ThenInclude(st => st.AppUser)
                                                    .Include(s => s.CommitteeDoctorSchedules)
                                                        .ThenInclude(cds => cds.Doctor)
                                                   .Where(s => s.Team.Students.Any(st => st.Id == student.Id) &&
                                                               s.AcademicAppointmentId == activeAppointment.Id)
                                                   .AsSplitQuery()
                                                   .ToListAsync();

            if (studentSchedules == null || !studentSchedules.Any())
                return NotFound(CreateErrorResponse404NotFound("No schedules found for his student."));

            var schedules = studentSchedules.Select(s => new StudentScheduleDto
            {
                ScheduleId = s.Id,
                TeamId = s.TeamId,
                TeamName = s.Team?.Name,
                ProjectName = s.Team?.FinalProjectIdea?.ProjectName,
                ProjectDescription = s.Team?.FinalProjectIdea?.ProjectDescription,
                ScheduleDate = s.ScheduleDate,
                Status = s.Status,
                SupervisorId = s.Team?.SupervisorId,
                SupervisorProfilePicture = s.Team?.Supervisor?.AppUser?.ProfilePicture,
                SupervisorName = s.Team?.Supervisor?.FullName,
                PostedBy = s.Team?.FinalProjectIdea?.PostedBy,
                TeamMembers = s.Team?.Students?.Select(st => new TeamMemberDto
                                                {
                                                    Id = st.Id,
                                                    FullName = st.FullName,
                                                    Email = st.Email,
                                                    InTeam = st.InTeam,
                                                    Specialty = st.Specialty,
                                                    ProfilePicture = st.AppUser?.ProfilePicture
                                                }).ToList() ?? new List<TeamMemberDto>()
            })
            .ToList();

            return Ok(new ApiResponse(200, "Student schedules retrieved successfully.", new { IsSuccess = true, Schedules = schedules }));
        }

        // Finished / Reviewed / Tested / Edited
        [HttpPost("CreateSchedule")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSchedule([FromBody] CreateScheduleDto model)
        {
            if (model is null)
                return BadRequest(CreateErrorResponse400BadRequest("Invalid input data."));

            var team = await _dbContext.Teams.Include(t => t.Supervisor).FirstOrDefaultAsync(t => t.Id == model.TeamId && t.HasProject == true);
            if (team == null)
                return NotFound(CreateErrorResponse404NotFound("Team not found."));

            if (model.ScheduleDate <= DateTime.Now)
                return BadRequest(CreateErrorResponse400BadRequest("Schedule Date must be in the future."));

            var activeAcademicAppointment = await _dbContext.AcademicAppointments
                                                            .Where(a => a.Status == StatusType.Active.ToString())
                                                            .FirstOrDefaultAsync();

            if (model.ScheduleDate < activeAcademicAppointment?.FirstTermStart &&
                model.ScheduleDate > activeAcademicAppointment?.FirstTermEnd &&
                model.ScheduleDate < activeAcademicAppointment?.SecondTermStart &&
                model.ScheduleDate > activeAcademicAppointment?.SecondTermEnd
               )
                return BadRequest(CreateErrorResponse400BadRequest("Schedule Date must be within the active academic appointment period."));

            if (model.CommitteeDoctorIds == null || !model.CommitteeDoctorIds.Any())
                return BadRequest(CreateErrorResponse400BadRequest("Committee Doctors list cannot be empty."));

            var doctors = await _dbContext.Doctors.ToListAsync();
            var committeeDoctors = doctors.Where(d => model.CommitteeDoctorIds.Contains(d.Id)).ToList();

            if (committeeDoctors.Count != model.CommitteeDoctorIds.Count)
                return BadRequest(CreateErrorResponse400BadRequest("One or more Doctor IDs are invalid."));

            var exsitingSchedule = await _dbContext.Schedules.Where(s => s.TeamId == model.TeamId).FirstOrDefaultAsync();
            if (exsitingSchedule != null)
                return BadRequest(CreateErrorResponse400BadRequest("Schedule exists for this team."));

            var newSchedule = new Schedule
            {
                TeamId = model.TeamId,
                ScheduleDate = model.ScheduleDate,
                AcademicAppointmentId = activeAcademicAppointment?.Id
            };

            await _unitOfWork.Repository<Schedule>().AddAsync(newSchedule);
            await _unitOfWork.CompleteAsync();

            var committeeDoctorSchedules = model.CommitteeDoctorIds.Select(doctorId => new CommitteeDoctorSchedule
            {
                ScheduleId = newSchedule.Id,
                DoctorId = doctorId,
                DoctorRole = "Examiner",
            }).ToList();

            foreach (var committeeDoctorSchedule in committeeDoctorSchedules)
                await _unitOfWork.Repository<CommitteeDoctorSchedule>().AddAsync(committeeDoctorSchedule);

            var committeeSupervisorSchedule = new CommitteeDoctorSchedule
            {
                ScheduleId = newSchedule.Id,
                DoctorId = team?.SupervisorId,
                DoctorRole = "Supervisor",
            };

            await _unitOfWork.Repository<CommitteeDoctorSchedule>().AddAsync(committeeSupervisorSchedule);

            // Add CriteriaSchedule entries
            var criterias = await _dbContext.Criterias.Where(c => c.Specialty == team.Specialty).ToListAsync(); // Fetch all criteria
            var criteriaSchedules = criterias.Select(c => new CriteriaSchedule
            {
                CriteriaId = c.Id,
                ScheduleId = newSchedule.Id,
                MaxGrade = c.MaxGrade,
            }).ToList();

            foreach (var criteriaSchedule in criteriaSchedules)
                await _unitOfWork.Repository<CriteriaSchedule>().AddAsync(criteriaSchedule);

            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(200, $"Schedule created successfully for this team .", new { IsSuccess = true }));
        }


        private static ApiResponse CreateErrorResponse400BadRequest(string message)
        {
            return new ApiResponse(400, message, new { IsSuccess = false });
        }

        private static ApiResponse CreateErrorResponse404NotFound(string message)
        {
            return new ApiResponse(404, message, new { IsSuccess = false });
        }
    }
}
