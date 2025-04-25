using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using GradingManagementSystem.Core.Entities;
using GradingManagementSystem.Core.CustomResponses;
using GradingManagementSystem.APIs.Hubs;
using GradingManagementSystem.Core.DTOs;
using GradingManagementSystem.Core;
using Microsoft.AspNetCore.Authorization;
using GradingManagementSystem.Core.Repositories.Contact;

namespace GradingManagementSystem.APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly INotificationRepository _notificationRepository;

        public NotificationsController(IUnitOfWork unitOfWork,
                                      IHubContext<NotificationHub> hubContext,
                                      INotificationRepository notificationRepository)
        {
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
            _notificationRepository = notificationRepository;
        }

        [HttpPost("SendNotification")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendNotification([FromBody] NotificationDto model)
        {
            if (model is null)
                return BadRequest(new ApiResponse(400, "Invalid input data."));

            var adminId = User.FindFirst("UserId")?.Value;
            
            if(adminId == null)
                return BadRequest(new ApiResponse(400, "Invalid admin id."));

            var admin = await _unitOfWork.Repository<Admin>().FindAsync(A => A.AppUserId == adminId);

            if (admin == null)
                return NotFound(new ApiResponse(404, "Admin not found."));

            if(!Enum.TryParse(model.Role, out NotificationRole role))
                return BadRequest(new ApiResponse(400, "Invalid role."));

            var notification = new Notification
            {
                Title = model.Title,
                Description = model.Description,
                Role = role,
                AdminId = admin.Id
            };

            await _unitOfWork.Repository<Notification>().AddAsync(notification);
            await _unitOfWork.CompleteAsync();

            await _hubContext.Clients.Group(model.Role).SendAsync("ReceiveNotification", notification.Title, notification.Description, role.ToString());
            
            return Ok(new ApiResponse(200, "Notification sent successfully!", new
            {
                Title = notification.Title,
                Description = notification.Description,
                Role = role.ToString(),
                CreatedAt = notification.CreatedAt,
                IsRead = notification?.IsRead,
                AdminId = admin.Id,

            }));
        }

        [HttpGet("GetAllNotifications")]
        [Authorize(Roles = "Admin, Doctor, Student")]
        public async Task<IActionResult> GetAllNotifications()
        {
            var notifications = await _notificationRepository.GetNotificationsByRoleAsync("All");

            if (notifications == null || !notifications.Any())
                return NotFound(new ApiResponse(404, "No notifications found."));

            return Ok(new ApiResponse(200, "Notifications retrieved successfully.", notifications));
        }

        [HttpGet("GetStudentNotifications")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetStudentNotifications()
        {
            var studentNotifications = await _notificationRepository.GetNotificationsByRoleAsync("Students");

            if (studentNotifications == null || !studentNotifications.Any())
                return NotFound(new ApiResponse(404, "No notifications found for students."));

            return Ok(new ApiResponse(200, "Student notifications retrieved successfully.", studentNotifications));
        }

        [HttpGet("GetDoctorNotifications")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetDoctorNotifications()
        {
            var doctorNotifications = await _notificationRepository.GetNotificationsByRoleAsync("Doctors");

            if (doctorNotifications == null || !doctorNotifications.Any())
                return NotFound(new ApiResponse(404, "No notifications found for doctors."));

            return Ok(new ApiResponse(200, "Doctor notifications retrieved successfully.", doctorNotifications));
        }

        [HttpPut("MarkAsRead/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _unitOfWork.Repository<Notification>().GetByIdAsync(id);
            if (notification == null)
                return NotFound(new ApiResponse(404, "Notification not found."));

            if (notification.IsRead)
                return BadRequest(new ApiResponse(400, "Notification is already marked as read."));

            notification.IsRead = true;
            _unitOfWork.Repository<Notification>().Update(notification);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(200, "Notification marked as read successfully."));
        }

        [HttpDelete("DeleteNotification/{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var notification = await _unitOfWork.Repository<Notification>().GetByIdAsync(id);
            if (notification == null)
                return NotFound(new ApiResponse(404, "Notification not found."));

            _unitOfWork.Repository<Notification>().Delete(notification);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(200, "Notification deleted successfully."));
        }
    }
}
