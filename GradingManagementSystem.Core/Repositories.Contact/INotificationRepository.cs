using GradingManagementSystem.Core.DTOs;
using GradingManagementSystem.Core.Entities;

namespace GradingManagementSystem.Core.Repositories.Contact
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<IEnumerable<NotificationResponseDto>> GetNotificationsByRoleAsync(string role);
    }
}
