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

        // Finished / Reviewed / Tested
        [HttpGet("AllDoctorSchedules")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetAllDoctorSchedules()
        {
            var doctorAppUserId = User.FindFirst("Id")?.Value;
            if (doctorAppUserId == null)
                return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));

            var doctor = await _unitOfWork.Repository<Doctor>().FindAsync(d => d.AppUserId == doctorAppUserId);
            if (doctor == null)
                return NotFound(new ApiResponse(404, "Doctor not found.", new { IsSuccess = false }));


            var doctorSchedules = await _dbContext.CommitteeDoctorSchedules
                                                .Include(ds => ds.Schedule)
                                                    .ThenInclude(s => s.Team)
                                                        .ThenInclude(t => t.FinalProjectIdea)
                                                .Include(ds => ds.Schedule.Team)
                                                    .ThenInclude(t => t.Students)
                                                        .ThenInclude(s => s.AppUser)
                                                .Include(ds => ds.Schedule.Team)
                                                    .ThenInclude(t => t.Supervisor)
                                                        .ThenInclude(s => s.AppUser)
                                                .Include(ds => ds.Schedule.Team)
                                                    .ThenInclude(t => t.Leader)
                                                .Where(ds => ds.DoctorId == doctor.Id)
                                                .AsSplitQuery()
                                                .ToListAsync();

            if (doctorSchedules == null || !doctorSchedules.Any())
                return NotFound(new ApiResponse(404, "No schedules found for his doctor.", new { IsSuccess = false }));

            // Determine primary role
            var primaryRole = doctorSchedules.Any(ds => ds.DoctorRole == "Supervisor")
                ? "Supervisor" : "Examiner";

            // Check if the doctor is a supervisor or examiner
            var isSupervisor = doctorSchedules.Any(ds => ds.DoctorRole == "Supervisor");
            var isExaminer = doctorSchedules.Any(ds => ds.DoctorRole == "Examiner");
            if (!isSupervisor && !isExaminer)
                return NotFound(new ApiResponse(404, "No schedules found for his doctor.", new { IsSuccess = false }));

            var schedules = doctorSchedules
        .Where(ds => ds.DoctorRole == primaryRole)
        .GroupBy(ds => ds.ScheduleId)
        .Select(group => new DoctorScheduleDto
        {
            ScheduleId = group.Key,
            ScheduleDate = group.First().Schedule.ScheduleDate,
            Status = group.First().Schedule.Status,
            TeamId = group.First().Schedule.TeamId,
            TeamName = group.First().Schedule.Team?.Name ?? "N/A",
            TeamLeaderId = group.First().Schedule.Team?.LeaderId ?? 0,
            TeamLeaderName = group.First().Schedule.Team?.Leader?.FullName ?? "N/A",
            Specialty = group.First().Schedule.Team?.Specialty ?? "N/A",
            ProjectId = group.First().Schedule.Team?.FinalProjectIdea?.ProjectId ?? 0,
            ProjectName = group.First().Schedule.Team?.FinalProjectIdea?.ProjectName ?? "N/A",
            ProjectDescription = group.First().Schedule.Team?.FinalProjectIdea?.ProjectDescription ?? "N/A",
            DoctorRole = primaryRole,
            PostedBy = group.First().Schedule.Team?.FinalProjectIdea?.PostedBy ?? "N/A",
            SupervisorId = group.First().Schedule.Team?.SupervisorId ?? 0,
            SupervisorName = group.First().Schedule.Team?.Supervisor?.FullName ?? "N/A",
            TeamMembers = group.First().Schedule.Team?.Students?
                .Select(s => new TeamMemberDto
                {
                    Id = s.Id,
                    FullName = s.FullName ?? "N/A",
                    Email = s.Email ?? "N/A",
                    InTeam = s.InTeam,
                    Specialty = s.Specialty ?? "N/A",
                    ProfilePicture = s.AppUser?.ProfilePicture ?? "default.jpg"
                }).ToList() ?? new List<TeamMemberDto>(),
            Examiners = doctorSchedules.Where(d => d.DoctorRole == "Examiner" && d.ScheduleId == group.Key)
                                        .Select(d => new ExaminerDto
                                        {
                                            ExaminerId = d.DoctorId,
                                            ExaminerName = d.Doctor?.FullName ?? "N/A"
                                        })
                                        .Distinct()
                                        .ToList()
        }).ToList();

            return Ok(new ApiResponse(200, "Doctor schedules retrieved successfully.", new { IsSuccess = true, DoctorSchedules = schedules }));
        }

        // Finished /
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
                                                        .ThenInclude(t => t.FinalProjectIdea)
                                                   .Include(s => s.Team)
                                                        .ThenInclude(t => t.Supervisor)
                                                        .ThenInclude(s => s.AppUser)
                                                   .Include(s => s.Team)
                                                        .ThenInclude(t => t.Students)
                                                        .ThenInclude(st => st.AppUser)
                                                    .Include(s => s.CommitteeDoctorSchedules)
                                                        .ThenInclude(cds => cds.Doctor)
                                                   .Where(s => s.Team.Students.Any(st => st.Id == student.Id))
                                                   .AsSplitQuery()
                                                   .ToListAsync();

            if (studentSchedules == null || !studentSchedules.Any())
                return NotFound(new ApiResponse(404, "No schedules found for his student.", new { IsSuccess = false }));

            var schedules = studentSchedules.Select(s => new DoctorScheduleDto
            {
                ScheduleId = s.Id,
                TeamId = s.TeamId,
                TeamName = s.Team?.Name ?? "N/A",
                ProjectName = s.Team?.FinalProjectIdea?.ProjectName ?? "N/A",
                ProjectDescription = s.Team?.FinalProjectIdea?.ProjectDescription ?? "N/A",
                ScheduleDate = s.ScheduleDate,
                Status = s.Status,
                SupervisorName = s.Team?.Supervisor?.FullName ?? "N/A",
                PostedBy = s.Team?.FinalProjectIdea?.PostedBy ?? "N/A",
                TeamMembers = s.Team?.Students?.Select(st => new TeamMemberDto
                                                {
                                                    Id = st.Id,
                                                    FullName = st.FullName ?? "N/A",
                                                    Email = st.Email ?? "N/A",
                                                    InTeam = st.InTeam,
                                                    Specialty = st.Specialty ?? "N/A",
                                                    ProfilePicture = st.AppUser?.ProfilePicture ?? "default.jpg"
                                                }).ToList() ?? new List<TeamMemberDto>(),
                Examiners = s.CommitteeDoctorSchedules?.Where(d => d.DoctorRole == "Examiner")
                                                       .Select(d => new ExaminerDto
                                                       {
                                                           ExaminerId = d.DoctorId,
                                                           ExaminerName = d.Doctor?.FullName ?? "N/A"
                                                       }).ToList() ?? new List<ExaminerDto>()
            }).ToList();

            return Ok(new ApiResponse(200, "Student schedules retrieved successfully.", new { IsSuccess = true, Schedules = schedules }));
        }

        // Finished / Tested
        [HttpPost("CreateSchedule")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSchedule([FromBody] CreateScheduleDto model)
        {
            if (model is null)
                return BadRequest(new ApiResponse(400, "Invalid input data.", new { IsSuccess = false }));

            var team = await _dbContext.Teams.Include(t => t.Supervisor).FirstOrDefaultAsync(t => t.Id == model.TeamId && t.HasProject == true);
            if (team == null)
                return NotFound(new ApiResponse(404, "Team not found.", new { IsSuccess = false }));

            if (model.ScheduleDate <= DateTime.Now)
                return BadRequest(new ApiResponse(400, "Schedule Date must be in the future.", new { IsSuccess = false }));

            var activeAcademicAppointment = await _dbContext.AcademicAppointments
                .Where(a => a.Status == "Active")
                .FirstOrDefaultAsync();

            if (model.ScheduleDate < activeAcademicAppointment?.FirstTermStart &&
                model.ScheduleDate > activeAcademicAppointment?.FirstTermEnd &&
                model.ScheduleDate < activeAcademicAppointment?.SecondTermStart &&
                model.ScheduleDate > activeAcademicAppointment?.SecondTermEnd
               )
                return BadRequest(new ApiResponse(400, "Schedule Date must be within the active academic appointment period.", new { IsSuccess = false }));

            if (model.CommitteeDoctorIds == null || !model.CommitteeDoctorIds.Any())
                return BadRequest(new ApiResponse(400, "Committee Doctors list cannot be empty.", new { IsSuccess = false }));

            var doctors = await _dbContext.Doctors.ToListAsync();
            var committeeDoctors = doctors.Where(d => model.CommitteeDoctorIds.Contains(d.Id)).ToList();

            if (committeeDoctors.Count != model.CommitteeDoctorIds.Count)
                return BadRequest(new ApiResponse(400, "One or more Doctor IDs are invalid.", new { IsSuccess = false }));

            var newSchedule = new Schedule
            {
                TeamId = model.TeamId,
                ScheduleDate = model.ScheduleDate,
                AcademicAppointmentId = activeAcademicAppointment.Id
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
                DoctorId = team.SupervisorId.Value,
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

            return Ok(new ApiResponse(200, $"Schedule created successfully for this team ID: '{team.Id}' with schedule ID: '{newSchedule.Id}'.", new { IsSuccess = true }));
        }
    }
}
