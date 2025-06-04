using GradingManagementSystem.Core;
using GradingManagementSystem.Core.CustomResponses;
using GradingManagementSystem.Core.DTOs;
using GradingManagementSystem.Core.Entities;
using GradingManagementSystem.Core.Services.Contact;
using GradingManagementSystem.Repository.Data.DbContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GradingManagementSystem.APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITaskService _taskService;
        private readonly GradingManagementSystemDbContext _dbContext;

        public TasksController(IUnitOfWork unitOfWork, ITaskService taskService, GradingManagementSystemDbContext dbContext)
        {
            _unitOfWork = unitOfWork;
            _taskService = taskService;
            _dbContext = dbContext;
        }

        // Finished / Reviewed / Tested
        [HttpPost("CreateTask")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> DoctorCreateTaskForStudents([FromForm] CreateTaskDto model)
        {
            if (model == null)
                return BadRequest(CreateErrorResponse400BadRequest("Invalid input task data."));

            if (model.Deadline < DateTime.Now)
                return BadRequest(CreateErrorResponse400BadRequest("Task deadline must be in the future."));

            var supervisor = await _unitOfWork.Repository<Doctor>().FindAsync(d => d.Id == model.SupervisorId);
            if (supervisor == null)
                return NotFound(CreateErrorResponse404NotFound("Supervisor not found."));

            var team = await _unitOfWork.Repository<Team>().FindAsync(t => t.Id == model.TeamId);
            if (team == null)
                return NotFound(CreateErrorResponse404NotFound("Team not found."));

            var task = new TaskItem
            {
                Name = model.Name,
                Description = model.Description,
                Deadline = model.Deadline,
                SupervisorId = model.SupervisorId,
                TeamId = model.TeamId,
            };

            var result = await _taskService.CreateTaskAsync(task, model.StudentIds);
            if (result.StatusCode == 400)
                return BadRequest(result);

            return Ok(result);
        }

        // Finished / Tested
        [HttpGet("TeamTasks/{teamId}")]
        [Authorize(Roles = "Doctor, Student")]
        public async Task<IActionResult> GetAllTeamTasks(int teamId)
        {
            if (teamId <= 0)
                return BadRequest(new ApiResponse(400, "Invalid input data.", new { IsSuccess = false }));

            var result = await _taskService.GetAllTeamTasksByTeamIdAsync(teamId);

            if (result.StatusCode == 404)
                return NotFound(result);

            return Ok(result);
        }

        // Finished / Tested
        [HttpPut("ReviewTask/{taskId}/{studentId}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> ReviewTask(int taskId, int studentId)
        {
            if (taskId <= 0 || studentId <= 0)
                return BadRequest(new ApiResponse(400, "Invalid input data.", new { IsSuccess = false }));

            //var task = await _unitOfWork.Repository<TaskItem>().FindAsync(t => t.Id == taskId);
            var task = await _dbContext.Tasks.Include(t => t.TaskMembers).FirstOrDefaultAsync(t => t.Id == taskId);
            if (task == null)
                return NotFound(new ApiResponse(404, "Task not found.", new { IsSuccess = false }));

            //var taskMemberEntity = await _unitOfWork.Repository<TaskMember>().FindAsync(tm => tm.Id == taskMember);
            var taskMemberEntity = task.TaskMembers.FirstOrDefault(tm => tm.StudentId == studentId);
            if (taskMemberEntity == null)
                return NotFound(new ApiResponse(404, "Task member not found for this task.", new { IsSuccess = false }));

            taskMemberEntity.Status = StatusType.Completed.ToString();
            taskMemberEntity.FinishedAt = DateTime.Now;
            _dbContext.TaskMembers.Update(taskMemberEntity);
            await _unitOfWork.CompleteAsync();

            if (task.TaskMembers.All(tm => tm.Status == StatusType.Completed.ToString()))
            {
                task.Status = StatusType.Completed.ToString();
                _dbContext.Tasks.Update(task);
            }
            await _unitOfWork.CompleteAsync();
            return Ok(new ApiResponse(200, "Task reviewed successfully.", new { IsSuccess = true }));
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