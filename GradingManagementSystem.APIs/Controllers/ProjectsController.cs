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

        public ProjectsController(IUnitOfWork unitOfWork,
                                  IProjectRepository projectRepository,
                                  GradingManagementSystemDbContext dbContext)
        {
            _unitOfWork = unitOfWork;
            _projectRepository = projectRepository;
            _dbContext = dbContext;
        }

        // Finished / Reviewed / Tested / Edited / D
        [HttpPost("SubmitDoctorProjectIdea")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> SubmitDoctorProjectIdea([FromBody] SubmitDoctorProjectIdeaDto model)
        {
            if (model is null || string.IsNullOrEmpty(model.Name) || string.IsNullOrEmpty(model.Description))
                return BadRequest(CreateErrorResponse400BadRequest("Invalid input data."));

            var doctorAppUserId = User.FindFirst("UserId")?.Value;
            if (doctorAppUserId == null)
                return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));

            var doctor = await _unitOfWork.Repository<Doctor>().FindAsync(d => d.AppUserId == doctorAppUserId);
            if (doctor == null)
                return NotFound(CreateErrorResponse404NotFound("Doctor not found."));

            var doctorIdeaExists = await _unitOfWork.Repository<DoctorProjectIdea>().FindAsync(p => p.Name == model.Name);
            if (doctorIdeaExists != null)
                return BadRequest(CreateErrorResponse400BadRequest("Project idea with this name already exists, Please enter other project name."));

            var ActiveAcademicYearAppointment = await _unitOfWork.Repository<AcademicAppointment>()
                                                                        .FindAsync(a => a.Status == StatusType.Active.ToString());
            if (ActiveAcademicYearAppointment == null)
                return NotFound(CreateErrorResponse404NotFound("No active academic appointment found, You cannot submit an idea now."));

            var newDoctorProjectIdea = new DoctorProjectIdea
            {
                Name = model.Name,
                Description = model.Description,
                DoctorId = doctor.Id,
                AcademicAppointmentId = ActiveAcademicYearAppointment.Id,
            };
            
            await _unitOfWork.Repository<DoctorProjectIdea>().AddAsync(newDoctorProjectIdea);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(200, "Project submitted successfully, Please wait until admin review this idea.", new { IsSuccess = true }));
        }

        // Finished / Reviewed / Tested / Edited / D
        [HttpPost("SubmitTeamProjectIdea")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> SubmitTeamProjectIdea([FromBody] ProjectIdeaFromTeamDto model)
        {
            if (model is null)
                return BadRequest(CreateErrorResponse400BadRequest("Invalid input data."));
            if (string.IsNullOrEmpty(model.Name) || string.IsNullOrEmpty(model.Description) || model.TeamId <= 0)
                return BadRequest(CreateErrorResponse400BadRequest("Invalid input data, Please enter valid project idea name, description and team id."));

            var leaderAppUserId = User.FindFirst("UserId")?.Value;
            if (leaderAppUserId == null)
                return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));

            var leader = await _unitOfWork.Repository<Student>().FindAsync(l => l.AppUserId == leaderAppUserId);
            if (leader == null)
                return NotFound(CreateErrorResponse404NotFound("Leader not found."));

            var team = await _unitOfWork.Repository<Team>().FindAsync(t => t.Id == model.TeamId);
            if (team == null)
                return NotFound(CreateErrorResponse404NotFound("Team not found."));

            if (!(leader.InTeam && leader.TeamId == model.TeamId) && team.LeaderId != leader.Id)
                return BadRequest(CreateErrorResponse400BadRequest("You're not in this team or not the team leader."));

            if(team.HasProject)
                return BadRequest(CreateErrorResponse400BadRequest("Your team already has a project idea."));

            var teamIdeaExists = await _unitOfWork.Repository<TeamProjectIdea>().FindAsync(p => p.Name == model.Name);
            if (teamIdeaExists != null)
                return BadRequest(CreateErrorResponse400BadRequest("Project idea with this name already exists, Please enter other project idea name."));

            var activeAcademicYearAppointment = await _unitOfWork.Repository<AcademicAppointment>()
                                                                        .FindAsync(a => a.Status == StatusType.Active.ToString());
            if (activeAcademicYearAppointment == null)
                return NotFound(CreateErrorResponse404NotFound("No active academic appointment found, You cannot submit an idea now."));

            var newTeamProjectIdea = new TeamProjectIdea
            {
                Name = model.Name,
                Description = model.Description,
                TeamId = model.TeamId,
                LeaderId = leader.Id,
                AcademicAppointmentId = activeAcademicYearAppointment.Id,
            };

            await _unitOfWork.Repository<TeamProjectIdea>().AddAsync(newTeamProjectIdea);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(200, "Project submitted successfully, Please wait until admin review this idea.", new { IsSuccess = true }));
        }

        // Finished / Reviewed / Tested / Edited / D
        [HttpGet("PendingDoctorProjectIdeas")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllPendingDoctorProjectIdeas()
        {
            var pendingDoctorProjects = await  _projectRepository.GetDoctorProjectIdeasByStatusAndDoctorIdIfNeededAsync("Pending");
            if (pendingDoctorProjects == null || !pendingDoctorProjects.Any())
                return NotFound(new ApiResponse(404, "No pending project ideas found.", new {IsSuccess = false}));

            return Ok(new ApiResponse(200, "Pending project ideas retrieved successfully.", new { IsSuccess = true, pendingDoctorProjects } ));
        }

        // Finished / Reviewed / Tested / Edited / D
        [HttpGet("PendingTeamProjectIdeas")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllPendingTeamProjectIdeas()
        {
            var pendingTeamProjects = await _projectRepository.GetTeamProjectIdeasByStatusAsync(StatusType.Pending.ToString());
            if (pendingTeamProjects == null || !pendingTeamProjects.Any())
                return NotFound(CreateErrorResponse404NotFound("No pending project ideas found."));

            return Ok(new ApiResponse(200, "Pending project ideas retrieved successfully.", new { IsSuccess = true, pendingTeamProjects }));
        }

        // Finished / Reviewed / Tested / Edited / D
        [HttpPut("ReviewDoctorProjectIdea")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReviewDoctorProjectIdea([FromBody] ReviewDoctorProjectIdeaDto model)
        {
            if (model == null)
                return BadRequest(new ApiResponse(400, "Invalid input data.", new { IsSuccess = false }));
            if (string.IsNullOrEmpty(model.NewStatus) || model.ProjectId <= 0)
                return BadRequest(new ApiResponse(400, "Invalid input data, Please enter valid project id and status.", new { IsSuccess = false }));

            var adminAppUserId = User.FindFirst("UserId")?.Value;
            if (adminAppUserId == null)
                return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));

            var admin = await _unitOfWork.Repository<Admin>().FindAsync(a => a.AppUserId == adminAppUserId);
            if (admin == null)
                return NotFound(new ApiResponse(404, "Admin not found.", new { IsSuccess = false }));

            if (model.NewStatus != StatusType.Accepted.ToString() && model.NewStatus != StatusType.Rejected.ToString())
                return BadRequest(new ApiResponse(400, "Invalid status value, Use 'Accepted' or 'Rejected'.", new { IsSuccess = false }));
            
            var doctorProjectIdea = await _unitOfWork.Repository<DoctorProjectIdea>().FindAsync(p => p.Id == model.ProjectId);
            if (doctorProjectIdea == null)
                return NotFound(new ApiResponse(404, "Project not found.", new { IsSuccess = false }));

            doctorProjectIdea.Status = model.NewStatus == StatusType.Accepted.ToString() ? StatusType.Accepted.ToString() : StatusType.Rejected.ToString();
            _unitOfWork.Repository<DoctorProjectIdea>().Update(doctorProjectIdea);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(200, $"Status of this project updated to '{model.NewStatus.ToLower()}' successfully!", new { IsSuccess = true }));
        }

        // Finished / Reviewed / Tested / Edited / D
        [HttpPut("ReviewTeamProjectIdea")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReviewTeamProjectIdea([FromBody] ReviewTeamProjectIdeaDto model)
        {
            if (model == null)
                return BadRequest(new ApiResponse(400, "Invalid input data.", new { IsSuccess = false }));
            if (string.IsNullOrEmpty(model.NewStatus) || model.ProjectId <= 0 || model.SupervisorId <= 0)
                return BadRequest(new ApiResponse(400, "Invalid input data, Please enter valid project id, status and supervisor id.", new { IsSuccess = false }));

            var adminAppUserId = User.FindFirst("UserId")?.Value;
            if (adminAppUserId == null)
                return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));

            var admin = await _unitOfWork.Repository<Admin>().FindAsync(a => a.AppUserId == adminAppUserId);
            if (admin == null)
                return NotFound(new ApiResponse(404, "Admin not found.", new { IsSuccess = false }));

            if (model.NewStatus != StatusType.Accepted.ToString() && model.NewStatus != StatusType.Rejected.ToString())
                return BadRequest(new ApiResponse(400, "Invalid status value, Use 'Accepted' or 'Rejected'.", new { IsSuccess = false }));

            var teamProjectIdea = await _unitOfWork.Repository<TeamProjectIdea>().FindAsync(p => p.Id == model.ProjectId);
            if (teamProjectIdea == null)
                return NotFound(new ApiResponse(404, "Project not found.", new { IsSuccess = false }));

            var team = await _unitOfWork.Repository<Team>().FindAsync(t => t.Id == teamProjectIdea.TeamId);
            if (team == null)
                return NotFound(new ApiResponse(404, "Team not found.", new { IsSuccess = true }));

            var activeAcademicYearAppointment = await _unitOfWork.Repository<AcademicAppointment>()
                                                                        .FindAsync(a => a.Status == StatusType.Active.ToString());
            if (activeAcademicYearAppointment == null)
                return NotFound(new ApiResponse(404, "No active academic appointment found.", new { IsSuccess = false }));

            teamProjectIdea.Status = model.NewStatus == StatusType.Accepted.ToString() ? StatusType.Accepted.ToString() : StatusType.Rejected.ToString();
            _unitOfWork.Repository<TeamProjectIdea>().Update(teamProjectIdea);

            if (model.NewStatus == StatusType.Accepted.ToString())
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
                PostedBy = "Team",
                AcademicAppointmentId = activeAcademicYearAppointment.Id
            };
            await _unitOfWork.Repository<FinalProjectIdea>().AddAsync(finalProjectIdea);
            await _unitOfWork.CompleteAsync();

            var pendingTeamProjectIdeas = _dbContext.TeamProjectIdeas
                                                    .Where(p => p.LeaderId == team.LeaderId &&
                                                                p.Status == StatusType.Pending.ToString() &&
                                                                p.TeamId == team.Id && 
                                                                p.Id != teamProjectIdea.Id &&
                                                                p.AcademicAppointmentId == activeAcademicYearAppointment.Id)
                                                    .ToList();
            foreach (var p in pendingTeamProjectIdeas)
                p.Status = StatusType.Rejected.ToString();

            await _unitOfWork.CompleteAsync();

            var pendingTeamRequestsForDoctorProjectIdeas = await _dbContext.TeamsRequestDoctorProjectIdeas
                                                                           .Where(tr => tr.TeamId == team.Id &&
                                                                                        tr.AcademicAppointmentId == activeAcademicYearAppointment.Id &&
                                                                                        tr.LeaderId == team.LeaderId &&
                                                                                        tr.Status == StatusType.Pending.ToString())
                                                                           .ToListAsync();
            foreach (var tr in pendingTeamRequestsForDoctorProjectIdeas)
                tr.Status = StatusType.Rejected.ToString();

            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(200, $"Status Of this project Updated To '{model.NewStatus.ToLower()}' Successfully!", new { IsSuccess = true }));
        }

        // Finished / Reviewed / Tested / Edited / D
        [HttpGet("AcceptedDoctorProjectIdeas")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetAllAcceptedDoctorProjectIdeasForViewStudents()
        {
            var acceptedDoctorProjects = await _projectRepository.GetDoctorProjectIdeasByStatusAndDoctorIdIfNeededAsync("Accepted");
            if (acceptedDoctorProjects == null || !acceptedDoctorProjects.Any())
                return NotFound(new ApiResponse(404, "No accepted project ideas found.", new { IsSuccess = false }));

            return Ok(new ApiResponse(200, "Accepted project ideas retrieved successfully", new { IsSuccess = true , acceptedDoctorProjects } ));
        }

        // Finished / Reviewed / Tested / Edited / D
        [HttpGet("AcceptedProjectIdeasForDoctor/{doctorId}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetAllAcceptedProjectIdeasForDoctor(int doctorId)
        {
            if(doctorId <= 0)
                return BadRequest(new ApiResponse(400, "Invalid input data.", new { IsSuccess = false }));

            var doctor = await _unitOfWork.Repository<Doctor>().FindAsync(d => d.Id == doctorId);
            if (doctor == null)
                return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));

            var acceptedProjectIdeasForDoctor = await _projectRepository.GetDoctorProjectIdeasByStatusAndDoctorIdIfNeededAsync("Accepted", doctorId);

            if (acceptedProjectIdeasForDoctor == null || !acceptedProjectIdeasForDoctor.Any())
                return NotFound(new ApiResponse(404, "No accepted project ideas found for his doctor.", new { IsSuccess = false }));

            return Ok(new ApiResponse(200, "Accepted project ideas retrieved successfully for his doctor.", new { IsSuccess = true, acceptedProjectIdeasForDoctor }));
        }

        // Finished / Reviewed / Tested / Edited / D
        [HttpGet("AcceptedTeamProjectIdeas")]
        [Authorize(Roles = "Admin, Student, Doctor")]
        public async Task<IActionResult> GetAllAcceptedTeamProjectIdeas()
        {
            var acceptedTeamProjects = await _projectRepository.GetTeamProjectIdeasByStatusAsync("Accepted");
            if (acceptedTeamProjects == null || !acceptedTeamProjects.Any())
                return NotFound(new ApiResponse(404, "No Accepted Project Ideas Found.", new { IsSuccess = false }));
            
            return Ok(new ApiResponse(200, "Accepted Project Ideas Retrieved Successfully", new { IsSuccess = true, acceptedTeamProjects }));
        }

        // Finished / Reviewed / Tested / Edited / D
        [HttpPost("RequestDoctorProjectIdea")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> RequestProjectIdeaFromDrByTeam([FromBody] RequestProjectIdeaFromDrByTeamDto model)
        {
            if (model is null)
                return BadRequest(new ApiResponse(400, "Invalid input data.", new { IsSuccess = false }));
            if (model.TeamId <= 0 || model.TeamLeaderId <= 0  || model.ProjectId <= 0 || model.DoctorId <= 0)
                return BadRequest(new ApiResponse(400, "Invalid input data, Please enter valid team id, project id and doctor id.", new { IsSuccess = false }));

            var leaderAppUserId = User.FindFirst("UserId")?.Value;
            if (leaderAppUserId == null)
                return Unauthorized(new ApiResponse(401, "Unauthorized access.", new { IsSuccess = false }));
            var leader = await _unitOfWork.Repository<Student>().FindAsync(l => l.AppUserId == leaderAppUserId);
            if (leader == null)
                return NotFound(new ApiResponse(404, "Student not found.", new { IsSuccess = false }));

            var academicAppointment = await _unitOfWork.Repository<AcademicAppointment>()
                                                       .FindAsync(a => a.Status == StatusType.Active.ToString());
            if (academicAppointment == null)
                return NotFound(new ApiResponse(404, "No active academic appointment found.", new { IsSuccess = false }));

            var teamExists = await _unitOfWork.Repository<Team>().FindAsync(t => t.Id == model.TeamId);
            if (teamExists == null)
                return NotFound(new ApiResponse(404, "Team not found.", new { IsSuccess = false }));

            var projectExists = await _unitOfWork.Repository<DoctorProjectIdea>()
                                                 .FindAsync(p => p.Id == model.ProjectId &&
                                                                 p.Status == StatusType.Accepted.ToString() &&
                                                                 p.AcademicAppointmentId == academicAppointment.Id);
            if (projectExists == null)
                return NotFound(new ApiResponse(404, "Project not found or not accepted.", new { IsSuccess = false }));

            if(leader.TeamId != model.TeamId || leader.LeaderOfTeamId == null || leader.TeamId == null || leader.InTeam == false)
                return BadRequest(new ApiResponse(400, "You're not in this team.", new { IsSuccess = false }));

            if(leader.LeaderOfTeam?.HasProject == true)
                return BadRequest(new ApiResponse(400, "Your team already has a project.", new { IsSuccess = false }));

            var existingTeamRequest = await _dbContext.TeamsRequestDoctorProjectIdeas
                                                      .FirstOrDefaultAsync(tr => tr.TeamId == teamExists.Id &&
                                                                                 tr.DoctorProjectIdeaId == projectExists.Id &&
                                                                                 tr.LeaderId == leader.Id &&
                                                                                 tr.DoctorId == projectExists.DoctorId &&
                                                                                 tr.Status == StatusType.Pending.ToString() &&
                                                                                 tr.AcademicAppointmentId == academicAppointment.Id);
            if (existingTeamRequest != null)
                return BadRequest(new ApiResponse(400, "A request exists before that for this project idea.", new { IsSuccess = false }));

            var projectRequest = new TeamRequestDoctorProjectIdea
            {
                DoctorProjectIdeaId = model.ProjectId,
                TeamId = teamExists.Id,
                LeaderId = leader.Id,
                DoctorId = model.DoctorId,
                AcademicAppointmentId = academicAppointment?.Id,
            };

            await _unitOfWork.Repository<TeamRequestDoctorProjectIdea>().AddAsync(projectRequest);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(200, "Request of this project successfully submitted!", new { IsSuccess = true }));
        }

        // Finished / Reviewed / Tested / Edited / D
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
                return NotFound(new ApiResponse(404, "No team requests for your project ideas.", new { IsSuccess = false }));

            return Ok(new ApiResponse(200, "Pending team requests for your project ideas retrieved successfully.", new { IsSuccess = true, pendingTeamRequests }));
        }

        // Finished / Reviewed / Tested / Edited / D
        [HttpPut("ReviewTeamProjectRequest")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> DoctorReviewTeamProjectIdeaRequest([FromBody] ReviewTeamProjectRequestDto model)
        {
            if (model is null)
                return BadRequest(new ApiResponse(400, "Invalid input data.", new { IsSuccess = false }));
            if (model.RequestId <= 0 || model.DoctorId <= 0 || string.IsNullOrEmpty(model.NewStatus))
                return BadRequest(new ApiResponse(400, "Invalid input data, Please enter valid request id, doctor id and status.", new { IsSuccess = false }));
            if (model.NewStatus != StatusType.Accepted.ToString() && model.NewStatus != StatusType.Rejected.ToString())
                return BadRequest(new ApiResponse(400, "Invalid Status Value. Use 'Accepted' or 'Rejected'.", new { IsSuccess = false }));

            var activeAcademicYearAppointment = await _unitOfWork.Repository<AcademicAppointment>()
                                                                        .FindAsync(a => a.Status == StatusType.Active.ToString());
            if (activeAcademicYearAppointment == null)
                return NotFound(new ApiResponse(404, "No active academic appointment found.", new { IsSuccess = false }));

            var projectRequest = await _dbContext.TeamsRequestDoctorProjectIdeas
                                                 .Include(r => r.DoctorProjectIdea)
                                                 .FirstOrDefaultAsync(r => r.Id == model.RequestId && r.AcademicAppointmentId == activeAcademicYearAppointment.Id);
            if (projectRequest == null)
                return NotFound(new ApiResponse(404, "Request not found.", new { IsSuccess = false }));

            var team = await _unitOfWork.Repository<Team>().FindAsync(t => t.Id == projectRequest.TeamId);
            if (team == null)
                return NotFound(new ApiResponse(404, "Team not found.", new { IsSuccess = false }));

            if (team.HasProject && model.NewStatus == StatusType.Accepted.ToString())
                return BadRequest(new ApiResponse(400, "This team already has a project.", new { IsSuccess = false }));

            var doctor = await _unitOfWork.Repository<Doctor>().FindAsync(d => d.Id == model.DoctorId && d.Id == projectRequest.DoctorId);
            if (doctor == null)
                return NotFound(new ApiResponse(404, "Doctor not found.", new { IsSuccess = false }));

            var leader = await _unitOfWork.Repository<Student>().FindAsync(s => s.Id == projectRequest.LeaderId);
            if (leader == null)
                return NotFound(new ApiResponse(404, "Leader not found.", new { IsSuccess = false }));

            projectRequest.Status = model.NewStatus == StatusType.Accepted.ToString() ? StatusType.Accepted.ToString() : StatusType.Rejected.ToString();
            _unitOfWork.Repository<TeamRequestDoctorProjectIdea>().Update(projectRequest);

            if (model.NewStatus == StatusType.Accepted.ToString())
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
                    PostedBy = "Doctor",
                    AcademicAppointmentId = activeAcademicYearAppointment?.Id
                };
                await _unitOfWork.Repository<FinalProjectIdea>().AddAsync(finalProject);
                await _unitOfWork.CompleteAsync();

                if (projectRequest?.DoctorProjectIdea == null)
                {
                    return NotFound(new ApiResponse(404, "Doctor project idea not found.", new { IsSuccess = false }));
                }

                projectRequest.DoctorProjectIdea.Taken = true;
                _unitOfWork.Repository<DoctorProjectIdea>().Update(projectRequest.DoctorProjectIdea);
                await _unitOfWork.CompleteAsync();

                var pendingRequests = await _unitOfWork.Repository<TeamRequestDoctorProjectIdea>()
                    .FindAllAsync(r => r.LeaderId == leader.Id && 
                                       r.Status == StatusType.Pending.ToString() &&
                                       r.TeamId == team.Id &&
                                       r.AcademicAppointmentId == activeAcademicYearAppointment.Id &&
                                       r.Id != projectRequest.Id);

                foreach (var r in pendingRequests)
                {
                    r.Status = StatusType.Rejected.ToString();
                    _unitOfWork.Repository<TeamRequestDoctorProjectIdea>().Update(r);
                }
            }

            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(200, "Project Request approved and other requests rejected successfully!", new { IsSuccess = true }));
        }

        // Finished / Reviewed / Tested / Edited / D
        [HttpGet("FinalProjectIdeas")]
        [Authorize(Roles = "Admin, Student, Doctor")]
        public async Task<IActionResult> GetAllFinalProjectIdeas()
        {
            var finalProjectIdeas = await _projectRepository.GetAllFinalProjectIdeas();
            if (finalProjectIdeas == null || !finalProjectIdeas.Any())
                return NotFound(new ApiResponse(404, "No final project ideas found.", new { IsSuccess = false }));

            return Ok(new ApiResponse(200, "Final project ideas retrieved successfully.", new { IsSuccess = true, finalProjectIdeas }));
        }

        // Finished / Reviewed / Tested / Edited / D
        [HttpGet("FinalProjectIdea/{teamId}")]
        [Authorize(Roles = "Admin, Student, Doctor")]
        public async Task<IActionResult> GetFinalProjectIdeaById(int teamId)
        {
            if (teamId <= 0)
                return BadRequest(CreateErrorResponse400BadRequest("TeamId must be positive number."));

            var academicAppointment = await _unitOfWork.Repository<AcademicAppointment>().FindAsync(a => a.Status == StatusType.Active.ToString());
            if (academicAppointment == null)
                return NotFound(CreateErrorResponse404NotFound("No active academic appointment found."));

            var finalProjectIdea = await _dbContext.FinalProjectIdeas.Include(f => f.Team)
                                                                     .Include(f => f.Supervisor)
                                                                     .FirstOrDefaultAsync(f => f.TeamId == teamId && 
                                                                                               f.AcademicAppointmentId == academicAppointment.Id);
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
                PostedBy = finalProjectIdea?.PostedBy,
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
