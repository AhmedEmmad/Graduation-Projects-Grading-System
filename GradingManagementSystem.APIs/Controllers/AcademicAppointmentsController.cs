using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GradingManagementSystem.APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AcademicAppointmentsController : ControllerBase
    {
        [HttpPost("CreateAppointment")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateNewAcademicAppointment(CreateAppointmentDto model)
        {

        }

    }
}
