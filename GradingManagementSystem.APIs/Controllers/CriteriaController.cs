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
        
        // Finished / Tested
        [HttpPost("CreateCriteria")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCriteria([FromBody] CreateCriteriaDto model)
        {
            if (model is null)
                return BadRequest(new ApiResponse(400, "Invalid input data.", new { IsSuccess = false }));

            var existingCriteria = await _unitOfWork.Repository<Criteria>()
                                                    .FindAsync(c => c.Name == model.Name && c.Evaluator == model.Evaluator && c.Specialty == model.Specialty && c.IsActive);
            if (existingCriteria != null)
                return BadRequest(new ApiResponse(400, $"Criteria with the same name and evaluator already exists for this specialty: '{model.Specialty}'.", new { IsSuccess = false }));

            var activeAppointment = await _unitOfWork.Repository<AcademicAppointment>().FindAsync(a => a.Status == "Active");

            if (activeAppointment == null)
                return BadRequest(new ApiResponse(400, "You can't create a criteria in this time because no active academic appointment exists.", new { IsSuccess = false }));

            if(model.Term != "First-Term" && model.Term != "Second-Term")
                return BadRequest(new ApiResponse(400, "Invalid term. Must be 'First-Term' or 'Second-Term'.", new { IsSuccess = false }));

            var currentDate = DateTime.Now;
            
            if(model.Term == "First-Term")
            {
                if (currentDate < activeAppointment.FirstTermStart && currentDate > activeAppointment.FirstTermEnd)
                {
                    return BadRequest(new ApiResponse(400, $"You cannot create criteria outside of First-Term dates " +
                                     $"({activeAppointment.FirstTermStart} to {activeAppointment.FirstTermEnd})", new { IsSuccess = false }));
                }         
            }
            else if (model.Term == "Second-Term")
            {
                if (currentDate < activeAppointment.SecondTermStart && currentDate > activeAppointment.SecondTermEnd)
                {
                    return BadRequest(new ApiResponse(400, $"You cannot create criteria outside of Second-Term dates " +
                                     $"({activeAppointment.SecondTermStart} to {activeAppointment.SecondTermEnd})", new { IsSuccess = false }));
                }
            }

            var newCriteria = new Criteria
            {
                Name = model.Name,
                Description = model.Description,
                MaxGrade = model.MaxGrade,
                Evaluator = model.Evaluator,
                GivenTo = model.GivenTo,
                Specialty = model.Specialty,
                Year = activeAppointment.Year,
                Term = model.Term,
                AcademicAppointmentId = activeAppointment.Id,
            };

            await _unitOfWork.Repository<Criteria>().AddAsync(newCriteria);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(200, $"Criteria created successfully with ID: '{newCriteria.Id}' for the specialty: '{newCriteria.Specialty}'.", new { IsSuccess = true }));
        }

        // Finished / Tested
        [HttpGet("All")]
        [Authorize(Roles = "Admin, Doctor")]
        public async Task<IActionResult> GetAllCriteriaList()
        {
            var existingCriteriaList = await _unitOfWork.Repository<Criteria>().GetAllAsync();
            if (existingCriteriaList == null || !existingCriteriaList.Any())
                return NotFound(new ApiResponse(404, "No criteria list Found.", new { IsSuccess = false } ));

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
            }).ToList();
            return Ok(new ApiResponse(200, "Criteria list retrieved successfully.", new { IsSuccess = true , criteriaList }));
        }
        
        // Finished / Tested
        [HttpGet("{criteriaId}")]
        [Authorize(Roles = "Admin, Doctor")]
        public async Task<IActionResult> GetCriteriaById(int criteriaId)
        {
            if(criteriaId <= 0)
                return BadRequest(new ApiResponse(400, "Invalid input data.", new { IsSuccess = false }));

            var existingCriteria = await _unitOfWork.Repository<Criteria>().FindAsync(c => c.Id == criteriaId);
            if (existingCriteria == null)
                return NotFound(new ApiResponse(404, "Criteria not found.", new { IsSuccess = false }));

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

        // Finished / Tested
        [HttpPut("UpdateCriteria")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCriteria([FromBody] UpdateCriteriaDto model)
        {
            if (model is null)
                return BadRequest(new ApiResponse(400, "Invalid input data.", new { IsSuccess = false }));

            var existingCriteria = await _dbContext.Criterias.Include(c => c.AcademicAppointment).FirstOrDefaultAsync(c => c.Id == model.Id);
            if (existingCriteria == null)
                return NotFound(new ApiResponse(404, "Criteria not found.", new { IsSuccess = false }));

            if (model.Term != "First-Term" && model.Term != "Second-Term")
                return BadRequest(new ApiResponse(400, "Invalid term. Must be 'First-Term' or 'Second-Term'.", new { IsSuccess = false }));
            
            var currentDate = DateTime.Now;
            if (model.Term == "First-Term")
            {
                if (currentDate < existingCriteria.AcademicAppointment.FirstTermStart && currentDate > existingCriteria.AcademicAppointment.FirstTermEnd)
                {
                    return BadRequest(new ApiResponse(400, $"You cannot update criteria outside of First-Term dates " +
                                     $"({existingCriteria.AcademicAppointment.FirstTermStart} to {existingCriteria.AcademicAppointment.FirstTermEnd})", new { IsSuccess = false }));
                }
            }
            else if (model.Term == "Second-Term")
            {
                if (currentDate < existingCriteria.AcademicAppointment.SecondTermStart && currentDate > existingCriteria.AcademicAppointment.SecondTermEnd)
                {
                    return BadRequest(new ApiResponse(400, $"You cannot update criteria outside of Second-Term dates " +
                                     $"({existingCriteria.AcademicAppointment.SecondTermStart} to {existingCriteria.AcademicAppointment.SecondTermEnd})", new { IsSuccess = false }));
                }
            }
            if (existingCriteria.Name != model.Name)
            {
                var criteriaWithSameName = await _unitOfWork.Repository<Criteria>()
                    .FindAsync(c => c.Name == model.Name && c.Evaluator == model.Evaluator && c.Specialty == model.Specialty && c.IsActive);
                if (criteriaWithSameName != null)
                    return BadRequest(new ApiResponse(400, $"Criteria with the same name and evaluator already exists for this specialty: '{model.Specialty}'.", new { IsSuccess = false }));
            }

            existingCriteria.Name = model.Name;
            existingCriteria.Description = model.Description;
            existingCriteria.MaxGrade = model.MaxGrade;
            existingCriteria.Evaluator = model.Evaluator;
            existingCriteria.GivenTo = model.GivenTo;
            existingCriteria.Specialty = model.Specialty;
            existingCriteria.IsActive = model.IsActive;
            existingCriteria.Term = model.Term;
            existingCriteria.LastUpdatedAt = DateTime.Now;
            existingCriteria.Year = existingCriteria.AcademicAppointment.Year;
            existingCriteria.AcademicAppointmentId = existingCriteria.AcademicAppointment.Id;

            _unitOfWork.Repository<Criteria>().Update(existingCriteria);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(200, "Criteria updated successfully.", new { IsSuccess = true }));
        }

        // Finished / Tested
        [HttpDelete("DeleteCriteria/{criteriaId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCriteria(int criteriaId)
        {
            if (criteriaId <= 0)
                return BadRequest(new ApiResponse(400, "Invalid input data.", new { IsSuccess = false }));

            var existingCriteria = await _unitOfWork.Repository<Criteria>().FindAsync(c => c.Id == criteriaId);
            if (existingCriteria == null)
                return NotFound(new ApiResponse(404, "Criteria not found.", new { IsSuccess = false }));

            _unitOfWork.Repository<Criteria>().Delete(existingCriteria);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(200, "Criteria deleted successfully.", new { IsSuccess = true }));
        }
    }
}