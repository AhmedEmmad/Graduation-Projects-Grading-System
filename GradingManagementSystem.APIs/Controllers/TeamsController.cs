using GradingManagementSystem.Core.DTOs;
using GradingManagementSystem.Core.CustomResponses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GradingManagementSystem.Repository.Data.DbContexts;
using GradingManagementSystem.Core.Entities;
using GradingManagementSystem.Core;
using GradingManagementSystem.Core.Repositories.Contact;

namespace GradingManagementSystem.APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamsController : ControllerBase
    {
        private readonly GradingManagementSystemDbContext _dbContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITeamRepository _teamRepository;

        public TeamsController(GradingManagementSystemDbContext dbContext, IUnitOfWork unitOfWork, ITeamRepository teamRepository)
        {
            _dbContext = dbContext;
            _unitOfWork = unitOfWork;
            _teamRepository = teamRepository;
        }

        // Finished / Tested
        [HttpGet("TeamWithMembers/{teamId}")]
        [Authorize(Roles = "Student, Doctor")]
        public async Task<IActionResult> GetTeamWithMembersById(int teamId)
        {
            if (teamId <= 0)
                return BadRequest(new ApiResponse(400, "TeamId must be positive number.", new { IsSuccess = false }));

            var team = await _dbContext.Teams.Include(T => T.Students).FirstOrDefaultAsync(T => T.Id == teamId);
            if (team == null)
                return NotFound(new ApiResponse(404, "Team not found.", new { IsSuccess = false }));

            var teamMembers = await _dbContext.Students.Include(S => S.AppUser).Where(S => S.TeamId == teamId).ToListAsync();
            if (teamMembers == null || !teamMembers.Any())
                return NotFound(new ApiResponse(404, "No members found for this team.", new { IsSuccess = false }));

            var result = new TeamWithMembersDto
            {
                Id = team.Id,
                Name = team.Name,
                HasProject = team.HasProject,
                LeaderId = team.LeaderId,
                SupervisorId = team.SupervisorId,
                Members = team.Students.Select(s => new TeamMemberDto
                {
                    Id = s.Id,
                    FullName = s.FullName,
                    Email = s.Email,
                    Specialty = s.Specialty,
                    InTeam = s.InTeam,
                    ProfilePicture = s.AppUser.ProfilePicture
                }).ToList()
            };

            return Ok(new ApiResponse(200, "Team found.", new { IsSuccess = true, Team = result }));
        }

        // Finished / Tested
        [HttpPost("CreateTeam/{teamname}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> CreateTeam(string teamname)
        {
            var studentAppUserId = User.FindFirst("UserId")?.Value;
            if (studentAppUserId == null)
                return NotFound(new ApiResponse(404, "Student not found.", new { IsSuccess = false }));

            var student = await _unitOfWork.Repository<Student>().FindAsync(S => S.AppUserId == studentAppUserId);
            if (student == null)
                return NotFound(new ApiResponse(404, "Student not found.", new { IsSuccess = false }));

            if (student.InTeam == true)
                return BadRequest(new ApiResponse(400, "Student is already in a team.", new { IsSuccess = false }));

            if (student.LeaderOfTeamId != null && student.TeamId != null)
                return BadRequest(new ApiResponse(400, "Student is already a leader of a team or in a team.", new { IsSuccess = false }));

            var teamExists = await _unitOfWork.Repository<Team>().FindAsync(t => t.Name == teamname);
            if (teamExists != null)
                return BadRequest(new ApiResponse(400, "Team name already exists.", new { IsSuccess = false }));

            if(teamExists?.LeaderId == student.Id)
                return BadRequest(new ApiResponse(400, "You are already a leader of this team.", new { IsSuccess = false }));

            var teamCreated = new Team
            {
                Name = teamname,
                LeaderId = student.Id,
            };

            try
            {
                await _unitOfWork.Repository<Team>().AddAsync(teamCreated);
                await _unitOfWork.CompleteAsync();

                student.InTeam = true;
                student.TeamId = teamCreated?.Id;
                student.LeaderOfTeamId = teamCreated?.Id;
                _unitOfWork.Repository<Student>().Update(student);
                await _unitOfWork.CompleteAsync();
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? "No inner exception";
                return StatusCode(500, new ApiResponse(500, "Failed to create team.", new { IsSuccess = false }));
            }

            return Ok(new ApiResponse(200, $"Team {teamCreated.Name} is created successfully, You're leader of this team.", new { IsSuccess = true }));
        }

        // Finished / Tested
        [HttpGet("DoctorTeams")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetTeamsForDoctor()
        {
            var doctorAppUserId = User.FindFirst("UserId")?.Value;

            if (doctorAppUserId == null)
                return BadRequest(new ApiResponse(400, "DoctorId is required.", new { IsSuccess = false }));

            var doctor = await _unitOfWork.Repository<Doctor>().FindAsync(d => d.AppUserId == doctorAppUserId);

            if (doctor == null)
                return NotFound(new ApiResponse(404, "Doctor not found.", new { IsSuccess = false }));

            var doctorId = doctor.Id;
            var TeamsList = await _teamRepository.GetAllTeamsForDoctor(doctorId);

            if (TeamsList == null || !TeamsList.Any())
                return NotFound(new ApiResponse(404, "No teams found for his doctor.", new { IsSuccess = false }));

            return Ok(new ApiResponse(200, "Teams retrieved successfully.", new { IsSuccess = true, Teams = TeamsList }));
        }

        // Finished / Tested
        [HttpPost("InviteStudent")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> SendInvitation([FromBody] InviteStudentDto model)
        {
            var currentStudent = await _unitOfWork.Repository<Student>().FindAsync(s => s.Id == model.LeaderId);
            if (currentStudent == null)
                return NotFound(new ApiResponse(404, "Leader not found.", new { IsSuccess = false }));

            if (currentStudent.TeamId == null && currentStudent.LeaderOfTeamId != model.TeamId)
                return BadRequest(new ApiResponse(400, "Only team leaders can send invitations.", new { IsSuccess = false }));

            var student = await _dbContext.Students.FirstOrDefaultAsync(s => s.Id == model.StudentId && s.InTeam == false);

            if (student == null)
                return BadRequest(new ApiResponse(400, "Student not found or already in a team.", new { IsSuccess = false }));

            var team = await _dbContext.Teams.FindAsync(model.TeamId);
            if (team == null)
                return NotFound(new ApiResponse(404, "Team not found.", new { IsSuccess = false }));

            var existingInvitation = await _dbContext.Invitations.FirstOrDefaultAsync(i => i.TeamId == model.TeamId && i.StudentId == model.StudentId && i.LeaderId == model.LeaderId && i.Status == "Pending");

            if (existingInvitation != null)
                return BadRequest(new ApiResponse(400, "Invitation already exists.", new { IsSuccess = false }));

            var invitation = new Invitation
            {
                TeamId = model.TeamId,
                StudentId = model.StudentId,
                LeaderId = model.LeaderId,
            };

            await _dbContext.Invitations.AddAsync(invitation);
            await _dbContext.SaveChangesAsync();

            return Ok(new ApiResponse(200, "Invitation sent successfully.", new
            {
                IsSuccess = true,
                InvitationData = new
                {
                    InvitationId = invitation.Id,
                    TeamId = team.Id,
                    TeamName = team.Name,
                    StudentId = student.Id,
                    StudentName = student.FullName,
                    LeaderId = currentStudent.Id,
                    LeaderName = currentStudent.FullName,
                    InvitationSentDate = invitation.SentDate,
                    InvitationStatus = invitation.Status
                }
            }));
        }

        // Finished / Tested
        [HttpGet("TeamInvitations")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetTeamInvitations()
        {
            var studentId = User.FindFirst("UserId")?.Value;
            if (studentId == null)
                return NotFound(new ApiResponse(404, "Student not found.", new { IsSuccess = false }));

            var student = await _dbContext.Students.FirstOrDefaultAsync(s => s.AppUserId == studentId);
            if (student == null)
                return NotFound(new ApiResponse(404, "Student not found.", new { IsSuccess = false }));


            var invitations = await _dbContext.Invitations
                                    .Include(i => i.Team)
                                    .ThenInclude(t => t.Students)
                                    .ThenInclude(s => s.AppUser)
                                    .Include(i => i.Leader)
                                    .ThenInclude(l => l.AppUser)
                                    .Where(i => i.StudentId == student.Id && i.Status == StatusType.Pending.ToString())
                                    .ToListAsync();


            if (invitations.Count == 0 || !invitations.Any())
                return NotFound(new ApiResponse(404, "No invitations found.", new { IsSuccess = false }));

            var result = invitations.Select(i => new TeamInvitationDto
            {
                InvitationId = i.Id,
                TeamId = i.TeamId,
                TeamName = i.Team.Name,
                LeaderId = i.LeaderId,
                LeaderName = i.Leader.FullName,
                StudentId = i.StudentId,
                StudentName = i.Student.FullName,
                InvitationSentDate = i.SentDate,
                InvitationStatus = i.Status,
                TeamMembers = i.Team.Students.Select(s => new TeamMemberDto
                {
                    Id = s.Id,
                    FullName = s.FullName,
                    Email = s.Email,
                    Specialty = s.Specialty,
                    InTeam = s.InTeam,
                    ProfilePicture = s.AppUser.ProfilePicture
                }).ToList()
            });
            return Ok(new ApiResponse(200, "Team invitations retrieved successfully.", new { IsSuccess = true, Invitations = result }));
        }

        // Finished / Tested
        [HttpPut("ReviewTeamInvitation")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> ReviewTeamInvitation([FromBody] ReviewTeamInvitationDto model)
        {
            if (model.invitationId <= 0)
                return BadRequest(new ApiResponse(400, "InvitationId is required.", new { IsSuccess = false }));
            if (model.newStatus != "Accepted" && model.newStatus != "Rejected")
                return BadRequest(new ApiResponse(400, "Invalid Status Value. Use 'Accepted' or 'Rejected'.", new { IsSuccess = false }));

            var studentAppUserId = User.FindFirst("UserId")?.Value;
            if (studentAppUserId == null)
                return NotFound(new ApiResponse(404, "Student not found.", new { IsSuccess = false }));

            var student = await _unitOfWork.Repository<Student>().FindAsync(s => s.AppUserId == studentAppUserId);
            if (student == null)
                return NotFound(new ApiResponse(404, "Student not found.", new { IsSuccess = false }));

            if (student.InTeam == true)
                return BadRequest(new ApiResponse(400, "Student is already in a team.", new { IsSuccess = false }));

            var invitation = await _dbContext.Invitations.Include(i => i.Team).FirstOrDefaultAsync(i => i.Id == model.invitationId && i.StudentId == student.Id && i.Status == "Pending");
            if (invitation == null)
                return NotFound(new ApiResponse(404, "Invitation not found.", new { IsSuccess = false }));

            var leader = await _unitOfWork.Repository<Student>().FindAsync(s => s.Id == invitation.LeaderId);
            if (leader == null)
                return NotFound(new ApiResponse(404, "Leader not found.", new { IsSuccess = false }));

            invitation.Status = model.newStatus;
            invitation.RespondedDate = DateTime.Now;

            if (model.newStatus == "Accepted")
            {
                student.TeamId = invitation.TeamId;
                student.InTeam = true;
                student.LeaderOfTeamId = invitation.LeaderId;
                _unitOfWork.Repository<Student>().Update(student);

                var team = await _dbContext.Teams.Include(t => t.Students).FirstOrDefaultAsync(t => t.Id == invitation.TeamId);
                if (team != null)
                {
                    team.Students.Add(student);
                    _unitOfWork.Repository<Team>().Update(team);
                }
                var otherInvitations = await _unitOfWork.Repository<Invitation>().FindAllAsync(i => i.StudentId == student.Id && i.Id != model.invitationId && i.Status == "Pending");

                foreach (var otherInvitation in otherInvitations)
                {
                    otherInvitation.Status = "Rejected";
                    otherInvitation.RespondedDate = DateTime.Now;
                    _unitOfWork.Repository<Invitation>().Update(otherInvitation);
                }
            }
            _unitOfWork.Repository<Invitation>().Update(invitation);
            await _unitOfWork.CompleteAsync();
            return Ok(new ApiResponse(200, $"Invitation {model.newStatus.ToLower()} successfully.", new { IsSuccess = true }));
        }
    }
}
