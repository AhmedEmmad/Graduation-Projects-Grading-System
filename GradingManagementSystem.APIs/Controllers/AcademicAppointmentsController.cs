using GradingManagementSystem.Core.DTOs;
using GradingManagementSystem.Core.Services.Contact;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GradingManagementSystem.APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AcademicAppointmentsController : ControllerBase
    {
        private readonly IAcademicAppointmentsService _academicAppointmentsService;

        public AcademicAppointmentsController(IAcademicAppointmentsService academicAppointmentsService)
        {
            _academicAppointmentsService = academicAppointmentsService;
        }

        // Finished / Reviewed / Tested
        [HttpPost("CreateAppointment")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateNewAcademicAppointment(CreateAcademicAppointmentDto model)
        {
            var result = await _academicAppointmentsService.CreateNewAcademicAppointmentAsync(model);
            if (result.StatusCode == 400)
                return BadRequest(result);
            if (result.StatusCode == 404)
                return NotFound(result);
            return Ok(result);
        }

        // Finished / Reviewed / Tested
        [HttpGet("AllYears")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAcademicAppointments()
        {
            var result = await _academicAppointmentsService.GetAllAcademicYearAppointmentsAsync();
            if (result.StatusCode == 404)
                return NotFound(result);
            if (result.StatusCode == 400)
                return BadRequest(result);
            return Ok(result);
        }

        // Finished / Reviewed / Tested
        [HttpPut("SetActiveYear")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetActiveAcademicYearAppointment([FromBody] SetActiveYearDto model)
        {
            var result = await _academicAppointmentsService.SetNewAcademicYearAppointmentAsync(model);
            if (result.StatusCode == 400)
                return BadRequest(result);
            if (result.StatusCode == 404)
                return NotFound(result);
            return Ok(result);
        }
    }
}
