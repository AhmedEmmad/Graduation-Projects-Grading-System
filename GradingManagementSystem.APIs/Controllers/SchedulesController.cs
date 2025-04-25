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

        [HttpGet("AllDoctorSchedules")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetAllDoctorSchedules()
        {
            var doctorId = User.FindFirst("Id")?.Value;
            if (doctorId == null)
                return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));

            var doctor = await _unitOfWork.Repository<Doctor>().FindAsync(d => d.AppUserId == doctorId);
            if (doctor == null)
                return NotFound(new ApiResponse(404, "Doctor not found.", new { IsSuccess = false }));


            var doctorSchedules = await _dbContext.DoctorSchedules
                                                  .Include(ds => ds.Schedule)
                                                  .Include(ds => ds.Schedule.DoctorSchedules)
                                                  .Include(ds => ds.Schedule.Team)
                                                  .Include(ds => ds.Schedule.Team.FinalProjectIdea)
                                                  .Include(ds => ds.Schedule.Team.Supervisor)
                                                  .Include(ds => ds.Schedule.Team.Supervisor.AppUser)
                                                  .Include(ds => ds.Schedule.Team.Students)
                                                  .Include(ds => ds.Schedule.Team.Students.Select(s => s.AppUser))
                                                  .Where(ds => ds.DoctorId == doctor.Id)
                                                  .ToListAsync();

            if (doctorSchedules == null || !doctorSchedules.Any())
                return NotFound(new ApiResponse(404, "No schedules found for this doctor.", new { IsSuccess = false }));

            // Check if the doctor is a supervisor or examiner
            var isSupervisor = doctorSchedules.Any(ds => ds.DoctorRole == "Supervisor");
            var isExaminer = doctorSchedules.Any(ds => ds.DoctorRole == "Examiner");
            if (!isSupervisor && !isExaminer)
                return NotFound(new ApiResponse(404, "No schedules found for this doctor.", new { IsSuccess = false }));

            // Filter schedules based on the doctor's role
            if (isSupervisor)
                doctorSchedules = doctorSchedules.Where(ds => ds.DoctorRole == "Supervisor").ToList();
            else if (isExaminer)
                doctorSchedules = doctorSchedules.Where(ds => ds.DoctorRole == "Examiner").ToList();

            var schedules = doctorSchedules.Select(ds => new DoctorScheduleDto
            {
                ScheduleId = ds.ScheduleId,
                TeamId = ds.Schedule.TeamId,
                TeamName = ds.Schedule.Team.Name,
                ProjectName = ds.Schedule.Team.FinalProjectIdea.ProjectName,
                ProjectDescription = ds.Schedule.Team.FinalProjectIdea.ProjectDescription,
                ScheduleDate = ds.Schedule.ScheduleDate,
                DoctorRole = ds.DoctorRole,
                PostedBy = ds.Schedule.Team.FinalProjectIdea.PostedBy,
                SupervisorName = ds.Schedule.Team.Supervisor.FullName,
                TeamMembers = ds.Schedule.Team.Students.Select(s => new TeamMemberDto
                {
                    Id = s.Id,
                    FullName = s.FullName,
                    Email = s.Email,
                    InTeam = s.InTeam,
                    Specialty = s.Specialty,
                    ProfilePicture = s.AppUser.ProfilePicture
                }).ToList(),
                Examiners = ds.Schedule.DoctorSchedules.Where(d => d.DoctorRole == "Examiner").Select(d => new ExaminerDto
                {
                    ExaminerId = d.DoctorId,
                    ExaminerName = d.Doctor.FullName
                }).ToList()

            }).ToList();

            return Ok(new ApiResponse(200, "Schedules retrieved successfully.", new { IsSuccess = true, Schedules = schedules  }));
        }

        [HttpGet("AllStudentSchedules")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetAllStudentSchedules()
        {
            var studentId = User.FindFirst("Id")?.Value;
            if (studentId == null)
                return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));
            var student = await _unitOfWork.Repository<Student>().FindAsync(s => s.AppUserId == studentId);
            if (student == null)
                return NotFound(new ApiResponse(404, "Student not found.", new { IsSuccess = false }));
            var studentSchedules = await _dbContext.Schedules
                                                   .Include(s => s.Team)
                                                   .Include(s => s.Team.FinalProjectIdea)
                                                   .Include(s => s.Team.Supervisor)
                                                   .Include(s => s.Team.Supervisor.AppUser)
                                                   .Include(s => s.Team.Students)
                                                   .Include(s => s.Team.Students.Select(st => st.AppUser))
                                                   .Where(s => s.Team.Students.Any(st => st.Id == student.Id))
                                                   .ToListAsync();
            if (studentSchedules == null || !studentSchedules.Any())
                return NotFound(new ApiResponse(404, "No schedules found for this student.", new { IsSuccess = false }));
            var schedules = studentSchedules.Select(s => new StudentScheduleDto
            {
                ScheduleId = s.Id,
                TeamId = s.TeamId,
                TeamName = s.Team.Name,
                ProjectName = s.Team.FinalProjectIdea.ProjectName,
                ProjectDescription = s.Team.FinalProjectIdea.ProjectDescription,
                ScheduleDate = s.ScheduleDate,
                SupervisorName = s.Team.Supervisor.FullName,
                PostedBy = s.Team.FinalProjectIdea.PostedBy,
                TeamMembers = s.Team.Students.Select(st => new TeamMemberDto
                {
                    Id = st.Id,
                    FullName = st.FullName,
                    Email = st.Email,
                    InTeam = st.InTeam,
                    Specialty = st.Specialty,
                    ProfilePicture = st.AppUser.ProfilePicture
                }).ToList(),
                Examiners = s.DoctorSchedules.Where(d => d.DoctorRole == "Examiner").Select(d => new ExaminerDto
                {
                    ExaminerId = d.DoctorId,
                    ExaminerName = d.Doctor.FullName
                }).ToList()
            }).ToList();
            return Ok(new ApiResponse(200, "Schedules retrieved successfully.", new { IsSuccess = true, Schedules = schedules }));
        }

        [HttpPost("CreateSchedule")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSchedule([FromBody] CreateScheduleDto model)
        {
            if (model == null)
                return BadRequest(new ApiResponse(400, "Invalid input data.", new { IsSuccess = false }));

            var team = await _dbContext.Teams.Include(t => t.Supervisor).FirstOrDefaultAsync(t => t.Id == model.TeamId);
            if (team == null)
                return NotFound(new ApiResponse(404, "Team not found.", new { IsSuccess = false }));

            if (model.ScheduleDate <= DateTime.Now)
                return BadRequest(new ApiResponse(400, "Schedule Date must be in the future.", new { IsSuccess = false }));

            if (model.CommitteeDoctorIds == null || !model.CommitteeDoctorIds.Any())
                return BadRequest(new ApiResponse(400, "Committee Doctors list cannot be empty.", new { IsSuccess = false }));

            var doctors = await _unitOfWork.Repository<Doctor>().GetAllAsync();
            var committeeDoctors = doctors.Where(d => model.CommitteeDoctorIds.Contains(d.Id)).ToList();

            if (committeeDoctors.Count != model.CommitteeDoctorIds.Count)
                return BadRequest(new ApiResponse(400, "One or more Doctor IDs are invalid.", new { IsSuccess = false }));

            if (team.SupervisorId.HasValue && model.CommitteeDoctorIds.Contains(team.SupervisorId.Value))
                return BadRequest(new ApiResponse(400, "Supervisor cannot be part of the committee.", new { IsSuccess = false }));

            var newSchedule = new Schedule
            {
                TeamId = model.TeamId,
                ScheduleDate = model.ScheduleDate
            };

            await _unitOfWork.Repository<Schedule>().AddAsync(newSchedule);
            await _unitOfWork.CompleteAsync();

            var examinerSchedules = model.CommitteeDoctorIds.Select(doctorId => new DoctorSchedule
            {
                ScheduleId = newSchedule.Id,
                DoctorId = doctorId,
                DoctorRole = "Examiner",
            }).ToList();

            foreach (var examinerSchedule in examinerSchedules)
                await _unitOfWork.Repository<DoctorSchedule>().AddAsync(examinerSchedule);

            var supervisorSchedule = new DoctorSchedule
            {
                ScheduleId = newSchedule.Id,
                DoctorId = team.SupervisorId.Value,
                DoctorRole = "Supervisor",
            };

            await _unitOfWork.Repository<DoctorSchedule>().AddAsync(supervisorSchedule);

            await _unitOfWork.CompleteAsync();
            return Ok(new ApiResponse(200, $"Schedule created successfully for this team ID: '{team.Id}' with schedule ID: '{newSchedule.Id}'.", new { IsSuccess = true }));
        }
    }
}
