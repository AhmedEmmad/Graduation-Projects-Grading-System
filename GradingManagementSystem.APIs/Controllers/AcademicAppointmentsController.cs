using GradingManagementSystem.Core.CustomResponses;
using GradingManagementSystem.Core.DTOs;
using GradingManagementSystem.Core.Entities;
using GradingManagementSystem.Repository.Data.DbContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

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

        // Finished / Reviewed / Tested
        [HttpPost("CreateAppointment")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateNewAcademicAppointment(CreateAcademicAppointmentDto model)
        {
            if (model == null)
                return BadRequest(CreateErrorResponse("Invalid input data."));

            if(string.IsNullOrWhiteSpace(model.Year) ||
                model.Year.Length != 9 ||
                !Regex.IsMatch(model.Year, @"^\d{4}-\d{4}$"))
                return BadRequest(CreateErrorResponse("Invalid year format, Academic year must be in 'YYYY-YYYY' format."));

            var existingAppointment = await _dbContext.AcademicAppointments.AnyAsync(a => a.Year == model.Year);
            if (existingAppointment)
                return BadRequest(CreateErrorResponse($"Academic appointment for year {model.Year} already exists."));

            if (model.FirstTermStart > model.FirstTermEnd)
                return BadRequest(CreateErrorResponse("First term start date cannot be after its end date."));
            if (model.SecondTermStart > model.SecondTermEnd)
                return BadRequest(CreateErrorResponse("Second term start date cannot be after its end date."));

            if (model.FirstTermStart > model.SecondTermStart)
                return BadRequest(CreateErrorResponse("First term cannot start after second term."));
            if (model.FirstTermEnd > model.SecondTermEnd)
                return BadRequest(CreateErrorResponse("First term cannot end after second term."));

            var newAcademicAppointment = new AcademicAppointment
            {
                Year = model.Year.Trim(),
                FirstTermStart = model.FirstTermStart,
                FirstTermEnd = model.FirstTermEnd,
                SecondTermStart = model.SecondTermStart,
                SecondTermEnd = model.SecondTermEnd
            };

            await _dbContext.AcademicAppointments.AddAsync(newAcademicAppointment);
            await _dbContext.SaveChangesAsync();
            return Ok(new ApiResponse(200, $"Academic appointment for year {model.Year} created successfully.", new { IsSuccess = true }));
        }

        // Finished / Reviewed / Tested
        [HttpGet("AllYears")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAcademicAppointments()
        {
            var academicYearAppointments = await _dbContext.AcademicAppointments
                    .OrderBy(a => a.Year)
                    .Select(a => new AcademicYearAppointmentDto
                    {
                        Id = a.Id,
                        Year = a.Year,
                        FirstTermStart = a.FirstTermStart,
                        FirstTermEnd = a.FirstTermEnd,
                        SecondTermStart = a.SecondTermStart,
                        SecondTermEnd = a.SecondTermEnd,
                        Status = a.Status
                    })
                    .ToListAsync();

            if (academicYearAppointments.Count == 0 || academicYearAppointments == null) // Will comparing with academicYearAppointments.Any();
                return NotFound(new ApiResponse(404, "No academic appointments found.", new { IsSuccess = false }));

            return Ok(new ApiResponse(200, "Academic appointments retrieved successfully.", new { IsSuccess = true, academicYearAppointments }));
        }

        // Finished / Reviewed / Tested
        [HttpPut("SetActiveYear/{appointmentId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetActiveAcademicYearAppointment(int appointmentId)
        {
            if (appointmentId <= 0)
                return BadRequest(CreateErrorResponse("Invalid appointment ID."));

            var academicAppointment = await _dbContext.AcademicAppointments.FirstOrDefaultAsync(a => a.Id == appointmentId);
            if (academicAppointment == null)
                return NotFound(new ApiResponse(404, "Academic appointment not found.", new { IsSuccess = false }));

            if (academicAppointment.Status == "Active")
                return Ok(new ApiResponse(200, "Academic year is already active.", new { IsSuccess = true }));

            academicAppointment.Status = "Active";
            academicAppointment.LastUpdatedAt = DateTime.Now;
            _dbContext.AcademicAppointments.Update(academicAppointment);
            var remainingAcademicAppointments = await _dbContext.AcademicAppointments.Where(a => a.Id != appointmentId).ToListAsync();
            foreach (var appointment in remainingAcademicAppointments)
            {
                appointment.Status = "Inactive";
                _dbContext.AcademicAppointments.Update(appointment);
            }
            await _dbContext.SaveChangesAsync();
            return Ok(new ApiResponse(200, $"This academic year appointment {academicAppointment.Year} set to active successfully.", new { IsSuccess = true }));
        }

        private static ApiResponse CreateErrorResponse(string message, object? errors = null)
        {
            return new ApiResponse(400, message, new { IsSuccess = false });
        }

    }
}
