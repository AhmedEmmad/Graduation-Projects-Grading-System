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
    public class AcademicAppointmentsController : ControllerBase
    {
        private readonly GradingManagementSystemDbContext _dbContext;

        public AcademicAppointmentsController(GradingManagementSystemDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Finished / Tested
        [HttpPost("CreateAppointment")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateNewAcademicAppointment(CreateAcademicAppointmentDto model)
        {
            if (model is null)
                return BadRequest(new ApiResponse(400, "Invalid input data.", new { IsSuccess = false }));

            if (model.FirstTermStart > model.FirstTermEnd)
                return BadRequest(new ApiResponse(400, "First term start date cannot be after the end date.", new { IsSuccess = false }));
            if (model.SecondTermStart > model.SecondTermEnd)
                return BadRequest(new ApiResponse(400, "Second term start date cannot be after the end date.", new { IsSuccess = false }));

            if (model.FirstTermStart > model.SecondTermStart)
                return BadRequest(new ApiResponse(400, "First term start date cannot be after the second term start date.", new { IsSuccess = false }));

            if (model.FirstTermEnd > model.SecondTermEnd)
                return BadRequest(new ApiResponse(400, "First term end date cannot be after the second term end date.", new { IsSuccess = false }));

            if (model.Year.Length != 9 || !model.Year.Contains("-"))
                return BadRequest(new ApiResponse(400, "Invalid academic year format. Use 'YYYY-YYYY'.", new { IsSuccess = false }));

            var existingAppointment = await _dbContext.AcademicAppointments.FirstOrDefaultAsync(ea => ea.Year == model.Year);
            if (existingAppointment != null)
                return BadRequest(new ApiResponse(400, "Academic appointment for this year already exists.", new { IsSuccess = false }));

            var newAcademicAppointment = new AcademicAppointment
            {
                Year = model.Year,
                FirstTermStart = model.FirstTermStart,
                FirstTermEnd = model.FirstTermEnd,
                SecondTermStart = model.SecondTermStart,
                SecondTermEnd = model.SecondTermEnd
            };

            await _dbContext.AcademicAppointments.AddAsync(newAcademicAppointment);
            await _dbContext.SaveChangesAsync();
            return Ok(new ApiResponse(200, "Academic appointment created successfully.", new { IsSuccess = true }));
        }

        // Finished / Tested
        [HttpGet("AllYears")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAcademicAppointments()
        {
            if (_dbContext.AcademicAppointments == null)
                return NotFound(new ApiResponse(404, "No academic appointments found.", new { IsSuccess = false }));
           
            var academicYearAppointments = await _dbContext.AcademicAppointments.Select(a => new AcademicYearAppointmentDto
            {
                Id = a.Id,
                Year = a.Year,
                FirstTermStart = a.FirstTermStart,
                FirstTermEnd = a.FirstTermEnd,
                SecondTermStart = a.SecondTermStart,
                SecondTermEnd = a.SecondTermEnd,
                Status = a.Status
            }).ToListAsync();

            if (academicYearAppointments == null || !academicYearAppointments.Any())
                return NotFound(new ApiResponse(404, "No academic appointments found.", new { IsSuccess = false }));

            return Ok(new ApiResponse(200, "Academic appointment years retrieved successfully.", new { IsSuccess = true, academicYearAppointments }));
        }

        // Finished / Tested
        [HttpPut("SetActiveYear/{appointmentId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetActiveAcademicYear(int appointmentId)
        {
            if (appointmentId <= 0)
                return BadRequest(new ApiResponse(400, "Invalid appointment ID.", new { IsSuccess = false }));
            var academicAppointment = await _dbContext.AcademicAppointments.FirstOrDefaultAsync(a => a.Id == appointmentId);
            if (academicAppointment == null)
                return NotFound(new ApiResponse(404, "Academic appointment not found.", new { IsSuccess = false }));
            
            academicAppointment.Status = "Active";
            academicAppointment.LastUpdatedAt = DateTime.UtcNow;
            _dbContext.AcademicAppointments.Update(academicAppointment);
            var remainingAcademicAppointments = await _dbContext.AcademicAppointments.Where(a => a.Id != appointmentId).ToListAsync();
            foreach (var appointment in remainingAcademicAppointments)
            {
                appointment.Status = "Inactive";
                _dbContext.AcademicAppointments.Update(appointment);
            }
            await _dbContext.SaveChangesAsync();
            return Ok(new ApiResponse(200, "This academic year appointment set to active successfully.", new { IsSuccess = true }));
        }
    }
}
