using GradingManagementSystem.Core;
using GradingManagementSystem.Core.DTOs;
using GradingManagementSystem.Core.Entities;
using GradingManagementSystem.Core.CustomResponses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GradingManagementSystem.Repository.Data.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace GradingManagementSystem.APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CriteriaController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly GradingManagementSystemDbContext _dbContext;

        public CriteriaController(IUnitOfWork unitOfWork, GradingManagementSystemDbContext dbContext)
        {
            _unitOfWork = unitOfWork;
            _dbContext = dbContext;
        }
        
        // Finished / Reviewed / Tested / Edited / D
        [HttpPost("Create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateNewCriteria([FromBody] CreateCriteriaDto model)
        {
            if (model is null)
                return BadRequest(CreateErrorResponse400BadRequest("Invalid input data."));

            if (model.MaxGrade <= 0)
                return BadRequest(CreateErrorResponse400BadRequest("Maximum grade must be greater than zero."));
            if (model.Evaluator != "Examiner" && model.Evaluator != "Supervisor" && model.Evaluator != "Admin")
                return BadRequest(CreateErrorResponse400BadRequest("Invalid evaluator. Must be 'Examiner', 'Supervisor', or 'Admin'."));
            if (model.GivenTo != "Student" && model.GivenTo != "Team")
                return BadRequest(CreateErrorResponse400BadRequest("Invalid given to. Must be 'Student' or 'Team'."));
            if (model.Specialty != "CS" && model.Specialty != "CS & STAT" && model.Specialty != "CS & MATH" && model.Specialty != "CS & PHYS")
                return BadRequest(CreateErrorResponse400BadRequest("Invalid specialty. Must be 'CS', 'CS & STAT', 'CS & MATH', or 'CS & PHYS'."));
            var validTerms = new[] { "First-Term", "Second-Term" };
            if (!validTerms.Contains(model.Term))
                return BadRequest(CreateErrorResponse400BadRequest("Invalid term. Must be 'First-Term' or 'Second-Term'."));

            var existingCriteria = await _unitOfWork.Repository<Criteria>()
                                                    .FindAsync(c => c.Name == model.Name.Trim() &&
                                                               c.MaxGrade == model.MaxGrade &&
                                                               c.Evaluator == model.Evaluator.Trim() &&
                                                               c.Specialty == model.Specialty.Trim() &&
                                                               c.GivenTo == model.GivenTo.Trim() &&
                                                               c.Term == model.Term.Trim() &&
                                                               c.IsActive);
            if (existingCriteria != null)
                return BadRequest(CreateErrorResponse400BadRequest($"Criteria with the same name and evaluator already exists for this specialty: '{model.Specialty}' and given to '{model.GivenTo}'."));

            var activeAcademicYearAppointment = await _unitOfWork.Repository<AcademicAppointment>().FindAsync(a => a.Status == StatusType.Active.ToString());
            if (activeAcademicYearAppointment == null)
                return BadRequest(CreateErrorResponse400BadRequest("You can't create a criteria in this time because no active academic appointment exists."));

            var currentDateTime = DateTime.Now;
            if(model.Term == "First-Term")
            {
                if (!(currentDateTime.Date >= activeAcademicYearAppointment.FirstTermStart.Date && currentDateTime.Date <= activeAcademicYearAppointment.FirstTermEnd.Date))
                {
                    return BadRequest(CreateErrorResponse400BadRequest($"You cannot create criteria outside of First-Term dates " +
                                     $"({activeAcademicYearAppointment.FirstTermStart.Date} to {activeAcademicYearAppointment.FirstTermEnd.Date})"));
                }         
            }
            else if (model.Term == "Second-Term")
            {
                if (!(currentDateTime.Date >= activeAcademicYearAppointment.SecondTermStart.Date && currentDateTime.Date <= activeAcademicYearAppointment.SecondTermEnd.Date))
                {
                    return BadRequest(CreateErrorResponse400BadRequest($"You cannot create criteria outside of Second-Term dates " +
                                     $"({activeAcademicYearAppointment.SecondTermStart.Date} to {activeAcademicYearAppointment.SecondTermEnd.Date})"));
                }
            }

            var newCriteria = new Criteria
            {
                Name = model.Name.Trim(),
                Description = model.Description.Trim(),
                MaxGrade = model.MaxGrade,
                Evaluator = model.Evaluator.Trim(),
                GivenTo = model.GivenTo.Trim(),
                Specialty = model.Specialty.Trim(),
                Year = activeAcademicYearAppointment.Year,
                Term = model.Term,
                AcademicAppointmentId = activeAcademicYearAppointment.Id,
            };

            await _unitOfWork.Repository<Criteria>().AddAsync(newCriteria);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(200, $"Criteria '{newCriteria.Name}' created successfully.", new { IsSuccess = true }));
        }

        // Finished / Reviewed / Tested / Edited / D
        [HttpGet("All")]
        [Authorize(Roles = "Admin, Student, Doctor")]
        public async Task<IActionResult> GetAllCriteriaList()
        {
            var activeAcademicYearAppointment = await _unitOfWork.Repository<AcademicAppointment>().FindAsync(a => a.Status == StatusType.Active.ToString());
            if (activeAcademicYearAppointment == null)
                return BadRequest(CreateErrorResponse400BadRequest("No active academic appointment found."));

            var existingCriteriaList = await _unitOfWork.Repository<Criteria>().FindAllAsync(c => c.IsActive == true && c.AcademicAppointmentId == activeAcademicYearAppointment.Id);
            if (existingCriteriaList == null || !existingCriteriaList.Any())
                return NotFound(CreateErrorResponse404NotFound("No criteria list found."));

            var criteriaList = existingCriteriaList.Select(c => new CriteriaObjectDto
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
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                LastUpdatedAt = c.LastUpdatedAt,
            })
            .OrderByDescending(c => c.CreatedAt)
            .ToList();

            return Ok(new ApiResponse(200, "Criteria list retrieved successfully.", new { IsSuccess = true, criteriaList }));
        }

        // Finished / Reviewed / Tested / Edited / D
        [HttpGet("AllForStudent")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetAllCriteriaListForStudentBasedOnYourSpecialty()
        {
            var studentAppUserId = User.FindFirst("UserId")?.Value;
            if (studentAppUserId == null)
                return Unauthorized(new ApiResponse(401, "Unauthorized Access.", new { IsSuccess = false }));

            var student = await _unitOfWork.Repository<Student>().FindAsync(s => s.AppUserId == studentAppUserId);
            if (student == null)
                return NotFound(CreateErrorResponse404NotFound("Student not found."));

            var activeAcademicYearAppointment = await _unitOfWork.Repository<AcademicAppointment>().FindAsync(a => a.Status == StatusType.Active.ToString());
            if (activeAcademicYearAppointment == null)
                return BadRequest(CreateErrorResponse400BadRequest("No active academic appointment found."));

            var existingCriteriaList = await _unitOfWork.Repository<Criteria>().FindAllAsync(c => c.IsActive == true && 
                                                                                                  c.Specialty == student.Specialty &&
                                                                                                  c.AcademicAppointmentId == activeAcademicYearAppointment.Id);
            if (existingCriteriaList == null || !existingCriteriaList.Any())
                return NotFound(CreateErrorResponse404NotFound("No criteria list Found."));

            var criteriaList = existingCriteriaList.Select(c => new CriteriaObjectDto
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
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                LastUpdatedAt = c.LastUpdatedAt,
            })
            .OrderByDescending(c => c.CreatedAt)
            .ToList();

            return Ok(new ApiResponse(200, "Criteria list retrieved successfully.", new { IsSuccess = true , criteriaList }));
        }

        // Finished / Reviewed / Tested / Edited / D
        [HttpGet("{criteriaId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCriteriaById(int criteriaId)
        {
            if(criteriaId <= 0)
                return BadRequest(CreateErrorResponse400BadRequest("Invalid input data."));

            var existingCriteria = await _unitOfWork.Repository<Criteria>().FindAsync(c => c.Id == criteriaId);
            if (existingCriteria == null)
                return NotFound(CreateErrorResponse404NotFound($"Criteria not found."));

            var criteria = new CriteriaObjectDto
            {
                Id = existingCriteria.Id,
                Name = existingCriteria.Name,
                Description = existingCriteria.Description,
                MaxGrade = existingCriteria.MaxGrade,
                Evaluator = existingCriteria.Evaluator,
                GivenTo = existingCriteria.GivenTo,
                Year = existingCriteria.Year,
                Specialty = existingCriteria.Specialty,
                Term = existingCriteria.Term,
                CreatedAt = existingCriteria.CreatedAt,
                LastUpdatedAt = existingCriteria.LastUpdatedAt,
                IsActive = existingCriteria.IsActive,
            };
            return Ok(new ApiResponse(200, "Criteria retrieved successfully.", new { IsSuccess = true , criteria }));
        }

        // Finished / Reviewed / Tested / Edited / D
        [HttpPut("Update")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateExistingCriteria([FromBody] UpdateCriteriaDto model)
        {
            if (model is null)
                return BadRequest(CreateErrorResponse400BadRequest("Invalid input data."));

            if (model.MaxGrade <= 0)
                return BadRequest(CreateErrorResponse400BadRequest("Maximum grade must be greater than zero."));
            if (model.Evaluator != "Examiner" && model.Evaluator != "Supervisor" && model.Evaluator != "Admin")
                return BadRequest(CreateErrorResponse400BadRequest("Invalid evaluator. Must be 'Examiner', 'Supervisor', or 'Admin'."));
            if (model.GivenTo != "Student" && model.GivenTo != "Team")
                return BadRequest(CreateErrorResponse400BadRequest("Invalid given to. Must be 'Student' or 'Team'."));
            if (model.Specialty != "CS" && model.Specialty != "CS & STAT" && model.Specialty != "CS & MATH" && model.Specialty != "CS & PHYS")
                return BadRequest(CreateErrorResponse400BadRequest("Invalid specialty. Must be 'CS', 'CS & STAT', 'CS & MATH', or 'CS & PHYS'."));
            var validTerms = new[] { "First-Term", "Second-Term" };
            if (!validTerms.Contains(model.Term))
                return BadRequest(CreateErrorResponse400BadRequest("Invalid term. Must be 'First-Term' or 'Second-Term'."));

            var existingCriteria = await _unitOfWork.Repository<Criteria>().FindAsync(c => c.Id == model.Id);
            if (existingCriteria == null)
                return NotFound(CreateErrorResponse404NotFound($"Criteria not found."));

            var activeAppointment = await _unitOfWork.Repository<AcademicAppointment>().FindAsync(a => a.Id == existingCriteria.AcademicAppointmentId);
            if (activeAppointment == null)
                return BadRequest(CreateErrorResponse400BadRequest("Associated academic appointment not found."));

            existingCriteria.Name = model.Name.Trim();
            existingCriteria.Description = model.Description.Trim();
            existingCriteria.MaxGrade = model.MaxGrade;
            existingCriteria.Evaluator = model.Evaluator.Trim();
            existingCriteria.GivenTo = model.GivenTo.Trim();
            existingCriteria.Specialty = model.Specialty.Trim();
            existingCriteria.Term = model.Term.Trim();
            existingCriteria.LastUpdatedAt = DateTime.Now.AddHours(1);
            existingCriteria.Year = activeAppointment.Year;
            existingCriteria.AcademicAppointmentId = existingCriteria.AcademicAppointmentId;

            _unitOfWork.Repository<Criteria>().Update(existingCriteria);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(200, "Criteria updated successfully.", new { IsSuccess = true }));
        }

        // Finished / Reviewed / Tested / Edited / D
        [HttpDelete("Delete/{criteriaId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteExistingCriteria(int criteriaId)
        {
            if (criteriaId <= 0)
                return BadRequest(CreateErrorResponse400BadRequest("Invalid input data."));

            var existingCriteria = await _unitOfWork.Repository<Criteria>().FindAsync(c => c.Id == criteriaId);
            if (existingCriteria == null)
                return NotFound(CreateErrorResponse404NotFound($"Criteria not found."));

            _unitOfWork.Repository<Criteria>().Delete(existingCriteria);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(200, "Criteria deleted successfully.", new { IsSuccess = true }));
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