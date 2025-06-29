﻿using Microsoft.AspNetCore.Mvc;
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

        // Finished / Reviewed / Tested / Edited
        [HttpPost("SendNotification")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendNotification([FromBody] NotificationDto model)
        {
            // Log the incoming notification data
            Console.WriteLine("Notification received via API");
            Console.WriteLine($"Title: {model?.Title}, Description: {model?.Description}, Role: {model?.Role}");

            if (model == null || string.IsNullOrWhiteSpace(model.Title) || string.IsNullOrWhiteSpace(model.Description) || string.IsNullOrWhiteSpace(model.Role))
            {
                Console.WriteLine("Invalid data received: Title, Description, and Role are required.");

                return BadRequest(new ApiResponse(400, "Title, Description, And Role are required.", new { IsSuccess = false }));
            }

            // Validation for valid roles
            if (!ValidRoles.Contains(model.Role, StringComparer.OrdinalIgnoreCase))
            {
                Console.WriteLine($"Invalid role: {model.Role}. Must be 'Doctors', 'Students', or 'All'.");
                return BadRequest(new ApiResponse(400, "Invalid Role. Must be 'Doctors', 'Students', or 'All'.", new { IsSuccess = false }));
            }

            // Retrieve admin information
            var adminAppUserId = User.FindFirst("UserId")?.Value;
            if (adminAppUserId == null)
            {
                Console.WriteLine("Admin ID not found in the request.");
                return BadRequest(new ApiResponse(400, "Invalid admin id.", new { IsSuccess = false }));
            }

            var admin = await _unitOfWork.Repository<Admin>().FindAsync(A => A.AppUserId == adminAppUserId);
            if (admin == null)
            {
                Console.WriteLine($"Admin not found with ID: {adminAppUserId}");
                return NotFound(new ApiResponse(404, "Admin not found.", new { IsSuccess = false }));
            }

            var activeAppointment = await _unitOfWork.Repository<AcademicAppointment>().FindAsync(a => a.Status == StatusType.Active.ToString());
            if (activeAppointment == null)
            {
                Console.WriteLine("No active academic appointment found.");
                return NotFound(new ApiResponse(404, "No active academic appointment found.", new { IsSuccess = false }));
            }

            // Create new notification
            var newNotification = new Notification
            {
                Title = model?.Title,
                Description = model?.Description,
                Role = model?.Role == NotificationRole.All.ToString() ? NotificationRole.All.ToString() :
                       model?.Role == NotificationRole.Doctors.ToString() ? NotificationRole.Doctors.ToString()
                       : NotificationRole.Students.ToString(),
                AdminId = admin?.Id,
                AcademicAppointmentId = activeAppointment?.Id,
            };

            // Save notification in the database
            await _unitOfWork.Repository<Notification>().AddAsync(newNotification);
            await _unitOfWork.CompleteAsync();

            // Map to NotificationResponseDto for sending
            var notificationDto = new NotificationResponseDto
            {
                Id = newNotification.Id,
                Title = newNotification.Title,
                Description = newNotification.Description,
                Role = newNotification.Role,
                IsReadFromAdmin = newNotification.IsReadFromAdmin,
                IsReadFromDoctor = newNotification.IsReadFromDoctor,
                IsReadFromStudent = newNotification.IsReadFromStudent,
                SentAt = newNotification.SentAt
            };

            // Log the sending of notification
            Console.WriteLine($"Sending notification to group: {newNotification.Role}");
            await _hubContext.Clients.Group(newNotification.Role).SendAsync("ReceiveNotification", notificationDto);

            return Ok(new ApiResponse(200, "Notification sent successfully!", new { IsSuccess = true }));
        }

        // Finished / Reviewed / Tested / Edited
        [HttpGet("All")]
        [Authorize(Roles = "Admin, Doctor, Student")]
        public async Task<IActionResult> GetAllNotifications()
        {
            var notifications = await _notificationRepository.GetNotificationsByRoleAsync("All");
            if (notifications == null || !notifications.Any())
            {
                Console.WriteLine("No notifications found for 'All' role.");
                return NotFound(new ApiResponse(404, "No notifications found.", new { IsSuccess = false }));
            }

            return Ok(new ApiResponse(200, "Notifications retrieved successfully.", new { IsSuccess = true, notifications }));
        }

        // Finished / Reviewed / Tested / Edited
        [HttpGet("StudentNotifications")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetStudentNotifications()
        {
            var notifications = await _notificationRepository.GetNotificationsByRoleAsync("Students");
            if (notifications == null || !notifications.Any())
            {
                Console.WriteLine("No notifications found for 'Student' role.");
                return NotFound(new ApiResponse(404, "No notifications found for students.", new { IsSuccess = false }));
            }

            return Ok(new ApiResponse(200, "Student notifications retrieved successfully.", new { IsSuccess = true, notifications }));
        }

        // Finished / Reviewed / Tested / Edited
        [HttpGet("DoctorNotifications")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetDoctorNotifications()
        {
            var notifications = await _notificationRepository.GetNotificationsByRoleAsync("Doctors");
            if (notifications == null || !notifications.Any())
            {
                Console.WriteLine("No notifications found for 'Doctor' role.");
                return NotFound(new ApiResponse(404, "No notifications found for doctors.", new { IsSuccess = false }));
            }

            return Ok(new ApiResponse(200, "Doctor notifications retrieved successfully.", new { IsSuccess = true, notifications }));
        }

        [HttpPut("MarkAsRead")]
        [Authorize(Roles = "Admin, Doctor, Student")]
        public async Task<IActionResult> MarkAsRead([FromBody] UpdateNotificationDto model)
        {
            var notification = await _unitOfWork.Repository<Notification>().FindAsync(n => n.Id == model.NotificationId);
            if (notification == null)
                return NotFound(new ApiResponse(404, "Notification not found.", new { IsSuccess = false }));

            // Determine user role
            if (User.IsInRole("Admin"))
            {
                if (notification.IsReadFromAdmin)
                    return BadRequest(new ApiResponse(400, "Notification is already marked as read for admin.", new { IsSuccess = false }));

                notification.IsReadFromAdmin = true;
            }
            else if (User.IsInRole("Doctor"))
            {
                if (notification.IsReadFromDoctor)
                    return BadRequest(new ApiResponse(400, "Notification is already marked as read for doctor.", new { IsSuccess = false }));

                notification.IsReadFromDoctor = true;
            }
            else if (User.IsInRole("Student"))
            {
                if (notification.IsReadFromStudent)
                    return BadRequest(new ApiResponse(400, "Notification is already marked as read for student.", new { IsSuccess = false }));

                notification.IsReadFromStudent = true;
            }
            else
            {
                return BadRequest(new ApiResponse(400, "Invalid user role.", new { IsSuccess = false }));
            }

            _unitOfWork.Repository<Notification>().Update(notification);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(200, "Notification marked as read successfully.", new { IsSuccess = true }));
        }

        // InProgress
        [HttpDelete("Delete")]
        [Authorize(Roles = "Admin, Doctor, Student")]
        public async Task<IActionResult> DeleteNotification([FromBody] UpdateNotificationDto model)
        {
            var notification = await _unitOfWork.Repository<Notification>().FindAsync(n => n.Id == model.NotificationId);
            if (notification == null)
                return NotFound(new ApiResponse(404, "Notification not found.", new { IsSuccess = false }));

            _unitOfWork.Repository<Notification>().Delete(notification);
            await _unitOfWork.CompleteAsync();

            return Ok(new ApiResponse(200, "Notification deleted successfully.", new { IsSuccess = true }));
        }
    }
}
