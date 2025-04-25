using GradingManagementSystem.Core;
using GradingManagementSystem.Core.DTOs;
using GradingManagementSystem.Core.Entities;
using GradingManagementSystem.Core.CustomResponses;
using GradingManagementSystem.Core.Repositories.Contact;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GradingManagementSystem.Repository.Data.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace GradingManagementSystem.APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProjectRepository _projectRepository;
        private readonly GradingManagementSystemDbContext _dbContext;

        public ProjectsController(IUnitOfWork unitOfWork, IProjectRepository projectRepository, GradingManagementSystemDbContext dbContext)
        {
            _unitOfWork = unitOfWork;
            _projectRepository = projectRepository;
            _dbContext = dbContext;
        }

        // Finished / Tested
        [HttpPost("SubmitDoctorProjectIdea")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> SubmitDoctorProjectIdea([FromBody] ProjectIdeaFromDrDto model)
        {
            if (model == null)
                return BadRequest(new ApiResponse(400, "Invalid input data.", new { IsSuccess = false }));

            var doctorAppUserId = User.FindFirst("UserId")?.Value;
            if (doctorAppUserId == null)
                return NotFound(new ApiResponse(404, "Doctor not found.", new { IsSuccess = false }));

            var doctor = await _unitOfWork.Repository<Doctor>().FindAsync(d => d.AppUserId == doctorAppUserId);
            if (doctor == null)
                return NotFound(new ApiResponse(404, "Doctor not found.", new { IsSuccess = false }));

            var doctorIdeaExists = await _unitOfWork.Repository<DoctorProjectIdea>().FindAsync(p => p.Name == model.Name);
            if (doctorIdeaExists != null)
                return BadRequest(new ApiResponse(400, "Project idea with this name already exists, Please enter other project name.", new { IsSuccess = false }));

            var newDoctorProjectIdea = new DoctorProjectIdea
            {
                Name = model.Name,
                Description = model.Description,
                DoctorId = doctor.Id
            };
            
            await _unitOfWork.Repository<DoctorProjectIdea>().AddAsync(newDoctorProjectIdea);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(200, $"Project submitted successfully with id: '{newDoctorProjectIdea.Id}', Please wait until admin review this idea.", new { IsSuccess = true }));
        }

        // Finished / Tested
        [HttpPost("SubmitTeamProjectIdea")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> SubmitTeamProjectIdea([FromBody] ProjectIdeaFromTeamDto model)
        {
            if (model == null)
                return BadRequest(new ApiResponse(400, "Invalid input data.", new { IsSuccess = false})); ;

            var leaderAppUserId = User.FindFirst("UserId")?.Value;
            if (leaderAppUserId == null)
                return NotFound(new ApiResponse(404, "Leader not found.", new { IsSuccess = false }));

            var leader = await _unitOfWork.Repository<Student>().FindAsync(l => l.AppUserId == leaderAppUserId);
            if (leader == null)
                return NotFound(new ApiResponse(404, "Leader not found.", new { IsSuccess = false }));

            if(leader.InTeam == false && leader.LeaderOfTeamId == null && leader.TeamId == null)
                return BadRequest(new ApiResponse(400, "You're not in a team.", new { IsSuccess = false }));

            if(leader.LeaderOfTeam?.HasProject == true)
                return BadRequest(new ApiResponse(400, "Your team already has a project.", new { IsSuccess = false }));

            var teamIdeaExists = await _unitOfWork.Repository<TeamProjectIdea>().FindAsync(p => p.Name == model.Name);
            if (teamIdeaExists != null)
                return BadRequest(new ApiResponse(400, "Project idea with this name already exists, Please enter other project name.", new { IsSuccess = false }));

            var newTeamProjectIdea = new TeamProjectIdea
            {
                Name = model.Name,
                Description = model.Description,
                TeamId = model.TeamId,
                LeaderId = leader.Id,
            };

            await _unitOfWork.Repository<TeamProjectIdea>().AddAsync(newTeamProjectIdea);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(200, $"Project submitted successfully with id: '{newTeamProjectIdea.Id}', Please wait until admin review this idea.", new { IsSuccess = true }));
        }

        // Finished / Tested
        [HttpGet("PendingDoctorProjectIdeas")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllPendingDoctorProjectIdeas()
        {
            var pendingDoctorProjects = await  _projectRepository.GetDoctorProjectIdeasByStatusAndDoctorIdIfNeededAsync("Pending");
            if (pendingDoctorProjects == null || !pendingDoctorProjects.Any())
                return NotFound(new ApiResponse(404, "No pending project ideas found.", new {IsSuccess = false}));

            return Ok(new ApiResponse(200, "Pending project ideas retrieved successfully.", new { IsSuccess = true, pendingDoctorProjects } ));
        }

        // Finished / Tested
        [HttpGet("PendingTeamProjectIdeas")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllPendingTeamProjectIdeas()
        {
            var pendingTeamProjects = await _projectRepository.GetTeamProjectIdeasByStatusAsync("Pending");
            if (pendingTeamProjects == null || !pendingTeamProjects.Any())
                return NotFound(new ApiResponse(404, "No pending project ideas found.", new { IsSuccess = false }));

            return Ok(new ApiResponse(200, "Pending project ideas retrieved successfully.", new { IsSuccess = true, pendingTeamProjects }));
        }

        // Finished / Tested
        [HttpPut("ReviewDoctorProjectIdea")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReviewDoctorProjectIdea([FromBody] ReviewDoctorProjectIdeaDto model)
        {
            if (model == null)
                return BadRequest(new ApiResponse(400, "Invalid input data.", new { IsSuccess = false }));

            var adminAppUserId = User.FindFirst("UserId")?.Value;
            if (adminAppUserId == null)
                return NotFound(new ApiResponse(404, "Admin not found.", new { IsSuccess = false }));

            var admin = await _unitOfWork.Repository<Admin>().FindAsync(a => a.AppUserId == adminAppUserId);
            if (admin == null)
                return NotFound(new ApiResponse(404, "Admin not found.", new { IsSuccess = false }));

            if (model.NewStatus != "Accepted" && model.NewStatus != "Rejected")
                return BadRequest(new ApiResponse(400, "Invalid status value, Use 'Accepted' or 'Rejected'.", new { IsSuccess = false }));
            
            var doctorProjectIdea = await _unitOfWork.Repository<DoctorProjectIdea>().FindAsync(p => p.Id == model.ProjectId);
            if (doctorProjectIdea == null)
                return NotFound(new ApiResponse(404, "Project not found.", new { IsSuccess = false }));

            doctorProjectIdea.Status = model.NewStatus == "Accepted" ? StatusType.Accepted.ToString() : StatusType.Rejected.ToString();
            _unitOfWork.Repository<DoctorProjectIdea>().Update(doctorProjectIdea);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(200, $"Status Of Project id: '{doctorProjectIdea.Id}' updated to '{model.NewStatus.ToLower()}' successfully!", new { IsSuccess = true }));
        }

        // Finished / Tested
        [HttpPut("ReviewTeamProjectIdea")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReviewTeamProjectIdea([FromBody] ReviewTeamProjectIdeaDto model)
        {
            if (model == null)
                return BadRequest(new ApiResponse(400, "Invalid input data.", new { IsSuccess = false }));

            var adminAppUserId = User.FindFirst("UserId")?.Value;
            if (adminAppUserId == null)
                return NotFound(new ApiResponse(404, "Admin not found.", new { IsSuccess = false }));

            var admin = await _unitOfWork.Repository<Admin>().FindAsync(a => a.AppUserId == adminAppUserId);
            if (admin == null)
                return NotFound(new ApiResponse(404, "Admin not found.", new { IsSuccess = false }));

            if (model.NewStatus != "Accepted" && model.NewStatus != "Rejected")
                return BadRequest(new ApiResponse(400, "Invalid status value, Use 'Accepted' or 'Rejected'.", new { IsSuccess = false }));

            var teamProjectIdea = await _unitOfWork.Repository<TeamProjectIdea>().FindAsync(p => p.Id == model.ProjectId);
            if (teamProjectIdea == null)
                return NotFound(new ApiResponse(404, "Project not found.", new { IsSuccess = false }));

            var team = await _unitOfWork.Repository<Team>().FindAsync(t => t.Id == teamProjectIdea.TeamId);
            if (team == null)
                return NotFound(new ApiResponse(404, "Team not found.", new { IsSuccess = true }));

            teamProjectIdea.Status = model.NewStatus == "Accepted" ? StatusType.Accepted.ToString() : StatusType.Rejected.ToString();
            _unitOfWork.Repository<TeamProjectIdea>().Update(teamProjectIdea);

            if (model.NewStatus == "Accepted")
            {
                team.HasProject = true;
                team.SupervisorId = model.SupervisorId;
                _unitOfWork.Repository<Team>().Update(team);
            }

            var finalProjectIdea = new FinalProjectIdea
            {
                ProjectId = teamProjectIdea.Id,
                ProjectName = teamProjectIdea.Name,
                ProjectDescription = teamProjectIdea.Description,
                TeamProjectIdeaId = teamProjectIdea.Id,
                TeamRequestDoctorProjectIdeaId = null,
                SupervisorId = model.SupervisorId,
                TeamId = team.Id,
                PostedBy = "Team"
            };
            await _unitOfWork.Repository<FinalProjectIdea>().AddAsync(finalProjectIdea);

            var pendingTeamProjectIdeas = _dbContext.TeamProjectIdeas
                                                    .Where(p => p.LeaderId == team.LeaderId && p.Status == "Pending" && p.TeamId == team.Id && p.Id != teamProjectIdea.Id)
                                                    .ToList();
            foreach (var p in pendingTeamProjectIdeas)
                p.Status = "Rejected";

            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(200, $"Status Of Project id: '{teamProjectIdea.Id}' Updated To '{model.NewStatus.ToLower()}' Successfully!", new { IsSuccess = true }));
        }

        // Finished / Tested
        [HttpGet("AcceptedDoctorProjectIdeas")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetAllAcceptedDoctorProjectIdeasForViewStudents()
        {
            var acceptedDoctorProjects = await _projectRepository.GetDoctorProjectIdeasByStatusAndDoctorIdIfNeededAsync("Accepted");
            if (acceptedDoctorProjects is null || !acceptedDoctorProjects.Any())
                return NotFound(new ApiResponse(404, "No accepted project ideas found.", new { IsSuccess = false }));

            return Ok(new ApiResponse(200, "Accepted project ideas retrieved successfully", new { IsSuccess = true , acceptedDoctorProjects } ));
        }

        // Finished / Tested
        [HttpGet("AcceptedProjectIdeasForDoctor/{doctorId}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetAllAcceptedProjectIdeasForDoctor(int doctorId)
        {
            if(doctorId <= 0)
                return BadRequest(new ApiResponse(400, "Invalid input data.", new { IsSuccess = false }));

            var doctor = await _unitOfWork.Repository<Doctor>().FindAsync(d => d.Id == doctorId);
            if (doctor == null)
                return NotFound(new ApiResponse(404, "Doctor not found.", new { IsSuccess = false }));

            var acceptedProjectIdeasForDoctor = await _projectRepository.GetDoctorProjectIdeasByStatusAndDoctorIdIfNeededAsync("Accepted", doctorId);

            if (acceptedProjectIdeasForDoctor == null || !acceptedProjectIdeasForDoctor.Any())
                return NotFound(new ApiResponse(404, "No accepted project ideas found for his doctor.", new { IsSuccess = false }));

            return Ok(new ApiResponse(200, "Accepted project ideas retrieved successfully for his doctor.", new { IsSuccess = true, acceptedProjectIdeasForDoctor }));
        }

        // Finished / Tested
        [HttpGet("AcceptedTeamProjectIdeas")]
        [Authorize(Roles = "Admin, Student, Doctor")]
        public async Task<IActionResult> GetAllAcceptedTeamProjectIdeas()
        {
            var acceptedTeamProjects = await _projectRepository.GetTeamProjectIdeasByStatusAsync("Accepted");
            if (acceptedTeamProjects == null || !acceptedTeamProjects.Any())
                return NotFound(new ApiResponse(404, "No Accepted Project Ideas Found.", new { IsSuccess = false }));
            
            return Ok(new ApiResponse(200, "Accepted Project Ideas Retrieved Successfully", new { IsSuccess = true, acceptedTeamProjects }));
        }

        // Finished / Tested
        [HttpPost("RequestDoctorProjectIdea")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> RequestProjectIdeaFromDrByTeam([FromBody] RequestProjectIdeaFromDrByTeamDto model)
        {
            if (model == null)
                return BadRequest(new ApiResponse(400, "Invalid input data.", new { IsSuccess = false }));

            var leaderAppUserId = User.FindFirst("UserId")?.Value;
            var leader = await _unitOfWork.Repository<Student>().FindAsync(l => l.AppUserId == leaderAppUserId);
            if (leader == null)
                return NotFound(new ApiResponse(404, "Student not found.", new { IsSuccess = false }));

            var teamExists = await _unitOfWork.Repository<Team>().FindAsync(t => t.Id == model.TeamId);
            if (teamExists == null)
                return NotFound(new ApiResponse(404, "Team not found.", new { IsSuccess = false }));

            var projectExists = await _unitOfWork.Repository<DoctorProjectIdea>().FindAsync(p => p.Id == model.ProjectId && p.Status == "Accepted");
            if (projectExists is null)
                return NotFound(new ApiResponse(404, "Project not found or not accepted.", new { IsSuccess = false }));

            if(leader.TeamId != model.TeamId || leader.LeaderOfTeamId == null)
                return BadRequest(new ApiResponse(400, "You're not in this team.", new { IsSuccess = false }));

            if(leader.LeaderOfTeam?.HasProject == true)
                return BadRequest(new ApiResponse(400, "Your team already has a project.", new { IsSuccess = false }));

            var projectRequest = new TeamRequestDoctorProjectIdea
            {
                DoctorProjectIdeaId = model.ProjectId,
                TeamId = teamExists.Id,
                LeaderId = leader.Id,
                DoctorId = model.DoctorId
            };

            await _unitOfWork.Repository<TeamRequestDoctorProjectIdea>().AddAsync(projectRequest);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(200, $"Request Of Project id: '{model.ProjectId}' successfully submitted!", new { IsSuccess = true }));
        }

        // Finished / Tested
        [HttpGet("PendingTeamRequestsForDoctorProjectIdeas/{doctorId}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetAllPendingTeamRequestsForDoctorProjectIdeas(int doctorId)
        {
            if (doctorId <= 0)
                return BadRequest(new ApiResponse(400, "Invalid input data.", new { IsSuccess = false }));

            var doctor = await _unitOfWork.Repository<Doctor>().FindAsync(d => d.Id == doctorId);
            if (doctor == null)
                return NotFound(new ApiResponse(404, "Doctor not found.", new { IsSuccess = false }));

            var pendingTeamRequests = await _projectRepository.GetPendingTeamRequestsForDoctorProjectIdeasAsync(doctor.Id);
            if (pendingTeamRequests is null || !pendingTeamRequests.Any())
                return NotFound(new ApiResponse(404, $"No team requests for project ideas belong to his doctor: '{doctor.FullName}'.", new { IsSuccess = false }));

            return Ok(new ApiResponse(200, $"Pending team requests for project ideas belong to his doctor: '{doctor.FullName}' retrieved successfully.", new { IsSuccess = true, pendingTeamRequests }));
        }

        // Finished / Tested
        [HttpPut("ReviewTeamProjectRequest")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> DoctorReviewTeamProjectIdeaRequest([FromBody] ReviewTeamProjectRequestDto model)
        {
            if (model == null)
                return BadRequest(new ApiResponse(400, "Invalid input data.", new { IsSuccess = false }));

            if (model.NewStatus != "Accepted" && model.NewStatus != "Rejected")
                return BadRequest(new ApiResponse(400, "Invalid Status Value. Use 'Accepted' or 'Rejected'.", new { IsSuccess = false }));

            //var projectRequest = await _unitOfWork.Repository<TeamRequestDoctorProjectIdea>().FindAsync(r => r.Id == model.RequestId);
            var projectRequest = await _dbContext.TeamsRequestDoctorProjectIdeas.Include(r => r.DoctorProjectIdea).FirstOrDefaultAsync(r => r.Id == model.RequestId);
            if (projectRequest == null)
                return NotFound(new ApiResponse(404, "Request not found.", new { IsSuccess = false }));

            var team = await _unitOfWork.Repository<Team>().FindAsync(t => t.Id == projectRequest.TeamId);
            if (team == null)
                return NotFound(new ApiResponse(404, "Team not found.", new { IsSuccess = false }));

            if(team.HasProject)
                return BadRequest(new ApiResponse(400, "This team already has a project.", new { IsSuccess = false }));

            var doctor = await _unitOfWork.Repository<Doctor>().FindAsync(d => d.Id == model.DoctorId && d.Id == projectRequest.DoctorId);
            if (doctor == null)
                return NotFound(new ApiResponse(404, "Doctor not found.", new { IsSuccess = false }));

            var leader = await _unitOfWork.Repository<Student>().FindAsync(s => s.Id == projectRequest.LeaderId);
            if (leader == null)
                return NotFound(new ApiResponse(404, "Leader not found.", new { IsSuccess = false }));

            projectRequest.Status = model.NewStatus == "Accepted" ? StatusType.Accepted.ToString() : StatusType.Rejected.ToString();
            _unitOfWork.Repository<TeamRequestDoctorProjectIdea>().Update(projectRequest);

            if (model.NewStatus == "Accepted")
            {
                team.HasProject = true;
                team.SupervisorId = doctor.Id;
                _unitOfWork.Repository<Team>().Update(team);
                var finalProject = new FinalProjectIdea
                {
                    ProjectId = projectRequest.DoctorProjectIdeaId,
                    ProjectName = projectRequest?.DoctorProjectIdea.Name,
                    ProjectDescription = projectRequest?.DoctorProjectIdea.Description,
                    TeamRequestDoctorProjectIdeaId = projectRequest?.Id,
                    TeamProjectIdeaId = null,
                    SupervisorId = doctor?.Id,
                    TeamId = team?.Id,
                    PostedBy = "Doctor"
                };
                await _unitOfWork.Repository<FinalProjectIdea>().AddAsync(finalProject);
                
                var pendingRequests = await _unitOfWork.Repository<TeamRequestDoctorProjectIdea>()
                    .FindAllAsync(r => r.LeaderId == leader.Id && r.Status == "Pending" && r.TeamId == team.Id && r.Id != projectRequest.Id);

                foreach (var r in pendingRequests)
                {
                    r.Status = "Rejected";
                    _unitOfWork.Repository<TeamRequestDoctorProjectIdea>().Update(r);
                }
            }
            try
            {
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse(500, "Failed to save changes.", new { IsSuccess = false, Error = ex.InnerException?.Message ?? ex.Message }));
            }

            return Ok(new ApiResponse(200, $"Project Request approved and other requests rejected successfully!", new {IsSuccess = true }));
        }

        // Finished / Tested
        [HttpGet("FinalProjectIdeas")]
        [Authorize(Roles = "Admin, Student, Doctor")]
        public async Task<IActionResult> GetAllFinalProjectIdeas()
        {
            var finalProjectIdeas = await _projectRepository.GetAllFinalProjectIdeas();
            if (finalProjectIdeas == null || !finalProjectIdeas.Any())
                return NotFound(new ApiResponse(404, "No final project ideas found.", new { IsSuccess = false }));

            return Ok(new ApiResponse(200, "Final project ideas retrieved successfully.", new { IsSuccess = true, finalProjectIdeas }));
        }

        // Finished / Tested
        [HttpGet("FinalProjectIdea/{teamId}")]
        [Authorize(Roles = "Admin, Student, Doctor")]
        public async Task<IActionResult> GetFinalProjectIdeaById(int teamId)
        {
            var finalProjectIdea = await _dbContext.FinalProjectIdeas.Include(f => f.Team)
                                                                     .Include(f => f.Supervisor)
                                                                     .FirstOrDefaultAsync(f => f.TeamId == teamId);
            if (finalProjectIdea == null)
                return NotFound(new ApiResponse(404, "Final project idea not found.", new { IsSuccess = false }));

            var projectIdea = new FinalProjectIdeaDto
            {
                ProjectId = finalProjectIdea.ProjectId,
                ProjectName = finalProjectIdea?.ProjectName,
                ProjectDescription = finalProjectIdea?.ProjectDescription,
                TeamId = finalProjectIdea?.TeamId,
                TeamName = finalProjectIdea?.Team?.Name,
                SupervisorId = finalProjectIdea?.SupervisorId,
                SupervisorName = finalProjectIdea?.Supervisor?.FullName,
                PostedBy = finalProjectIdea.PostedBy
            };
            return Ok(new ApiResponse(200, "Final project idea retrieved successfully.", new
            {
                IsSuccess = true,
                projectIdea
            }));
        }
    }
}
