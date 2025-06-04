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

        // Finished /  / Tested
        [HttpPost("SubmitDoctorProjectIdea")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> SubmitDoctorProjectIdea([FromBody] SubmitDoctorProjectIdeaDto model)
        {
            if (model == null || string.IsNullOrEmpty(model.Name) || string.IsNullOrEmpty(model.Description))
                return BadRequest(CreateErrorResponse400BadRequest("Invalid input data."));

            var doctorAppUserId = User.FindFirst("UserId")?.Value;
            if (doctorAppUserId == null)
                return NotFound(CreateErrorResponse404NotFound("Doctor not found."));

            var doctor = await _unitOfWork.Repository<Doctor>().FindAsync(d => d.AppUserId == doctorAppUserId);
            if (doctor == null)
                return NotFound(CreateErrorResponse404NotFound("Doctor not found."));

            var doctorIdeaExists = await _unitOfWork.Repository<DoctorProjectIdea>().FindAsync(p => p.Name == model.Name);
            if (doctorIdeaExists != null)
                return BadRequest(CreateErrorResponse400BadRequest("Project idea with this name already exists, Please enter other project name."));

            var currentActiveAcademicYearAppointment = await _unitOfWork.Repository<AcademicAppointment>().FindAsync(a => a.Status == "Active");

            var newDoctorProjectIdea = new DoctorProjectIdea
            {
                Name = model.Name,
                Description = model.Description,
                DoctorId = doctor.Id,
                //AcademicAppointmentId = currentActiveAcademicYearAppointment.Id,
            };
            
            await _unitOfWork.Repository<DoctorProjectIdea>().AddAsync(newDoctorProjectIdea);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(200, $"Project submitted successfully, Please wait until admin review this idea.", new { IsSuccess = true }));
        }

        // Finished / Reviewed / Tested
        [HttpPost("SubmitTeamProjectIdea")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> SubmitTeamProjectIdea([FromBody] ProjectIdeaFromTeamDto model)
        {
            if (model == null)
                return BadRequest(CreateErrorResponse400BadRequest("Invalid input data.")); ;

            var leaderAppUserId = User.FindFirst("UserId")?.Value;
            if (leaderAppUserId == null)
                return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));

            var leader = await _unitOfWork.Repository<Student>().FindAsync(l => l.AppUserId == leaderAppUserId);
            if (leader == null)
                return NotFound(CreateErrorResponse404NotFound("Leader not found."));

            if(leader.InTeam == false && leader.LeaderOfTeamId == null && leader.TeamId == null)
                return BadRequest(CreateErrorResponse400BadRequest("You're not in a team."));

            var team = await _unitOfWork.Repository<Team>().FindAsync(t => t.Id == model.TeamId);
            if (team == null)
                return NotFound(CreateErrorResponse404NotFound("Team not found."));

            if(team.HasProject)
                return BadRequest(CreateErrorResponse400BadRequest("Your team already has a project idea."));

            var teamIdeaExists = await _unitOfWork.Repository<TeamProjectIdea>().FindAsync(p => p.Name == model.Name);
            if (teamIdeaExists != null)
                return BadRequest(CreateErrorResponse400BadRequest("Project idea with this name already exists, Please enter other project idea name."));

            var newTeamProjectIdea = new TeamProjectIdea
            {
                Name = model.Name,
                Description = model.Description,
                TeamId = model.TeamId,
                LeaderId = leader.Id,
            };

            await _unitOfWork.Repository<TeamProjectIdea>().AddAsync(newTeamProjectIdea);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(200, "Project submitted successfully, Please wait until admin review this idea.", new { IsSuccess = true }));
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
            var pendingTeamProjects = await _projectRepository.GetTeamProjectIdeasByStatusAsync(StatusType.Pending.ToString());
            if (pendingTeamProjects == null || !pendingTeamProjects.Any())
                return NotFound(CreateErrorResponse404NotFound("No pending project ideas found."));

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

            await _unitOfWork.CompleteAsync();

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
            await _unitOfWork.CompleteAsync();

            var pendingTeamProjectIdeas = _dbContext.TeamProjectIdeas
                                                    .Where(p => p.LeaderId == team.LeaderId && p.Status == "Pending" && p.TeamId == team.Id && p.Id != teamProjectIdea.Id)
                                                    .ToList();
            foreach (var p in pendingTeamProjectIdeas)
                p.Status = "Rejected";

            await _unitOfWork.CompleteAsync();

            var pendingTeamRequestsForDoctorProjectIdeas = await _dbContext.TeamsRequestDoctorProjectIdeas
                                                                           .Where(tr => tr.TeamId == team.Id && tr.LeaderId == team.LeaderId && tr.Status == "Pending")
                                                                           .ToListAsync();
            foreach (var tr in pendingTeamRequestsForDoctorProjectIdeas)
                tr.Status = "Rejected";

            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(200, $"Status Of this project Updated To '{model.NewStatus.ToLower()}' Successfully!", new { IsSuccess = true }));
        }

        // Finished / Tested
        [HttpGet("AcceptedDoctorProjectIdeas")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetAllAcceptedDoctorProjectIdeasForViewStudents()
        {
            var acceptedDoctorProjects = await _projectRepository.GetDoctorProjectIdeasByStatusAndDoctorIdIfNeededAsync("Accepted");
            if (acceptedDoctorProjects == null || !acceptedDoctorProjects.Any())
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

            var existingTeamRequest = await _dbContext.TeamsRequestDoctorProjectIdeas.FirstOrDefaultAsync(tr => tr.TeamId == teamExists.Id &&
                                                             tr.DoctorProjectIdeaId == projectExists.Id &&
                                                             tr.LeaderId == leader.Id &&
                                                             tr.DoctorId == projectExists.DoctorId &&
                                                             tr.Status == "Pending");
            if (existingTeamRequest != null)
                return BadRequest(new ApiResponse(400, "A request exists before that for this project idea.", new { IsSuccess = false }));

            var projectRequest = new TeamRequestDoctorProjectIdea
            {
                DoctorProjectIdeaId = model.ProjectId,
                TeamId = teamExists.Id,
                LeaderId = leader.Id,
                DoctorId = model.DoctorId
            };

            await _unitOfWork.Repository<TeamRequestDoctorProjectIdea>().AddAsync(projectRequest);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(200, $"Request of this project successfully submitted!", new { IsSuccess = true }));
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
                return NotFound(new ApiResponse(404, $"No team requests for project ideas.", new { IsSuccess = false }));

            return Ok(new ApiResponse(200, "Pending team requests for project ideas retrieved successfully.", new { IsSuccess = true, pendingTeamRequests }));
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

            var projectRequest = await _dbContext.TeamsRequestDoctorProjectIdeas.Include(r => r.DoctorProjectIdea).FirstOrDefaultAsync(r => r.Id == model.RequestId);
            if (projectRequest == null)
                return NotFound(new ApiResponse(404, "Request not found.", new { IsSuccess = false }));

            var team = await _unitOfWork.Repository<Team>().FindAsync(t => t.Id == projectRequest.TeamId);
            if (team == null)
                return NotFound(new ApiResponse(404, "Team not found.", new { IsSuccess = false }));

            if (team.HasProject && model.NewStatus == "Accepted")
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

                // Set the Taken property of the DoctorProjectIdea to true  
                projectRequest.DoctorProjectIdea.Taken = true;
                _unitOfWork.Repository<DoctorProjectIdea>().Update(projectRequest.DoctorProjectIdea);

                var pendingRequests = await _unitOfWork.Repository<TeamRequestDoctorProjectIdea>()
                    .FindAllAsync(r => r.LeaderId == leader.Id && r.Status == "Pending" && r.TeamId == team.Id && r.Id != projectRequest.Id);

                foreach (var r in pendingRequests)
                {
                    r.Status = "Rejected";
                    _unitOfWork.Repository<TeamRequestDoctorProjectIdea>().Update(r);
                }
            }

            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(200, "Project Request approved and other requests rejected successfully!", new { IsSuccess = true }));
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

        // Finished / Reviewed / Tested
        [HttpGet("FinalProjectIdea/{teamId}")]
        [Authorize(Roles = "Admin, Student, Doctor")]
        public async Task<IActionResult> GetFinalProjectIdeaById(int teamId)
        {
            if (teamId <= 0)
                return BadRequest(CreateErrorResponse400BadRequest("TeamId must be positive number."));

            var finalProjectIdea = await _dbContext.FinalProjectIdeas.Include(f => f.Team)
                                                                     .Include(f => f.Supervisor)
                                                                     .FirstOrDefaultAsync(f => f.TeamId == teamId);
            if (finalProjectIdea == null)
                return NotFound(CreateErrorResponse404NotFound("Final project idea not found."));

            var projectIdea = new FinalProjectIdeaDto
            {
                ProjectId = finalProjectIdea.ProjectId,
                ProjectName = finalProjectIdea?.ProjectName,
                ProjectDescription = finalProjectIdea?.ProjectDescription,
                TeamId = finalProjectIdea?.TeamId,
                TeamName = finalProjectIdea?.Team?.Name,
                SupervisorId = finalProjectIdea?.SupervisorId,
                SupervisorName = finalProjectIdea?.Supervisor?.FullName,
                PostedBy = finalProjectIdea?.PostedBy
            };
            return Ok(new ApiResponse(200, "Final project idea retrieved successfully.", new { IsSuccess = true, projectIdea }));
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
