using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using GradingManagementSystem.Core.Entities;
using GradingManagementSystem.Core.CustomResponses;
using GradingManagementSystem.APIs.Hubs;
using GradingManagementSystem.Core.DTOs;
using GradingManagementSystem.Core;
using Microsoft.AspNetCore.Authorization;
using GradingManagementSystem.Core.Repositories.Contact;
using System.Data;

namespace GradingManagementSystem.APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly INotificationRepository _notificationRepository;
        private static readonly string[] ValidRoles = { "Doctors", "Students", "All" };

        public NotificationsController(IUnitOfWork unitOfWork,
                                      IHubContext<NotificationHub> hubContext,
                                      INotificationRepository notificationRepository)
        {
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
            _notificationRepository = notificationRepository;
        }
        
        // Finished / Reviewed / Tested
        [HttpPost("SendNotification")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendNotification([FromBody] NotificationDto model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Title) || string.IsNullOrWhiteSpace(model.Description) || string.IsNullOrWhiteSpace(model.Role))
                return BadRequest(new ApiResponse(400, "Title, Description, And Role are required.", new { IsSuccess = false }));
            
            if (!ValidRoles.Contains(model.Role, StringComparer.OrdinalIgnoreCase))
                return BadRequest(new ApiResponse(400, "Invalid Role. Must be 'Doctors', 'Students', or 'All'.", new { IsSuccess = false }));

            var adminAppUserId = User.FindFirst("UserId")?.Value;
            if (adminAppUserId == null)
                return BadRequest(new ApiResponse(400, "Invalid admin id.", new { IsSuccess = false }));

            var admin = await _unitOfWork.Repository<Admin>().FindAsync(A => A.AppUserId == adminAppUserId);
            if (admin == null)
                return NotFound(new ApiResponse(404, "Admin not found.", new { IsSuccess = false }));
            
            var newNotification = new Notification
            {
                Title = model?.Title,
                Description = model?.Description,
                Role = model?.Role == NotificationRole.All.ToString() ? NotificationRole.All.ToString() :
                       model?.Role == NotificationRole.Doctors.ToString() ? NotificationRole.Doctors.ToString() 
                       : NotificationRole.Students.ToString(),
                AdminId = admin?.Id
            };

            await _unitOfWork.Repository<Notification>().AddAsync(newNotification);
            await _unitOfWork.CompleteAsync();

            var notificationDto = new NotificationResponseDto
            {
                Id = newNotification.Id,
                Title = newNotification.Title,
                Description = newNotification.Description,
                Role = newNotification.Role,
                IsRead = newNotification.IsRead,
                SentAt = newNotification.SentAt
            };

            await _hubContext.Clients.Group(newNotification.Role).SendAsync("ReceiveNotification", notificationDto);

            return Ok(new ApiResponse(200, "Notification sent successfully!", new { IsSuccess = true }));
        }
        
        // Finished / Reviewed / Tested
        [HttpGet("All")]
        [Authorize(Roles = "Admin, Doctor, Student")]
        public async Task<IActionResult> GetAllNotifications()
        {
            var notifications = await _notificationRepository.GetNotificationsByRoleAsync("All");
            if (notifications == null || !notifications.Any())
                return NotFound(new ApiResponse(404, "No notifications found.", new { IsSuccess = false }));

            return Ok(new ApiResponse(200, "Notifications retrieved successfully.", new { IsSuccess = true, notifications }));
        }

        // Finished / Reviewed / Tested
        [HttpGet("StudentNotifications")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetStudentNotifications()
        {
            var studentNotifications = await _notificationRepository.GetNotificationsByRoleAsync(NotificationRole.Students.ToString());
            if (studentNotifications == null || !studentNotifications.Any())
                return NotFound(new ApiResponse(404, "No notifications found for students.", new { IsSuccess = false }));

            return Ok(new ApiResponse(200, "Student notifications retrieved successfully.", new { IsSuccess = true, studentNotifications }));
        }

        // Finished / Reviewed / Tested
        [HttpGet("DoctorNotifications")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetDoctorNotifications()
        {
            var doctorNotifications = await _notificationRepository.GetNotificationsByRoleAsync(NotificationRole.Doctors.ToString());
            if (doctorNotifications == null || !doctorNotifications.Any())
                return NotFound(new ApiResponse(404, "No notifications found for doctors.", new { IsSuccess = false }));

            return Ok(new ApiResponse(200, "Doctor notifications retrieved successfully.", new { IsSuccess = true, doctorNotifications }));
        }

        // Finished / Reviewed / Tested
        [HttpPut("MarkAsRead/{notificationId}")]
        [Authorize(Roles = "Doctor, Student")]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            var notification = await _unitOfWork.Repository<Notification>().GetByIdAsync(notificationId);
            if (notification == null)
                return NotFound(new ApiResponse(404, "Notification not found.", new { IsSuccess = false }));

            if (notification.IsRead)
                return BadRequest(new ApiResponse(400, "Notification is already marked as read.", new { IsSuccess = false }));

            notification.IsRead = true;
            _unitOfWork.Repository<Notification>().Update(notification);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(200, "Notification marked as read successfully.", new { IsSuccess = true }));
        }

        // Finished / Reviewed / Tested
        [HttpDelete("Delete/{notificationId}")]
        [Authorize(Roles = "Doctor, Student")]
        public async Task<IActionResult> DeleteNotification(int notificationId)
        {
            var notification = await _unitOfWork.Repository<Notification>().GetByIdAsync(notificationId);
            if (notification == null)
                return NotFound(new ApiResponse(404, "Notification not found.", new { IsSuccess = false }));

            _unitOfWork.Repository<Notification>().Delete(notification);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(200, "Notification deleted successfully.", new { IsSuccess = true }));
        }
    }
}
