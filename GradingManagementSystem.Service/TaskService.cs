using GradingManagementSystem.Core;
using GradingManagementSystem.Core.CustomResponses;
using GradingManagementSystem.Core.DTOs;
using GradingManagementSystem.Core.Entities;
using GradingManagementSystem.Core.Repositories.Contact;
using GradingManagementSystem.Core.Services.Contact;

namespace GradingManagementSystem.Service
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IUnitOfWork _unitOfWork;

        public TaskService(ITaskRepository taskRepository,IUnitOfWork unitOfWork)
        {
            _taskRepository = taskRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse> CreateTaskAsync(TaskItem task, List<int> studentIds)
        {
            if (task == null)
                return new ApiResponse(400, "Error in task creation.", new { IsSuccess = false });

            await _unitOfWork.Repository<TaskItem>().AddAsync(task);
            await _unitOfWork.CompleteAsync();

            foreach (var studentId in studentIds)
            {
                var student = await _unitOfWork.Repository<Student>().FindAsync(s => s.Id == studentId && s.TeamId == task.TeamId);
                if (student != null)
                {
                    var existingMember = await _unitOfWork.Repository<TaskMember>().FindAsync(tm => tm.StudentId == studentId && tm.TaskId == task.Id);
                    if (existingMember == null)
                    {
                        var taskMember = new TaskMember
                        {
                            TaskId = task.Id,
                            StudentId = studentId,
                            TeamId = task.TeamId,
                        };
                    await _unitOfWork.Repository<TaskMember>().AddAsync(taskMember);
                    }
                }
                else
                    return new ApiResponse(400, "Error in task members creation.", new { IsSuccess = false });
            }

            await _unitOfWork.CompleteAsync();
            return new ApiResponse(200, $"Task created successfully with id: '{task.Id}'.", new { IsSuccess = true });
        }

        public async Task<ApiResponse> GetAllTeamTasksByTeamIdAsync(int teamId)
        {
            var teamTasks = await _taskRepository.GetTeamTasksByTeamIdAsync(teamId);

            if (teamTasks == null || !teamTasks.Any())
                return new ApiResponse(404, $"Tasks not found for this team.", new { IsSuccess = false });

            var taskMembers = await _taskRepository.GetTaskMembersByTeamAsync(teamId);

            if (taskMembers == null || !taskMembers.Any())
                return new ApiResponse(404, "Task members not found for this team.", new { IsSuccess = false });

            var teamTasksWithTaskMembers = teamTasks.Select(t => new TaskDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Deadline = t.Deadline,
                StartTime = t.StartTime,
                Status = t.Status,
                SupervisorId = t.SupervisorId,
                TeamId = t.TeamId,
                TeamName = t.Team.Name,
                TaskMembers = t.TaskMembers.Select(tm => new TaskMemberDto
                {
                    TaskId = tm.TaskId,
                    TaskName = tm.Task?.Name,
                    StudentId = tm.StudentId,
                    StudentName = tm.Student?.FullName,
                    StudentProfilePicture = tm.Student?.AppUser?.ProfilePicture,
                    TeamId = tm.TeamId,
                    TeamName = tm.Team?.Name,
                    Status = tm.Status
                }).ToHashSet()
            }).ToList();

            return new ApiResponse(200, $"Tasks of this team id: '{teamId}' retrieved successfully.", new { IsSuccess = true, teamTasksWithTaskMembers });
        }
    }
}
