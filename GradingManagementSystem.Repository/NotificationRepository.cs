﻿using GradingManagementSystem.Core.DTOs;
using GradingManagementSystem.Core.Entities;
using GradingManagementSystem.Core.Repositories.Contact;
using GradingManagementSystem.Repository.Data.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace GradingManagementSystem.Repository
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        private readonly GradingManagementSystemDbContext _dbContext;

        public NotificationRepository(GradingManagementSystemDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<NotificationResponseDto>> GetNotificationsByRoleAsync(string role)
        {
            if (role == null)
                return Enumerable.Empty<NotificationResponseDto>();

            var activeAppointment = await _dbContext.AcademicAppointments
                .Where(a => a.Status == StatusType.Active.ToString())
                .FirstOrDefaultAsync();
            if (activeAppointment == null)
                return Enumerable.Empty<NotificationResponseDto>();

            var notifications = await _dbContext.Notifications
           .Where(n => n.Role == role && n.AcademicAppointmentId == activeAppointment.Id)
           .OrderByDescending(n => n.SentAt)
           .ToListAsync();
            return notifications.Select(notification => new NotificationResponseDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Description = notification.Description,
                Role = notification.Role,
                IsReadFromAdmin = notification.IsReadFromAdmin,
                IsReadFromDoctor = notification.IsReadFromDoctor,
                IsReadFromStudent = notification.IsReadFromStudent,
                SentAt = notification.SentAt,
                AdminId = notification.AdminId
            });
        }
    }
}
