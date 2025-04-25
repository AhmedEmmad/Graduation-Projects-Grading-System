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

            else if (!Enum.TryParse(role, ignoreCase: true, out NotificationRole parsedRole))
                return Enumerable.Empty<NotificationResponseDto>();

            else if (role.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                var notifications = await _dbContext.Notifications.Where(N => N.Role == NotificationRole.All).ToListAsync();
                return notifications.Select(notification => new NotificationResponseDto
                {
                    Id = notification.Id,
                    Title = notification.Title,
                    Description = notification.Description,
                    Role = notification.Role.ToString(),
                    IsRead = notification.IsRead,
                    CreatedAt = notification.CreatedAt,
                    AdminId = notification.AdminId
                });
            }
            else if (role.Equals("Students", StringComparison.OrdinalIgnoreCase))
            {
                var notifications = await _dbContext.Notifications.Where(N => N.Role == NotificationRole.Students && N.Role == NotificationRole.All).ToListAsync();
                return notifications.Select(notification => new NotificationResponseDto
                {
                    Id = notification.Id,
                    Title = notification.Title,
                    Description = notification.Description,
                    Role = notification.Role.ToString(),
                    IsRead = notification.IsRead,
                    CreatedAt = notification.CreatedAt,
                    AdminId = notification.AdminId
                });
            }
            else 
            {
                var notifications = await _dbContext.Notifications.Where(N => N.Role == NotificationRole.Doctors && N.Role == NotificationRole.All).ToListAsync();
                return notifications.Select(notification => new NotificationResponseDto
                {
                    Id = notification.Id,
                    Title = notification.Title,
                    Description = notification.Description,
                    Role = notification.Role.ToString(),
                    IsRead = notification.IsRead,
                    CreatedAt = notification.CreatedAt,
                    AdminId = notification.AdminId
                });
            }
        }
    }
}
