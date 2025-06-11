using GradingManagementSystem.Core;
using GradingManagementSystem.Core.CustomResponses;
using GradingManagementSystem.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GradingManagementSystem.APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public DoctorsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Finished / Reviewed / Tested / Edited / D
        [HttpGet("All")]
        [Authorize(Roles = "Admin, Doctor")]
        public async Task<IActionResult> GetAllDoctors()
        {
            var doctors = await _unitOfWork.Repository<Doctor>().GetAllAsync();
            if (doctors == null || !doctors.Any())
                return NotFound(new ApiResponse(404, "No doctors found.", new { IsSuccess = false }));

            var doctorsList = doctors.Select(x => new
            {
                DoctorId = x.Id,
                DoctorName = x.FullName
            })
            .OrderBy(x => x.DoctorName)
            .ToList();
            return Ok(new ApiResponse(200, "Doctors retrieved successfully.", new { IsSuccess = true, doctorsList }));
        }
    }
}
