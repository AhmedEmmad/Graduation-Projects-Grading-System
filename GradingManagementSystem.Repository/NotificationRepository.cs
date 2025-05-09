using GradingManagementSystem.Core.DTOs;
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

            else if (role == NotificationRole.All.ToString())
            {
                var notifications = await _dbContext.Notifications.Where(n => n.Role == NotificationRole.All.ToString()).ToListAsync();
                return notifications.Select(notification => new NotificationResponseDto
                {
                    Id = notification.Id,
                    Title = notification.Title,
                    Description = notification.Description,
                    Role = notification.Role,
                    IsRead = notification.IsRead,
                    SentAt = notification.SentAt,
                    AdminId = notification.AdminId
                });
            }
            else if (role == NotificationRole.Students.ToString())
            {
                var notifications = await _dbContext.Notifications.Where(n => n.Role == NotificationRole.Students.ToString() || n.Role == NotificationRole.All.ToString()).ToListAsync();
                return notifications.Select(notification => new NotificationResponseDto
                {
                    Id = notification.Id,
                    Title = notification.Title,
                    Description = notification.Description,
                    Role = notification.Role,
                    IsRead = notification.IsRead,
                    SentAt = notification.SentAt,
                    AdminId = notification.AdminId
                });
            }
            else 
            {
                var notifications = await _dbContext.Notifications.Where(n => n.Role == NotificationRole.Doctors.ToString() || n.Role == NotificationRole.All.ToString()).ToListAsync();
                return notifications.Select(notification => new NotificationResponseDto
                {
                    Id = notification.Id,
                    Title = notification.Title,
                    Description = notification.Description,
                    Role = notification.Role,
                    IsRead = notification.IsRead,
                    SentAt = notification.SentAt,
                    AdminId = notification.AdminId
                });
            }
        }
    }
}
