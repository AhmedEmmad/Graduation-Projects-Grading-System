using GradingManagementSystem.Core;
using GradingManagementSystem.Core.DTOs;
using GradingManagementSystem.Core.Entities;
using GradingManagementSystem.Core.CustomResponses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GradingManagementSystem.APIs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CriteriaController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public CriteriaController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost("CreateCriteria")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCriteria([FromBody] CreateCriteriaDto model)
        {
            if (model is null)
                return BadRequest(new ApiResponse(400, "Invalid input data.", new { IsSuccess = false }));

            var existingCriteria = await _unitOfWork.Repository<Criteria>()
                                                    .FindAsync(c => c.Name == model.Name && c.Evaluator == model.Evaluator && c.Specialty == model.Specialty);
            if (existingCriteria != null)
                return BadRequest(new ApiResponse(400, $"Criteria with the same name and evaluator already exists for this program: '{model.Specialty}'.", new { IsSuccess = false }));

            var academicAppointment = await _unitOfWork.Repository<AcademicAppointment>()
                                                       .FindAsync(a => a.Year == model.Year && a.Status == model.Term);
            if (academicAppointment == null)
                return BadRequest(new ApiResponse(400, "You can't create a criteria in this time.", new { IsSuccess = false }));

            var newCriteria = new Criteria
            {
                Name = model.Name,
                Description = model.Description,
                MaxGrade = model.MaxGrade,
                Evaluator = model.Evaluator,
                GivenTo = model.GivenTo,
                Specialty = model.Specialty,
                Year = model.Year,
                Term = model.Term,
                AcademicAppointmentId = academicAppointment.Id,
            };

            await _unitOfWork.Repository<Criteria>().AddAsync(newCriteria);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(200, $"Criteria created successfully with ID: '{newCriteria.Id}' for the Program: '{newCriteria.Specialty}'.", new { IsSuccess = true }));
        }
        
        [HttpGet("All")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllCriterias()
        {
            var existingCriterias = await _unitOfWork.Repository<Criteria>().GetAllAsync();
            if (existingCriterias == null || !existingCriterias.Any())
                return NotFound(new ApiResponse(404, "No Criterias Found.", new { IsSuccess = false } ));

            var criteriaList = existingCriterias.Select(c => new CriteriaDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                MaxGrade = c.MaxGrade,
                Evaluator = c.Evaluator,
                GivenTo = c.GivenTo,
                Year = c.Year,
                Program = c.Specialty,
                Term = c.Term,
            }).ToList();
            return Ok(new ApiResponse(200, "Criterias retrieved successfully.", new { IsSuccess = true , criteriaList }));
        }
        
        [HttpGet("{criteriaId}")]
        [Authorize(Roles = "Admin, Doctor")]
        public async Task<IActionResult> GetCriteriaById(int criteriaId)
        {
            if(criteriaId <= 0)
                return BadRequest(new ApiResponse(400, "Invalid input data.", new { IsSuccess = false }));

            var existingCriteria = await _unitOfWork.Repository<Criteria>().FindAsync(c => c.Id == criteriaId);
            if (existingCriteria == null)
                return NotFound(new ApiResponse(404, "Criteria not found.", new { IsSuccess = false }));

            var criteria = new CriteriaDto
            {
                Id = existingCriteria.Id,
                Name = existingCriteria.Name,
                Description = existingCriteria.Description,
                MaxGrade = existingCriteria.MaxGrade,
                Evaluator = existingCriteria.Evaluator,
                GivenTo = existingCriteria.GivenTo,
                Year = existingCriteria.Year,
                Program = existingCriteria.Specialty,
                Term = existingCriteria.Term,
            };
            return Ok(new ApiResponse(200, "Criteria retrieved successfully.", new { IsSuccess = true , criteria }));
        }

        //[HttpGet("AllCriteriasForGrading")]
        //[Authorize(Roles = "Admin, Doctor")]
        //public async Task<IActionResult> GetAllCriteriasForGradingByRole()
        //{

        //}

        [HttpPut("UpdateCriteria/{criteriaId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCriteria(int criteriaId, [FromBody] CriteriaDto model)
        {
            var existingCriteria = await _unitOfWork.Repository<Criteria>().GetByIdAsync(criteriaId);
            if (existingCriteria == null)
                return NotFound(new ApiResponse(404, "Criteria not found.", new { IsSuccess = false }));

            existingCriteria.Name = model.Name;
            existingCriteria.Description = model.Description;
            existingCriteria.MaxGrade = model.MaxGrade;
            existingCriteria.Evaluator = model.Evaluator;
            existingCriteria.GivenTo = model.GivenTo;

            _unitOfWork.Repository<Criteria>().Update(existingCriteria);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(200, "Criteria updated successfully.", new { IsSuccess = true }));
        }

        [HttpDelete("DeleteCriteria/{criteriaId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCriteria(int criteriaId)
        {
            var existingCriteria = await _unitOfWork.Repository<Criteria>().GetByIdAsync(criteriaId);
            if (existingCriteria == null)
                return NotFound(new ApiResponse(404, "Criteria not found.", new { IsSuccess = false }));

            _unitOfWork.Repository<Criteria>().Delete(existingCriteria);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(200, "Criteria deleted successfully.", new { IsSuccess = true }));
        }


        /*
        [HttpPost("GetCriteriaForDoctorRole")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetCriteriaForDoctorRole([FromBody] GetDoctorCriteriaDto model)
        {
            // التحقق من أن الفريق موجود
            var team = await _unitOfWork.Repository<Team>().GetByIdAsync(model.TeamId);
            if (team == null)
                return NotFound(new ApiResponse(404, "Team not found"));

            // استدعاء قائمة المعايير بناءً على الدور
            List<Criteria> criteriaList;

            if (model.Role == EvaluationRole.Supervisor.ToString())
            {
                // التحقق من إذا كان الدكتور هو المشرف على الفريق
                if (team.SupervisorId != model.DoctorId)
                    return BadRequest(new ApiResponse(400, "Doctor is not supervisor of this team."));

                // جلب المعايير للمشرف
                criteriaList = (await _unitOfWork.Repository<Criteria>()
                    .FindAllAsync(c => c.Evaluator == EvaluationRole.Supervisor)).ToList();
            }
            else if (model.Role == EvaluationRole.Examiner.ToString())
            {
                // التحقق من إذا كان الدكتور ضمن لجنة التقييم لهذا الفريق
                var committee = await _unitOfWork.Repository<Committee>()
                    .FindAsync(c => c.TeamId == model.TeamId &&
                                    c.DoctorCommittees.Any(dc => dc.DoctorId == model.DoctorId));

                if (committee == null)
                    return BadRequest(new ApiResponse(400, "Doctor is not in committee for this team."));

                // جلب المعايير للجنة
                criteriaList = (await _unitOfWork.Repository<Criteria>()
                    .FindAllAsync(c => c.Evaluator == EvaluationRole.Examiner)).ToList();
            }
            else
            {
                return BadRequest(new ApiResponse(400, "Invalid Role. Must be Supervisor or Committee."));
            }


            // تبسيط المعايير لتشمل الاسم والدرجة القصوى
            var simplifiedCriteria = criteriaList.Select(c => new SimpleCriteriaDto
            {
                Id = c.Id,
                Name = c.Name,
                MaxGrade = c.MaxGrade
            }).ToList();

            return Ok(new ApiResponse(200, "Criteria fetched successfully", simplifiedCriteria));
        }
        */

    }
}